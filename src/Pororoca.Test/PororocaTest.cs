using System.Text;
using System.Threading.Channels;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.ImportCollection;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.RequestRepeater;
using Pororoca.Domain.Features.TranslateRequest.Http;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;
using Pororoca.Domain.Features.VariableResolution;
using Pororoca.Infrastructure.Features.Requester;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepeater;
using static Pororoca.Domain.Features.RequestRepeater.HttpRepetitionValidator;

namespace Pororoca.Test;

public sealed class PororocaTest
{
    private PororocaCollection Collection { get; }
    private bool ShouldCheckTlsCertificate { get; set; }

    private PororocaTest(PororocaCollection col)
    {
        Collection = col;
        ShouldCheckTlsCertificate = true;
    }

    public static PororocaTest LoadCollectionFromFile(string filePath)
    {
        const string loadCollectionFailedMsg = "Error: Pororoca collection file loading failed. Please, check the file.";

        try
        {
            string json = File.ReadAllText(filePath, Encoding.UTF8);
            if (PororocaCollectionImporter.TryImportPororocaCollection(json, preserveId: true, out var col))
            {
                return new(col!);
            }
            else
            {
                throw new Exception(loadCollectionFailedMsg);
            }
        }
        catch (Exception ex)
        {
            throw new Exception(loadCollectionFailedMsg, ex);
        }
    }

    public PororocaTest AndUseTheEnvironment(string environmentName)
    {
        var selectedEnv = Collection.Environments.FirstOrDefault(e => e.Name == environmentName)
            ?? throw new Exception($"Error: Environment with the name '{environmentName}' was not found.");

        var newEnvs = Collection.Environments.Select(e => e with
        {
            IsCurrent = e.Name == environmentName
        }).ToList();

        Collection.Environments.Clear();
        Collection.Environments.AddRange(newEnvs);

        return this;
    }

    public PororocaTest AndDontCheckTlsCertificate()
    {
        ShouldCheckTlsCertificate = false;
        PororocaRequester.Singleton.DisableSslVerification = !ShouldCheckTlsCertificate;
        return this;
    }

    public string? GetCollectionVariable(string key) =>
        Collection.Variables.FirstOrDefault(v => v.Key == key)?.Value;

    public void SetCollectionVariable(string key, string? value)
    {
        var variable = Collection.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            var newVariable = variable with { Value = value };
            Collection.Variables.Remove(variable);
            Collection.Variables.Add(newVariable);
        }
        else
        {
            Collection.Variables.Add(new(true, key, value, false));
        }
    }

    public string? GetEnvironmentVariable(string environmentName, string key)
    {
        var env = Collection.Environments.FirstOrDefault(e => e.Name == environmentName)
            ?? throw new Exception($"Error: Environment with the name '{environmentName}' was not found.");

        return env.Variables.FirstOrDefault(v => v.Key == key)?.Value;
    }

    public void SetEnvironmentVariable(string environmentName, string key, string? value)
    {
        var env = Collection.Environments.FirstOrDefault(e => e.Name == environmentName)
            ?? throw new Exception($"Error: Environment with the name '{environmentName}' was not found.");

        var variable = env.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            var newVariable = variable with { Value = value };
            env.Variables.RemoveAll(v => v.Key == key);
            env.Variables.Add(newVariable);
        }
        else
        {
            env.Variables.Add(new(true, key, value, false));
        }
    }

    public PororocaHttpRequest? FindHttpRequestInCollection(Func<PororocaHttpRequest, bool> criteria) =>
        Collection.FindRequestInCollection(criteria);

    public PororocaWebSocketConnection? FindWebSocketInCollection(Func<PororocaWebSocketConnection, bool> criteria) =>
        Collection.FindRequestInCollection(criteria);

    public PororocaHttpRepetition? FindHttpRepetitionInCollection(Func<PororocaHttpRepetition, bool> criteria) =>
        Collection.FindRequestInCollection(criteria);

    public Task<PororocaHttpResponse> SendHttpRequestAsync(string requestName, CancellationToken cancellationToken = default)
    {
        var req = FindHttpRequestInCollection(r => r.Name == requestName);
        if (req != null)
        {
            return SendHttpRequestAsync(req, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: Request with the name '{requestName}' was not found.");
        }
    }

    public async Task<PororocaHttpResponse> SendHttpRequestAsync(PororocaHttpRequest req, CancellationToken cancellationToken = default)
    {
        var effectiveVars = ((IPororocaVariableResolver)Collection).GetEffectiveVariables();
        if (!PororocaHttpRequestValidator.IsValidRequest(effectiveVars,
                                                         Collection.CollectionScopedAuth,
                                                         req,
                                                         out string? errorCode))
        {
            throw new Exception($"Error: PororocaRequest could not be sent. Cause: '{errorCode}'.");
        }
        else
        {
            var requester = PororocaRequester.Singleton;
            var res = await requester.RequestAsync(effectiveVars, Collection.CollectionScopedAuth, Collection.CollectionScopedRequestHeaders, req, cancellationToken);
            CaptureResponseValues(req, res);
            return res;
        }
    }

    public Task<PororocaTestWebSocketConnector> ConnectWebSocketAsync(string wsName,
                                                                      OnWebSocketConnectionChanged? onConnectionChanged = null,
                                                                      OnWebSocketMessageSending? onMessageSending = null,
                                                                      CancellationToken cancellationToken = default)
    {
        var ws = FindWebSocketInCollection(x => x.Name == wsName);
        if (ws != null)
        {
            return ConnectWebSocketAsync(ws, onConnectionChanged, onMessageSending, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: WebSocket with the name '{wsName}' was not found.");
        }
    }

    public async Task<PororocaTestWebSocketConnector> ConnectWebSocketAsync(PororocaWebSocketConnection ws,
                                                                            OnWebSocketConnectionChanged? onConnectionChanged = null,
                                                                            OnWebSocketMessageSending? onMessageSending = null,
                                                                            CancellationToken cancellationToken = default)
    {
        var effectiveVars = ((IPororocaVariableResolver)Collection).GetEffectiveVariables();

        if (!PororocaWebSocketConnectionValidator.IsValidConnection(effectiveVars, Collection.CollectionScopedAuth, ws, out var resolvedUri, out string? validationErrorCode))
        {
            throw new Exception($"Error: Could not connect to WebSocket. Cause: '{validationErrorCode}'.");
        }
        else if (!PororocaWebSocketConnectionTranslator.TryTranslateConnection(effectiveVars, Collection.CollectionScopedAuth, Collection.CollectionScopedRequestHeaders, PororocaHttpClientProvider.Singleton, ws, !ShouldCheckTlsCertificate, out var wsAndHttpCli, out string? translationErrorCode))
        {
            throw new Exception($"Error: Could not connect to WebSocket. Cause: '{translationErrorCode}'.");
        }
        else
        {
            PororocaTestWebSocketConnector connector = new(Collection, ws, onConnectionChanged, onMessageSending);
            await connector.ConnectAsync(wsAndHttpCli.wsCli!, wsAndHttpCli.httpCli!, resolvedUri!, cancellationToken);
            if (connector.ConnectionException is not null)
            {
                throw connector.ConnectionException;
            }
            else
            {
                return connector;
            }
        }
    }

    public Task<ChannelReader<PororocaHttpRepetitionResult>> StartHttpRepetitionAsync(string repName, CancellationToken cancellationToken = default)
    {
        var rep = FindHttpRepetitionInCollection(r => r.Name == repName);
        if (rep != null)
        {
            return StartHttpRepetitionAsync(rep!, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: Repetition with the name '{repName}' was not found.");
        }
    }

    public async Task<ChannelReader<PororocaHttpRepetitionResult>> StartHttpRepetitionAsync(PororocaHttpRepetition rep, CancellationToken cancellationToken = default)
    {
        var colEffectiveVars = ((IPororocaVariableResolver)Collection).GetEffectiveVariables();
        var baseReq = Collection.GetHttpRequestByPath(rep.BaseRequestPath);
        var (valid, errorCode, resolvedInputData) = await IsValidRepetitionAsync(colEffectiveVars, baseReq, rep, cancellationToken);

        if (!valid)
        {
            throw new Exception($"Error: Repetition could not be started. Cause: '{errorCode}'.");
        }
        else
        {
            var requester = PororocaRequester.Singleton;
            return StartRepetition(requester, colEffectiveVars, resolvedInputData, Collection.CollectionScopedAuth, Collection.CollectionScopedRequestHeaders, rep, baseReq!, cancellationToken);
        }
    }

    private void CaptureResponseValues(PororocaHttpRequest req, PororocaHttpResponse res)
    {
        // if an environment is selected, then save captures into its variables
        // otherwise, save captures into collection variables
        var colVars = Collection.Variables;
        var envVars = Collection.Environments.FirstOrDefault(e => e.IsCurrent)?.Variables;
        var targetVars = envVars ?? colVars;

        var captures = req.ResponseCaptures ?? new();
        foreach (var capture in captures)
        {
            var targetVar = targetVars.FirstOrDefault(v => v.Key == capture.TargetVariable);
            string? capturedValue = res.CaptureValue(capture);
            if (capturedValue is not null)
            {
                if (targetVar is null)
                {
                    targetVar = new(true, capture.TargetVariable, capturedValue, true);
                    targetVars.Add(targetVar);
                }
                else
                {
                    var targetVarUpdated = targetVar with { Value = capturedValue };
                    targetVars.Remove(targetVar);
                    targetVars.Add(targetVarUpdated);
                }
            }
        }
    }
}
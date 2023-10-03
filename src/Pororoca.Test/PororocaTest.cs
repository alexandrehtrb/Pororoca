using System.Text;
using Pororoca.Domain.Features.Common;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.ImportCollection;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest.Http;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;
using Pororoca.Domain.Features.VariableCapture;
using Pororoca.Infrastructure.Features.Requester;

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

        foreach (var env in Collection.Environments)
        {
            env.IsCurrent = env.Name == environmentName;
        }
        return this;
    }

    public PororocaTest AndDontCheckTlsCertificate()
    {
        ShouldCheckTlsCertificate = false;
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
            env.RemoveVariable(key);
            env.AddVariable(newVariable);
        }
        else
        {
            env.AddVariable(new(true, key, value, false));
        }
    }

    public PororocaHttpRequest? FindHttpRequestInCollection(Func<PororocaHttpRequest, bool> criteria)
    {
        static PororocaHttpRequest? FindHttpRequestInFolder(PororocaCollectionFolder folder, Func<PororocaHttpRequest, bool> criteria)
        {
            var reqInFolder = folder.HttpRequests.FirstOrDefault(criteria);
            if (reqInFolder != null)
            {
                return reqInFolder;
            }
            else
            {
                foreach (var subFolder in folder.Folders)
                {
                    var reqInSubfolder = FindHttpRequestInFolder(subFolder, criteria);
                    if (reqInSubfolder != null)
                    {
                        return reqInSubfolder;
                    }
                }
                return null;
            }
        }

        var reqInCol = Collection.HttpRequests.FirstOrDefault(criteria);
        if (reqInCol != null)
        {
            return reqInCol;
        }
        else
        {
            foreach (var folder in Collection.Folders)
            {
                var reqInFolder = FindHttpRequestInFolder(folder, criteria);
                if (reqInFolder != null)
                {
                    return reqInFolder;
                }
            }
            return null;
        }
    }

    public PororocaWebSocketConnection? FindWebSocketInCollection(Func<PororocaWebSocketConnection, bool> criteria)
    {
        static PororocaWebSocketConnection? FindWebSocketInFolder(PororocaCollectionFolder folder, Func<PororocaWebSocketConnection, bool> criteria)
        {
            var wsInFolder = folder.WebSocketConnections.FirstOrDefault(criteria);
            if (wsInFolder != null)
            {
                return wsInFolder;
            }
            else
            {
                foreach (var subFolder in folder.Folders)
                {
                    var wsInSubfolder = FindWebSocketInFolder(subFolder, criteria);
                    if (wsInSubfolder != null)
                    {
                        return wsInSubfolder;
                    }
                }
                return null;
            }
        }

        var wsInCol = Collection.WebSocketConnections.FirstOrDefault(criteria);
        if (wsInCol != null)
        {
            return wsInCol;
        }
        else
        {
            foreach (var folder in Collection.Folders)
            {
                var wsInFolder = FindWebSocketInFolder(folder, criteria);
                if (wsInFolder != null)
                {
                    return wsInFolder;
                }
            }
            return null;
        }
    }

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
        if (!PororocaHttpRequestValidator.IsValidRequest(Collection,
                                                         req,
                                                         out string? errorCode))
        {
            throw new Exception($"Error: PororocaRequest could not be sent. Cause: '{errorCode}'.");
        }
        else
        {
            IPororocaRequester requester = PororocaRequester.Singleton;
            var res = await requester.RequestAsync(Collection, req, !ShouldCheckTlsCertificate, cancellationToken);
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
        if (!PororocaWebSocketConnectionValidator.IsValidConnection(Collection, ws, out var resolvedUri, out string? validationErrorCode))
        {
            throw new Exception($"Error: Could not connect to WebSocket. Cause: '{validationErrorCode}'.");
        }
        else if (!PororocaWebSocketConnectionTranslator.TryTranslateConnection(Collection, PororocaHttpClientProvider.Singleton, ws, !ShouldCheckTlsCertificate, out var wsAndHttpCli, out string? translationErrorCode))
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

    private void CaptureResponseValues(PororocaHttpRequest req, PororocaHttpResponse res)
    {
        string? CaptureValue(PororocaHttpResponseValueCapture capture)
        {
            if (capture.Type == PororocaHttpResponseValueCaptureType.Header)
            {
                return res.Headers?.FirstOrDefault(x => x.Key == capture.HeaderName).Value;
            }
            else if (capture.Type == PororocaHttpResponseValueCaptureType.Body)
            {
                bool isJsonBody = MimeTypesDetector.IsJsonContent(res.ContentType ?? string.Empty);
                bool isXmlBody = MimeTypesDetector.IsXmlContent(res.ContentType ?? string.Empty);
                if (isJsonBody)
                {
                    return PororocaResponseValueCapturer.CaptureJsonValue(capture.Path!, res.GetBodyAsText()!);
                }
                else if (isXmlBody)
                {
                    return PororocaResponseValueCapturer.CaptureXmlValue(capture.Path!, res.GetBodyAsText()!);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        var env = Collection.Environments.FirstOrDefault(e => e.IsCurrent);
        if (env is not null)
        {
            var captures = req.ResponseCaptures ?? new();
            foreach (var capture in captures)
            {
                var envVar = env.Variables.FirstOrDefault(v => v.Key == capture.TargetVariable);
                string? capturedValue = CaptureValue(capture);
                if (capturedValue is not null)
                {
                    if (envVar is null)
                    {
                        envVar = new(true, capture.TargetVariable, capturedValue, true);
                        env.Variables.Add(envVar);
                    }
                    else
                    {
                        var newEnvVar = envVar with { Value = capturedValue };
                        env.Variables.Remove(envVar);
                        env.Variables.Add(newEnvVar);
                    }
                }
            }
        }
    }
}
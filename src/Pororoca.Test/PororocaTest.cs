using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.ImportCollection;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.TranslateRequest.Http;
using Pororoca.Domain.Features.TranslateRequest.WebSockets.Connection;
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
            if (PororocaCollectionImporter.TryImportPororocaCollection(json, out var col))
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
        var selectedEnv = Collection.Environments.FirstOrDefault(e => e.Name == environmentName);
        if (selectedEnv == null)
        {
            throw new Exception($"Error: Environment with the name '{environmentName}' was not found.");
        }

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

    public void SetCollectionVariable(string key, string? value)
    {
        var variable = Collection.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            variable.Value = value;
        }
        else
        {
            Collection.AddVariable(new(true, key, value, false));
        }
    }

    public void SetEnvironmentVariable(string environmentName, string key, string? value)
    {
        var env = Collection.Environments.FirstOrDefault(e => e.Name == environmentName);
        if (env is null)
        {
            throw new Exception($"Error: Environment with the name '{environmentName}' was not found.");
        }

        var variable = env.Variables.FirstOrDefault(v => v.Key == key);
        if (variable != null)
        {
            variable.Value = value;
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

    public Task<PororocaHttpResponse> SendHttpRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var req = FindHttpRequestInCollection(r => r.Id == requestId);
        if (req != null)
        {
            return SendHttpRequestAsync(req, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: Request with the ID '{requestId}' was not found.");
        }
    }

    public Task<PororocaHttpResponse> SendHttpRequestAsync(PororocaHttpRequest req, CancellationToken cancellationToken = default)
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
            return requester.RequestAsync(Collection, req, !ShouldCheckTlsCertificate, cancellationToken);
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

    public Task<PororocaTestWebSocketConnector> ConnectWebSocketAsync(Guid wsId,
                                                                      OnWebSocketConnectionChanged? onConnectionChanged = null,
                                                                      OnWebSocketMessageSending? onMessageSending = null,
                                                                      CancellationToken cancellationToken = default)
    {
        var ws = FindWebSocketInCollection(x => x.Id == wsId);
        if (ws != null)
        {
            return ConnectWebSocketAsync(ws, onConnectionChanged, onMessageSending, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: WebSocket with the ID '{wsId}' was not found.");
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


}
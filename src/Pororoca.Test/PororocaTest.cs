using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.ImportCollection;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
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

    public Task<PororocaHttpResponse> SendRequestAsync(string requestName, CancellationToken cancellationToken = default)
    {
        var req = FindHttpRequestInCollection(r => r.Name == requestName);
        if (req != null)
        {
            return SendRequestAsync(req, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: Request with the name '{requestName}' was not found.");
        }
    }

    public Task<PororocaHttpResponse> SendRequestAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var req = FindHttpRequestInCollection(r => r.Id == requestId);
        if (req != null)
        {
            return SendRequestAsync(req, cancellationToken);
        }
        else
        {
            throw new Exception($"Error: Request with the ID '{requestId}' was not found.");
        }
    }

    public Task<PororocaHttpResponse> SendRequestAsync(PororocaHttpRequest req, CancellationToken cancellationToken = default)
    {
        if (!PororocaHttpRequestTranslator.IsValidRequest(Collection,
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
}
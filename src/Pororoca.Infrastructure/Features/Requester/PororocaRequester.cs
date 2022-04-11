using System.Diagnostics;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Infrastructure.Features.Requester;

public sealed class PororocaRequester : IPororocaRequester
{
    public static readonly PororocaRequester Singleton;

    static PororocaRequester()
    {
        Singleton = new();
    }

    private PororocaRequester()
    {
    }

    public async Task<PororocaResponse> RequestAsync(IPororocaVariableResolver variableResolver, PororocaRequest req, bool disableSslVerification, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage? reqMsg = null;
        HttpResponseMessage resMsg;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            if (!PororocaRequestTranslator.TryTranslateRequest(variableResolver, req, out reqMsg, out string? errorCode))
            {
                reqMsg?.Dispose();
                sw.Stop();
                return PororocaResponse.Failed(sw.Elapsed, new Exception("Invalid request. Please, check the resolved URL and the HTTP version compatibility."));
            }
            else
            {
                HttpClient httpClient = PororocaHttpClientProvider.Provide(disableSslVerification, reqMsg!);

                resMsg = await httpClient.SendAsync(reqMsg!, cancellationToken);
                reqMsg?.Dispose();
                sw.Stop();
                return await PororocaResponse.SuccessfulAsync(sw.Elapsed, resMsg);
            }
        }
        catch (Exception ex)
        {
            reqMsg?.Dispose();
            sw.Stop();
            return PororocaResponse.Failed(sw.Elapsed, ex);
        }
    }

    public bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaRequest req, out string? errorCode) =>
        PororocaRequestTranslator.IsValidRequest(variableResolver, req, out errorCode);
}
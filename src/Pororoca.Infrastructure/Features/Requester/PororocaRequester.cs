using System.Diagnostics;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest.Http;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Infrastructure.Features.Requester;

public sealed class PororocaRequester : IPororocaRequester
{
    public static readonly PororocaRequester Singleton;

    static PororocaRequester() => Singleton = new();

    private PororocaRequester()
    {
    }

    public async Task<PororocaHttpResponse> RequestAsync(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, bool disableSslVerification, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage? reqMsg = null;
        HttpResponseMessage resMsg;
        Stopwatch sw = new();
        sw.Start();
        try
        {
            if (!PororocaHttpRequestTranslator.TryTranslateRequest(variableResolver, req, out reqMsg, out string? errorCode))
            {
                reqMsg?.Dispose();
                sw.Stop();
                return PororocaHttpResponse.Failed(sw.Elapsed, new Exception("Invalid request. Please, check the resolved URL and the HTTP version compatibility."));
            }
            else
            {
                var httpClient = PororocaHttpClientProvider.Provide(disableSslVerification, reqMsg!);

                resMsg = await httpClient.SendAsync(reqMsg!, cancellationToken);
                reqMsg?.Dispose();
                sw.Stop();
                return await PororocaHttpResponse.SuccessfulAsync(sw.Elapsed, resMsg);
            }
        }
        catch (Exception ex)
        {
            reqMsg?.Dispose();
            sw.Stop();
            return PororocaHttpResponse.Failed(sw.Elapsed, ex);
        }
    }

    public bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, out string? errorCode) =>
        PororocaHttpRequestValidator.IsValidRequest(variableResolver, req, out errorCode);
}
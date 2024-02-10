using System.Diagnostics;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest.Http;

namespace Pororoca.Infrastructure.Features.Requester;

public sealed class PororocaRequester : IPororocaRequester
{
    public static readonly PororocaRequester Singleton = new();

    public bool DisableSslVerification { get; set; }

    private PororocaRequester()
    {
    }

    public async Task<PororocaHttpResponse> RequestAsync(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, CancellationToken cancellationToken = default)
    {
        PororocaHttpRequest? resolvedReq = null;
        HttpRequestMessage? reqMsg = null;
        HttpResponseMessage? resMsg;
        Stopwatch sw = new();
        DateTimeOffset? startedAt = null;
        try
        {
            if (!PororocaHttpRequestTranslator.TryTranslateRequest(effectiveVars, collectionScopedAuth, req, out resolvedReq, out reqMsg, out string? errorCode))
            {
                reqMsg?.Dispose();
                return PororocaHttpResponse.Failed(resolvedReq, DateTimeOffset.Now, TimeSpan.Zero, new Exception("Invalid request. Please, check the resolved URL and the HTTP version compatibility."));
            }
            else
            {
                var resolvedAuth = GetResolvedAuth(reqMsg!);
                var httpClient = PororocaHttpClientProvider.Singleton.Provide(DisableSslVerification, resolvedAuth);
                startedAt = DateTimeOffset.Now;
                sw.Start();
                resMsg = await httpClient.SendAsync(reqMsg!, cancellationToken);
                sw.Stop();
                reqMsg?.Dispose();
                var res = await PororocaHttpResponse.SuccessfulAsync(resolvedReq!, (DateTimeOffset)startedAt!, sw.Elapsed, resMsg);
                // PororocaHttpResponse.SuccessfulAsync uses HttpResponseMessage, so we need to dispose it only after line above
                resMsg?.Dispose();
                return res;
            }
        }
        catch (Exception ex)
        {
            startedAt ??= DateTimeOffset.Now;
            sw.Stop();
            reqMsg?.Dispose();
            return PororocaHttpResponse.Failed(resolvedReq, (DateTimeOffset)startedAt!, sw.Elapsed, ex);
        }
    }

    public bool IsValidRequest(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, out string? errorCode) =>
        PororocaHttpRequestValidator.IsValidRequest(effectiveVars, collectionScopedAuth, req, out errorCode);

    private static PororocaRequestAuth? GetResolvedAuth(HttpRequestMessage reqMsg)
    {
        object? obj = reqMsg.Options.FirstOrDefault(o => o.Key == PororocaHttpRequestTranslator.AuthOptionsKey).Value;
        if (obj is PororocaRequestAuth resolvedAuth)
        {
            return resolvedAuth;
        }
        else
        {
            return null;
        }
    }
}
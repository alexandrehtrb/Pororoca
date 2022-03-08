using System.Diagnostics;
using System.Net;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Requester;
using Pororoca.Domain.Features.TranslateRequest;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Infrastructure.Features.Requester;

public sealed class PororocaRequester : IPororocaRequester
{
    private static readonly HttpClient _httpClientWithoutSslVerification;
    private static readonly HttpClient _httpClientWithSslVerification;

    public static readonly PororocaRequester Singleton;

    static PororocaRequester()
    {
        TimeSpan timeout = TimeSpan.FromMinutes(5);

        HttpClientHandler handlerNoSslVerification = new()
        {
            AutomaticDecompression = DecompressionMethods.All,
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslErrors) => true
        };

        HttpClientHandler handlerWithSslVerification = new()
        {
            AutomaticDecompression = DecompressionMethods.All
        };

        _httpClientWithoutSslVerification = new(handlerNoSslVerification)
        {
            Timeout = timeout
        };

        _httpClientWithSslVerification = new(handlerWithSslVerification)
        {
            Timeout = timeout
        };

        Singleton = new();
    }

    private PororocaRequester()
    {
    }

    public async Task<PororocaResponse> RequestAsync(IPororocaVariableResolver variableResolver, PororocaRequest req, bool disableSslVerification, CancellationToken cancellationToken = default)
    {
        HttpRequestMessage? reqMsg = null;
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
                HttpResponseMessage resMsg;                
                if (disableSslVerification)
                {
                    resMsg = await _httpClientWithoutSslVerification.SendAsync(reqMsg!, cancellationToken);
                }
                else
                {
                    resMsg = await _httpClientWithSslVerification.SendAsync(reqMsg!, cancellationToken);
                }
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
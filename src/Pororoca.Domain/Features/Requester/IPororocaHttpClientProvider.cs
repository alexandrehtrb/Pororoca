using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaHttpClientProvider
{
    HttpClient Provide(bool disableSslVerification, PororocaRequestAuthClientCertificate? resolvedCert);
}
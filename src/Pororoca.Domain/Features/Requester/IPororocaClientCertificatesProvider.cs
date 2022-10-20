using System.Security.Cryptography.X509Certificates;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaClientCertificatesProvider
{
    X509Certificate2 Provide(PororocaRequestAuthClientCertificate resolvedClientCert);
}
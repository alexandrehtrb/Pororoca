using System.Security.Cryptography.X509Certificates;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Infrastructure.Features.Requester;

internal static class PororocaClientCertificatesProvider
{

    internal static X509Certificate2 Provide(PororocaRequestAuthClientCertificate resolvedClientCert) =>
        resolvedClientCert.Type switch
        {
            PororocaRequestAuthClientCertificateType.Pkcs12 => LoadPkcs12CertificateFromFile(resolvedClientCert),
            PororocaRequestAuthClientCertificateType.Pem => LoadPemCertificateFromFile(resolvedClientCert),
            _ => throw new Exception("Unknown certificate type")
        };

    // I) PKCS#12 certificates
    // .pfx and .p12 certificates include the private key and are password-protected files.
    private static X509Certificate2 LoadPkcs12CertificateFromFile(PororocaRequestAuthClientCertificate cc) =>
        new(cc.CertificateFilePath, cc.FilePassword);

    // II) PEM certificates:
    // .cer, .crt, .pem certificates may include the private key and are Base64 clear-text files;
    // a .key file holds a private key in some cases.
    // pathToKey is only necessary if the certificate file does not have the private key inside
    // PEM certificates may be encrypted. In this scenario, the key file
    // will have the "ENCRYPTED PRIVATE KEY" label inside.
    private static X509Certificate2 LoadPemCertificateFromFile(PororocaRequestAuthClientCertificate cc)
    {
        bool hasPassword = !string.IsNullOrWhiteSpace(cc.FilePassword);

        X509Certificate2 pemCert = hasPassword ?
            X509Certificate2.CreateFromEncryptedPemFile(cc.CertificateFilePath, cc.FilePassword, cc.PrivateKeyFilePath) :
            X509Certificate2.CreateFromPemFile(cc.CertificateFilePath, cc.PrivateKeyFilePath);
        
        // IMPORTANT: Most Windows versions cannot use PEM certificates:
        // https://github.com/dotnet/runtime/issues/23749#issuecomment-747407051
        // We need to convert PEM certificate to PKCS#12 certificate before using it on Windows    
        // work around for Windows (WinApi) problems with PEMS, still in .NET 5
        if (OperatingSystem.IsWindows())
        {
            return new(pemCert.Export(X509ContentType.Pkcs12));
        }
        else
        {
            return pemCert;
        }
    }
}
using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public enum PororocaRequestAuthClientCertificateType
{
    Pkcs12,
    Pem
}

public sealed record PororocaRequestAuthClientCertificate
(
    [property: JsonInclude] PororocaRequestAuthClientCertificateType Type,
    [property: JsonInclude] string CertificateFilePath,
    [property: JsonInclude] string? PrivateKeyFilePath,
    [property: JsonInclude] string? FilePassword
)
{
    // Parameterless constructor for JSON deserialization
    public PororocaRequestAuthClientCertificate() : this(PororocaRequestAuthClientCertificateType.Pem, string.Empty, string.Empty, null) { }

    public PororocaRequestAuthClientCertificate Copy() => this with { };
}
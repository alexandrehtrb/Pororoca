using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaRequestAuthClientCertificate : ICloneable
{
    [JsonInclude]
    public PororocaRequestAuthClientCertificateType Type { get; private set; }

    [JsonInclude]
    public string CertificateFilePath { get; private set; }

    [JsonInclude]
    public string? PrivateKeyFilePath { get; private set; }

    [JsonInclude]
    public string? FilePassword { get; private set; }

#nullable disable warnings
    public PororocaRequestAuthClientCertificate() : this(PororocaRequestAuthClientCertificateType.Pem, string.Empty, string.Empty, null)
    {
        // Parameterless constructor for JSON deserialization
    }
#nullable restore warnings

    public PororocaRequestAuthClientCertificate(PororocaRequestAuthClientCertificateType type, string certificateFilePath, string? privateKeyFilePath, string? filePassword)
    {
        Type = type;
        CertificateFilePath = certificateFilePath;
        PrivateKeyFilePath = privateKeyFilePath;
        FilePassword = filePassword;
    }

    public object Clone() =>
        new PororocaRequestAuthClientCertificate()
        {
            Type = Type,
            CertificateFilePath = CertificateFilePath,
            PrivateKeyFilePath = PrivateKeyFilePath,
            FilePassword = FilePassword
        };
}
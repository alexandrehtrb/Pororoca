using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public enum PororocaRequestAuthMode
{
    Basic,
    Bearer,
    ClientCertificate
}

public sealed record PororocaRequestAuth
(
    [property: JsonInclude] PororocaRequestAuthMode Mode,
    [property: JsonInclude] string? BasicAuthLogin,
    [property: JsonInclude] string? BasicAuthPassword,
    [property: JsonInclude] string? BearerToken,
    [property: JsonInclude] PororocaRequestAuthClientCertificate? ClientCertificate
)
{
    // Parameterless constructor for JSON deserialization
    public PororocaRequestAuth() : this(PororocaRequestAuthMode.Basic, null, null, null, null) { }

    public static PororocaRequestAuth MakeBasicAuth(string basicAuthLogin, string basicAuthPassword) => new(
        PororocaRequestAuthMode.Basic,
        basicAuthLogin,
        basicAuthPassword,
        null,
        null);

    public static PororocaRequestAuth MakeBearerAuth(string bearerToken) => new(
        PororocaRequestAuthMode.Bearer,
        null,
        null,
        bearerToken,
        null);

    public static PororocaRequestAuth MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType type, string certFilePath, string? keyFilePath, string? filePassword)
    {
        string? nulledKeyFilePath = string.IsNullOrWhiteSpace(keyFilePath) ? null : keyFilePath;
        string? nulledFilePassword = string.IsNullOrWhiteSpace(filePassword) ? null : filePassword;
        PororocaRequestAuthClientCertificate cc = new(type, certFilePath, nulledKeyFilePath, nulledFilePassword);
        return new(PororocaRequestAuthMode.ClientCertificate, null, null, null, cc);
    }

    public PororocaRequestAuth Copy() => this with
    {
        ClientCertificate = ClientCertificate?.Copy()
    };
}
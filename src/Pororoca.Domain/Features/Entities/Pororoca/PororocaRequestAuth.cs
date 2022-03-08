using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.Pororoca;

public sealed class PororocaRequestAuth : ICloneable
{
    [JsonInclude]
    public PororocaRequestAuthMode Mode { get; private set; }

    [JsonInclude]
    public string? BasicAuthLogin { get; private set; }

    [JsonInclude]
    public string? BasicAuthPassword { get; private set; }

    [JsonInclude]
    public string? BearerToken { get; private set; }

    #nullable disable warnings
    public PororocaRequestAuth() : this(PororocaRequestAuthMode.Basic)
    {
        // Parameterless constructor for JSON deserialization
    }
    #nullable restore warnings

    public PororocaRequestAuth(PororocaRequestAuthMode authMode)
    {
        Mode = authMode;
        BasicAuthLogin = null;
        BasicAuthPassword = null;
        BearerToken = null;
    }

    public void SetBasicAuth(string basicAuthLogin, string basicAuthPassword)
    {
        Mode = PororocaRequestAuthMode.Basic;
        BasicAuthLogin = basicAuthLogin;
        BasicAuthPassword = basicAuthPassword;
    }

    public void SetBearerAuth(string bearerToken)
    {
        Mode = PororocaRequestAuthMode.Bearer;
        BearerToken = bearerToken;
    }

    public object Clone() =>
        new PororocaRequestAuth(Mode)
        {
            BasicAuthLogin = BasicAuthLogin,
            BasicAuthPassword = BasicAuthPassword,
            BearerToken = BearerToken
        };
}
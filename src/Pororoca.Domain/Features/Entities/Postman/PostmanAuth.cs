#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman
{
    public enum PostmanAuthType
    {
        // TODO: Rename enum values according to C# style convention,
        // but preserving JSON serialization and deserialization
        noauth,
        basic,
        oauth1,
        oauth2,
        bearer,
        digest,
        apikey,
        awsv4, 
        hawk, 
        ntlm
    }

    public class PostmanAuth
    {
        public PostmanAuthType Type { get; set; }

        public PostmanVariable[]? Apikey { get; set; }

        public PostmanVariable[]? Awsv4 { get; set; }

        public PostmanVariable[]? Basic { get; set; }

        public PostmanVariable[]? Bearer { get; set; }

        public PostmanVariable[]? Digest { get; set; }

        public PostmanVariable[]? Hawk { get; set; }

        public PostmanVariable[]? Ntlm { get; set; }

        public PostmanVariable[]? Oauth1 { get; set; }

        public PostmanVariable[]? Oauth2 { get; set; }
    }
}

#nullable enable warnings
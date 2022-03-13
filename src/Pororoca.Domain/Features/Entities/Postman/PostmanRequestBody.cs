#nullable disable warnings

namespace Pororoca.Domain.Features.Entities.Postman
{
    public enum PostmanRequestBodyMode
    {
        Raw,
        Urlencoded,
        Formdata,
        File,
        Graphql
    }

    public class PostmanRequestBody
    {
        public PostmanRequestBodyMode Mode { get; set; }

        public string? Raw { get; set; }

        public PostmanRequestBodyOptions? Options { get; set; }

        public PostmanRequestBodyFormDataParam[]? Formdata { get; set; }

        public PostmanVariable[]? Urlencoded { get; set; }

        public PostmanRequestBodyFileAttachment? File { get; set; }

        public PostmanRequestBodyGraphQl? Graphql { get; set; }
    }
}

#nullable enable warnings
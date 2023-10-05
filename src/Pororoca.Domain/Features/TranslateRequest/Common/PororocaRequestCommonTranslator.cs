using System.Collections.Immutable;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.Common;

public static class PororocaRequestCommonTranslator
{
    private static readonly ImmutableList<string> acceptedUriSchemes = ImmutableList.Create<string>(
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeWs,
        Uri.UriSchemeWss
    );

    #region REQUEST URL

    public static bool TryResolveRequestUri(IPororocaVariableResolver variableResolver, string rawUrl, out Uri? uri, out string? errorCode)
    {
        string? resolvedRawUri = variableResolver.ReplaceTemplates(rawUrl);
        bool validUri = Uri.TryCreate(resolvedRawUri, UriKind.Absolute, out var parsedUri);
        bool validScheme = validUri && acceptedUriSchemes.Contains(parsedUri!.Scheme);
        bool success = validUri && validScheme;
        errorCode = success ? null : TranslateRequestErrors.InvalidUrl;
        uri = success ? parsedUri : null;
        return success;
    }

    #endregion

    #region HTTP VERSION

    internal static Version ResolveHttpVersion(decimal httpVersion) =>
        new((int)httpVersion, (int)(httpVersion * 10) % 10);

    #endregion

    #region HTTP HEADERS

    internal static bool IsContentHeader(string headerName) =>
        headerName == "Allow"
     || headerName == "Content-Disposition"
     || headerName == "Content-Encoding"
     || headerName == "Content-Language"
     || headerName == "Content-Length"
     || headerName == "Content-Location"
     || headerName == "Content-MD5"
     || headerName == "Content-Range"
     || headerName == "Content-Type"
     || headerName == "Expires"
     || headerName == "Last-Modified";

    internal static IDictionary<string, string> ResolveContentHeaders(IPororocaVariableResolver variableResolver, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders) =>
        ResolveHeaders(variableResolver, unresolvedHeaders, IsContentHeader);

    internal static IDictionary<string, string> ResolveNonContentHeaders(IPororocaVariableResolver variableResolver, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders, PororocaRequestAuth? customAuth)
    {
        var nonContentHeaders = ResolveHeaders(variableResolver, unresolvedHeaders, headerName => !IsContentHeader(headerName));

        string? customAuthHeaderValue = ResolveCustomAuthHeaderValue(variableResolver, customAuth);
        if (customAuthHeaderValue != null)
        {
            // Custom auth overrides declared authorization header
            nonContentHeaders["Authorization"] = customAuthHeaderValue;
        }

        return nonContentHeaders;
    }

    private static IDictionary<string, string> ResolveHeaders(IPororocaVariableResolver variableResolver, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders, Func<string, bool> headerNameCriteria) =>
        unresolvedHeaders == null ?
        new() :
        variableResolver
           .ResolveKeyValueParams(unresolvedHeaders)
           .Where(h => h.Key != "Content-Type" && headerNameCriteria(h.Key)) // Content-Type header is set by the content, later
           .ToDictionary(h => h.Key, h => h.Value);

    private static string? ResolveCustomAuthHeaderValue(IPororocaVariableResolver variableResolver, PororocaRequestAuth? customAuth)
    {
        string? MakeBasicAuthToken()
        {
            string? resolvedLogin = variableResolver.ReplaceTemplates(customAuth.BasicAuthLogin!);
            string? resolvedPassword = variableResolver.ReplaceTemplates(customAuth.BasicAuthPassword!);
            string basicAuthTkn = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resolvedLogin}:{resolvedPassword}"));
            return basicAuthTkn;
        }

        string? MakeBearerAuthToken()
        {
            string? resolvedBearerToken = variableResolver.ReplaceTemplates(customAuth.BearerToken!);
            return resolvedBearerToken;
        }

        return customAuth?.Mode switch
        {
            PororocaRequestAuthMode.Bearer => $"Bearer {MakeBearerAuthToken()}",
            PororocaRequestAuthMode.Basic => $"Basic {MakeBasicAuthToken()}",
            _ => null
        };
    }

    #endregion

    #region AUTH

    internal static PororocaRequestAuth? ResolveRequestAuth(IPororocaVariableResolver variableResolver, PororocaRequestAuth? auth)
    {
        if (auth is null)
            return null;

        var resolvedClientCert = ResolveClientCertificate(variableResolver, auth.ClientCertificate);
        var resolvedWindowsAuth = ResolveWindowsAuth(variableResolver, auth.Windows);

        if (resolvedClientCert is null && resolvedWindowsAuth is null)
            return null;

        // not bothering with resolving Basic and Bearer here,
        // they are not relevant for HttpClient configuration
        return new()
        {
            Mode = auth.Mode,
            Windows = resolvedWindowsAuth,
            ClientCertificate = resolvedClientCert
        };
    }

    #endregion

    #region CLIENT CERTIFICATES

    internal static PororocaRequestAuthClientCertificate? ResolveClientCertificate(IPororocaVariableResolver variableResolver, PororocaRequestAuthClientCertificate? unresolvedClientCertificate)
    {
        if (unresolvedClientCertificate == null)
            return null;

        string resolvedPrivateKeyFilePath = variableResolver.ReplaceTemplates(unresolvedClientCertificate.PrivateKeyFilePath);
        string resolvedFilePassword = variableResolver.ReplaceTemplates(unresolvedClientCertificate.FilePassword);

        string? nulledPrivateKeyFilePath = string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath) ?
                                           null : resolvedPrivateKeyFilePath;
        string? nulledFilePassword = string.IsNullOrWhiteSpace(resolvedFilePassword) ?
                                     null : resolvedFilePassword;

        return new(
            Type: unresolvedClientCertificate.Type,
            CertificateFilePath: variableResolver.ReplaceTemplates(unresolvedClientCertificate.CertificateFilePath),
            PrivateKeyFilePath: nulledPrivateKeyFilePath,
            FilePassword: nulledFilePassword
        );
    }

    #endregion

    #region WINDOWS AUTHENTICATION

    internal static PororocaRequestAuthWindows? ResolveWindowsAuth(IPororocaVariableResolver variableResolver, PororocaRequestAuthWindows? unresolvedWinAuth)
    {
        if (unresolvedWinAuth is null)
            return null;
        else if (unresolvedWinAuth.UseCurrentUser)
            return new(true, null, null, null);
        else return new(UseCurrentUser: false,
                        Login: variableResolver.ReplaceTemplates(unresolvedWinAuth.Login),
                        Password: variableResolver.ReplaceTemplates(unresolvedWinAuth.Password),
                        Domain: variableResolver.ReplaceTemplates(unresolvedWinAuth.Domain));
    }

    #endregion
}
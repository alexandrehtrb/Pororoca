using System.Collections.Immutable;
using System.Text;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.TranslateRequest.Common;

public static class PororocaRequestCommonTranslator
{
    internal static PororocaRequestAuth? ChooseRequestAuth(PororocaRequestAuth? collectionScopedAuth, PororocaRequestAuth? reqAuth) =>
        reqAuth?.Mode == PororocaRequestAuthMode.InheritFromCollection ?
        collectionScopedAuth : reqAuth;

    private static readonly ImmutableList<string> acceptedUriSchemes = ImmutableList.Create<string>(
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeWs,
        Uri.UriSchemeWss
    );

    #region REQUEST URL

    public static bool TryResolveRequestUri(IEnumerable<PororocaVariable> effectiveVars, string rawUrl, out Uri? uri, out string? errorCode)
    {
        string? resolvedRawUri = IPororocaVariableResolver.ReplaceTemplates(rawUrl, effectiveVars);
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

    internal static IDictionary<string, string> ResolveContentHeaders(IEnumerable<PororocaVariable> effectiveVars, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders) =>
        ResolveHeaders(effectiveVars, unresolvedHeaders, IsContentHeader);

    internal static IDictionary<string, string> ResolveNonContentHeaders(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaRequestAuth? reqAuth, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders)
    {
        var nonContentHeaders = ResolveHeaders(effectiveVars, unresolvedHeaders, headerName => !IsContentHeader(headerName));

        string? customAuthHeaderValue = ResolveCustomAuthHeaderValue(effectiveVars, collectionScopedAuth, reqAuth);
        if (customAuthHeaderValue != null)
        {
            // Custom auth overrides declared authorization header
            nonContentHeaders["Authorization"] = customAuthHeaderValue;
        }

        return nonContentHeaders;
    }

    private static IDictionary<string, string> ResolveHeaders(IEnumerable<PororocaVariable> effectiveVars, IReadOnlyList<PororocaKeyValueParam>? unresolvedHeaders, Func<string, bool> headerNameCriteria) =>
        unresolvedHeaders == null ?
        new() :
        IPororocaVariableResolver
           .ResolveKeyValueParams(unresolvedHeaders, effectiveVars)
           .Where(h => h.Key != "Content-Type" && headerNameCriteria(h.Key)) // Content-Type header is set by the content, later
           .ToDictionary(h => h.Key, h => h.Value);

    private static string? ResolveCustomAuthHeaderValue(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaRequestAuth? reqAuth)
    {
        string? MakeBasicAuthToken(PororocaRequestAuth auth)
        {
            string? resolvedLogin = IPororocaVariableResolver.ReplaceTemplates(auth.BasicAuthLogin!, effectiveVars);
            string? resolvedPassword = IPororocaVariableResolver.ReplaceTemplates(auth.BasicAuthPassword!, effectiveVars);
            string basicAuthTkn = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resolvedLogin}:{resolvedPassword}"));
            return basicAuthTkn;
        }

        string? MakeBearerAuthToken(PororocaRequestAuth auth)
        {
            string? resolvedBearerToken = IPororocaVariableResolver.ReplaceTemplates(auth.BearerToken!, effectiveVars);
            return resolvedBearerToken;
        }

        var authToUse = ChooseRequestAuth(collectionScopedAuth, reqAuth);

        return authToUse?.Mode switch
        {
            PororocaRequestAuthMode.Bearer => $"Bearer {MakeBearerAuthToken(authToUse)}",
            PororocaRequestAuthMode.Basic => $"Basic {MakeBasicAuthToken(authToUse)}",
            _ => null
        };
    }

    #endregion

    #region AUTH

    internal static PororocaRequestAuth? ResolveRequestAuth(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaRequestAuth? reqAuth)
    {
        var authToUse = ChooseRequestAuth(collectionScopedAuth, reqAuth);

        if (authToUse is null)
            return null;

        var resolvedClientCert = ResolveClientCertificate(effectiveVars, authToUse.ClientCertificate);
        var resolvedWindowsAuth = ResolveWindowsAuth(effectiveVars, authToUse.Windows);

        if (resolvedClientCert is null && resolvedWindowsAuth is null)
            return null;

        // not bothering with resolving Basic and Bearer here,
        // they are not relevant for HttpClient configuration
        return new()
        {
            Mode = authToUse.Mode,
            Windows = resolvedWindowsAuth,
            ClientCertificate = resolvedClientCert
        };
    }

    #endregion

    #region CLIENT CERTIFICATES

    internal static PororocaRequestAuthClientCertificate? ResolveClientCertificate(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuthClientCertificate? unresolvedClientCertificate)
    {
        if (unresolvedClientCertificate == null)
            return null;

        string resolvedPrivateKeyFilePath = IPororocaVariableResolver.ReplaceTemplates(unresolvedClientCertificate.PrivateKeyFilePath, effectiveVars);
        string resolvedFilePassword = IPororocaVariableResolver.ReplaceTemplates(unresolvedClientCertificate.FilePassword, effectiveVars);

        string? nulledPrivateKeyFilePath = string.IsNullOrWhiteSpace(resolvedPrivateKeyFilePath) ?
                                           null : resolvedPrivateKeyFilePath;
        string? nulledFilePassword = string.IsNullOrWhiteSpace(resolvedFilePassword) ?
                                     null : resolvedFilePassword;

        return new(
            Type: unresolvedClientCertificate.Type,
            CertificateFilePath: IPororocaVariableResolver.ReplaceTemplates(unresolvedClientCertificate.CertificateFilePath, effectiveVars),
            PrivateKeyFilePath: nulledPrivateKeyFilePath,
            FilePassword: nulledFilePassword
        );
    }

    #endregion

    #region WINDOWS AUTHENTICATION

    internal static PororocaRequestAuthWindows? ResolveWindowsAuth(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuthWindows? unresolvedWinAuth)
    {
        if (unresolvedWinAuth is null)
            return null;
        else if (unresolvedWinAuth.UseCurrentUser)
            return new(true, null, null, null);
        else return new(UseCurrentUser: false,
                        Login: IPororocaVariableResolver.ReplaceTemplates(unresolvedWinAuth.Login, effectiveVars),
                        Password: IPororocaVariableResolver.ReplaceTemplates(unresolvedWinAuth.Password, effectiveVars),
                        Domain: IPororocaVariableResolver.ReplaceTemplates(unresolvedWinAuth.Domain, effectiveVars));
    }

    #endregion
}
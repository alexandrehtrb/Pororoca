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

    private static readonly ImmutableList<string> acceptedUriSchemes =
    [
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeWs,
        Uri.UriSchemeWss
    ];

    #region RESOLVE KEY VALUE PARAMS

    internal static List<PororocaKeyValueParam> ResolveKVParams(IEnumerable<PororocaVariable> effectiveVars, List<PororocaKeyValueParam>? unresolvedParams) =>
        unresolvedParams?
           .Where(h => h.Enabled)
           .Select(x => new PororocaKeyValueParam(
                true,
                IPororocaVariableResolver.ReplaceTemplates(x.Key, effectiveVars),
                IPororocaVariableResolver.ReplaceTemplates(x.Value, effectiveVars))).ToList() ?? [];

    #endregion

    #region REQUEST URL

    public static bool TryResolveAndMakeRequestUri(IEnumerable<PororocaVariable> effectiveVars, string rawUrl, out Uri? uri, out string? errorCode)
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

    internal static Version MakeHttpVersion(decimal httpVersion) =>
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

    internal static Dictionary<string, string> MakeContentHeaders(IReadOnlyList<PororocaKeyValueParam>? resolvedHeaders) =>
        MakeHeaders(resolvedHeaders, IsContentHeader);

    internal static Dictionary<string, string> MakeNonContentHeaders(PororocaRequestAuth? resolvedAuth, IReadOnlyList<PororocaKeyValueParam>? resolvedHeaders)
    {
        var nonContentHeaders = MakeHeaders(resolvedHeaders, headerName => !IsContentHeader(headerName));

        string? customAuthHeaderValue = MakeCustomAuthHeaderValue(resolvedAuth);
        if (customAuthHeaderValue != null)
        {
            // Custom auth overrides declared authorization header
            nonContentHeaders["Authorization"] = customAuthHeaderValue;
        }

        return nonContentHeaders;
    }

    private static Dictionary<string, string> MakeHeaders(IReadOnlyList<PororocaKeyValueParam>? resolvedHeaders, Func<string, bool> headerNameCriteria) =>
        resolvedHeaders == null ?
        new() :
        resolvedHeaders!
           .Where(h => h.Key != "Content-Type" && headerNameCriteria(h.Key)) // Content-Type header is set by the content, later
           .ToDictionary(h => h.Key, h => h.Value!);

    internal static string? MakeCustomAuthHeaderValue(PororocaRequestAuth? resolvedAuth) =>
        resolvedAuth?.Mode switch
        {
            PororocaRequestAuthMode.Bearer => $"Bearer {resolvedAuth!.BearerToken}",
            PororocaRequestAuthMode.Basic => $"Basic {Convert.ToBase64String(Encoding.UTF8.GetBytes($"{resolvedAuth!.BasicAuthLogin!}:{resolvedAuth!.BasicAuthPassword!}"))}",
            _ => null
        };

    #endregion

    #region AUTH

    internal static PororocaRequestAuth? ResolveRequestAuth(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaRequestAuth? reqAuth)
    {
        var authToUse = ChooseRequestAuth(collectionScopedAuth, reqAuth);

        if (authToUse is null)
            return null;

        return new()
        {
            Mode = authToUse.Mode,
            BasicAuthLogin = IPororocaVariableResolver.ReplaceTemplates(authToUse.BasicAuthLogin, effectiveVars),
            BasicAuthPassword = IPororocaVariableResolver.ReplaceTemplates(authToUse.BasicAuthPassword, effectiveVars),
            BearerToken = IPororocaVariableResolver.ReplaceTemplates(authToUse.BearerToken, effectiveVars),
            Windows = ResolveWindowsAuth(effectiveVars, authToUse.Windows),
            ClientCertificate = ResolveClientCertificate(effectiveVars, authToUse.ClientCertificate)
        };
    }

    #endregion

    #region CLIENT CERTIFICATES

    private static PororocaRequestAuthClientCertificate? ResolveClientCertificate(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuthClientCertificate? unresolvedClientCertificate)
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

    private static PororocaRequestAuthWindows? ResolveWindowsAuth(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuthWindows? unresolvedWinAuth)
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
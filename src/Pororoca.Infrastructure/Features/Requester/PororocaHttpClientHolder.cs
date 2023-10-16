using System.Security.Cryptography;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Infrastructure.Features.Requester;

internal sealed record ClientCertificateUniqueness(
    string? ClientCertificateFileHash,
    string? ClientCertificatePrivateKeyFileHash,
    string? ClientCertificateFilePassword)
{
    internal ClientCertificateUniqueness(PororocaRequestAuthClientCertificate? resolvedCert)
        : this(GetFileSha512Hash(resolvedCert?.CertificateFilePath),
               GetFileSha512Hash(resolvedCert?.PrivateKeyFilePath),
               resolvedCert?.FilePassword){}

    private static string? GetFileSha512Hash(string? filePath)
    {
        if (filePath == null || !File.Exists(filePath))
        {
            return null;
        }
        else
        {
            using var sha512 = SHA512.Create();
            using var fileStream = File.OpenRead(filePath);

            return BitConverter.ToString(sha512.ComputeHash(fileStream)).Replace("-", string.Empty).ToLowerInvariant();
        }
    }
}

internal sealed class PororocaHttpClientHolder : IEquatable<PororocaHttpClientHolder>
{
    internal string? Name { get; set; }

    private bool DisableSslVerification { get; }

    private ClientCertificateUniqueness? ClientCertificate { get; }

    private PororocaRequestAuthWindows? WindowsAuth { get; }

    public HttpClient? Client { get; private set; }

    internal PororocaHttpClientHolder(bool disableSslVerification, PororocaRequestAuth? resolvedAuth)
    {
        DisableSslVerification = disableSslVerification;
        ClientCertificate = resolvedAuth?.ClientCertificate is not null ? new(resolvedAuth?.ClientCertificate) : null;
        WindowsAuth = resolvedAuth?.Windows;
    }

    internal void KeepClient(HttpClient client) =>
        Client = client;    

    public override bool Equals(object? obj) =>
        obj is PororocaHttpClientHolder other
        && Equals(other);

    public bool Equals(PororocaHttpClientHolder? other) =>
        other != null
        && DisableSslVerification == other.DisableSslVerification
        && ClientCertificate == other.ClientCertificate
        && WindowsAuth == other.WindowsAuth;

    public override int GetHashCode() =>
        HashCode.Combine(DisableSslVerification, ClientCertificate, WindowsAuth);
}
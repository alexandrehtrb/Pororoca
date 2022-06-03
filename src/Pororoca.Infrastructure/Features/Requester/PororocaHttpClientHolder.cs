using System.Security.Cryptography;
using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Infrastructure.Features.Requester;

internal sealed class PororocaHttpClientHolder : IEquatable<PororocaHttpClientHolder>
{
    public string? Name { get; private set; }

    public bool DisableSslVerification { get; }

    private string? ClientCertificateFileHash { get; }

    private string? ClientCertificatePrivateKeyFileHash { get; }

    public string? ClientCertificateFilePassword { get; }

    public HttpClient? Client { get; private set; }

    internal PororocaHttpClientHolder(bool disableSslVerification, PororocaRequestAuthClientCertificate? resolvedCert)
    {
        DisableSslVerification = disableSslVerification;
        ClientCertificateFileHash = GetFileSha512Hash(resolvedCert?.CertificateFilePath);
        ClientCertificatePrivateKeyFileHash = GetFileSha512Hash(resolvedCert?.PrivateKeyFilePath);
        ClientCertificateFilePassword = resolvedCert?.FilePassword;
    }

    internal void KeepClient(HttpClient client) =>
        Client = client;

    internal void SetName(string name) =>
        Name = name;

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

    public override bool Equals(object? obj) =>
        obj is PororocaHttpClientHolder other
        && Equals(other);

    public bool Equals(PororocaHttpClientHolder? other) =>
        other != null
        && DisableSslVerification == other.DisableSslVerification
        && ClientCertificateFileHash == other.ClientCertificateFileHash
        && ClientCertificatePrivateKeyFileHash == other.ClientCertificatePrivateKeyFileHash
        && ClientCertificateFilePassword == other.ClientCertificateFilePassword;


    public override int GetHashCode() =>
        HashCode.Combine(DisableSslVerification, ClientCertificateFileHash, ClientCertificatePrivateKeyFileHash, ClientCertificateFilePassword);
}
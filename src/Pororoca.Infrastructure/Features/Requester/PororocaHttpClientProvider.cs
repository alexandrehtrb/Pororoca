using System.Net;
using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Requester;

namespace Pororoca.Infrastructure.Features.Requester;

public sealed class PororocaHttpClientProvider : IPororocaHttpClientProvider
{
    private static readonly TimeSpan defaultPooledConnectionLifetime = TimeSpan.FromMinutes(20);

    private static readonly TimeSpan defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly List<PororocaHttpClientHolder> cachedHolders = new();

    public static readonly PororocaHttpClientProvider Singleton = new();

    private PororocaHttpClientProvider()
    {
    }

    public HttpClient Provide(bool disableSslVerification, PororocaRequestAuth? resolvedAuth)
    {
        PororocaHttpClientHolder holder = new(disableSslVerification, resolvedAuth);
        var cachedHolder = cachedHolders.FirstOrDefault(h => h.Equals(holder));

        if (cachedHolder != null)
        {
            return cachedHolder.Client!;
        }
        else
        {
            var newClient = MakeHttpClient(disableSslVerification, resolvedAuth);
            holder.KeepClient(newClient);
            holder.Name = $"client{cachedHolders.Count + 1}";
            cachedHolders.Add(holder);

            return newClient;
        }
    }

    private static HttpClient MakeHttpClient(bool disableSslVerification, PororocaRequestAuth? resolvedAuth)
    {
        /*
        https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#alternatives-to-ihttpclientfactory-1
        https://stackoverflow.com/questions/48778580/singleton-httpclient-vs-creating-new-httpclient-request/68495953#68495953
        */
        SocketsHttpHandler httpHandler = new()
        {
            // Sets how long a connection can be in the pool to be considered reusable (by default - infinite)
            PooledConnectionLifetime = defaultPooledConnectionLifetime,
            AutomaticDecompression = DecompressionMethods.All
        };
        SetupTlsServerCertificateCheck(httpHandler, disableSslVerification);
        SetupTlsClientCertificateIfSelected(httpHandler, resolvedAuth?.ClientCertificate);
        SetupWindowsAuthenticationIfSelected(httpHandler, resolvedAuth?.Windows);

        HttpClient httpClient = new(httpHandler, disposeHandler: false)
        {
            Timeout = defaultTimeout
        };
        return httpClient;
    }

    private static void SetupTlsServerCertificateCheck(SocketsHttpHandler httpHandler, bool disableSslVerification)
    {
        if (disableSslVerification)
        {
            httpHandler.SslOptions.RemoteCertificateValidationCallback =
                (sender, certificate, chain, sslPolicyErrors) => true;
        }
    }

    private static void SetupTlsClientCertificateIfSelected(SocketsHttpHandler httpHandler, PororocaRequestAuthClientCertificate? resolvedCert)
    {
        if (resolvedCert != null)
        {
            var cert = PororocaClientCertificatesProvider.Provide(resolvedCert);
            httpHandler.SslOptions.ClientCertificates ??= new();
            httpHandler.SslOptions.ClientCertificates.Add(cert);
            //httpHandler.SslOptions.LocalCertificateSelectionCallback =
            //    (sender, targetHost, localCertificates, remoteCertificate, acceptableIssuers) => cert;
        }
    }

    private static void SetupWindowsAuthenticationIfSelected(SocketsHttpHandler httpHandler, PororocaRequestAuthWindows? resolvedWindowsAuth)
    {
        // https://learn.microsoft.com/en-us/dotnet/framework/network-programming/ntlm-and-kerberos-authentication
        // "The negotiate authentication module determines whether the remote server 
        // is using NTLM or Kerberos authentication, and sends the appropriate response."
        // https://stackoverflow.com/questions/2528743/defaultnetworkcredentials-or-defaultcredentials
        if (resolvedWindowsAuth != null)
        {
            httpHandler.PreAuthenticate = true;
            if (resolvedWindowsAuth.UseCurrentUser)
            {
                httpHandler.Credentials = CredentialCache.DefaultNetworkCredentials;
            }
            else
            {
                httpHandler.Credentials = new NetworkCredential(resolvedWindowsAuth.Login, resolvedWindowsAuth.Password, resolvedWindowsAuth.Domain);
            }
        }
    }
}
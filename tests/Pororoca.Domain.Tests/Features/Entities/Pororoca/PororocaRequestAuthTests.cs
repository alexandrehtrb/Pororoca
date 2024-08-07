using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Entities.Pororoca.Repetition;
using Pororoca.Domain.Features.Entities.Pororoca.WebSockets;
using Pororoca.Domain.Features.VariableResolution;
using Xunit;
using static Pororoca.Domain.Features.Entities.Pororoca.PororocaRequestAuth;

namespace Pororoca.Domain.Tests.Features.Entities.Pororoca;

public static class PororocaRequestAuthTests
{
    public static IEnumerable<object[]> GetAllTestAuths()
    {
        yield return new object[] { InheritedFromCollection };
        yield return new object[] { MakeBasicAuth("{{BasicAuthLogin}}", "{{BasicAuthPassword}}") };
        yield return new object[] { MakeBearerAuth("{{BearerAuthToken}}") };
        yield return new object[] { MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pkcs12, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}") };
        yield return new object[] { MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", "{{PrivateKeyFilePath}}", "{{PrivateKeyFilePassword}}") };
        yield return new object[] { MakeClientCertificateAuth(PororocaRequestAuthClientCertificateType.Pem, "{{CertificateFilePath}}", null, "{{PrivateKeyFilePassword}}") };
        yield return new object[] { MakeWindowsAuth(false, "{{win_login}}", "{{win_pwd}}", "{{win_domain}}") };
        yield return new object[] { MakeWindowsAuth(true, null, null, null) };
    }

    [Theory]
    [MemberData(nameof(GetAllTestAuths))]
    public static void Should_copy_auths_creating_new_instances(PororocaRequestAuth auth)
    {
        // GIVEN,  WHEN
        var copy = auth.Copy();

        // THEN
        Assert.Equal(auth, copy);
        Assert.NotSame(auth, copy);
        if (auth.ClientCertificate is not null)
        {
            Assert.Equal(auth.ClientCertificate, copy.ClientCertificate);
            Assert.NotSame(auth.ClientCertificate, copy.ClientCertificate);
        }
        if (auth.Windows is not null)
        {
            Assert.Equal(auth.Windows, copy.Windows);
            Assert.NotSame(auth.Windows, copy.Windows);
        }
    }
}
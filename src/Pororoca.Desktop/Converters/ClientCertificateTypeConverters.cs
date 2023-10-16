using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Desktop.Converters;

internal static class ClientCertificateTypeMapping
{
    internal static PororocaRequestAuthClientCertificateType? MapIndexToEnum(int index) =>
        index switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            2 => PororocaRequestAuthClientCertificateType.Pem,
            1 => PororocaRequestAuthClientCertificateType.Pkcs12,
            _ => null
        };

    internal static int MapEnumToIndex(PororocaRequestAuthClientCertificateType? type) =>
        type switch
        { // TODO: Improve this, do not use fixed integers to resolve mode
            PororocaRequestAuthClientCertificateType.Pem => 2,
            PororocaRequestAuthClientCertificateType.Pkcs12 => 1,
            _ => 0
        };
}

public class ClientCertificateTypeMatchConverter : EnumMatchConverter<PororocaRequestAuthClientCertificateType>
{
    protected override PororocaRequestAuthClientCertificateType? MapIndexToEnum(int index) =>
        ClientCertificateTypeMapping.MapIndexToEnum(index);
}
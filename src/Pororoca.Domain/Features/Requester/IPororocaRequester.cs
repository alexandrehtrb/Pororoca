using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaRequester
{
    bool DisableSslVerification { get; set; }

    bool IsValidRequest(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, out string? errorCode);

    // TODO: Customizable timeout period
    // TODO: Optional compressed request or response
    Task<PororocaHttpResponse> RequestAsync(IEnumerable<PororocaVariable> effectiveVars, PororocaRequestAuth? collectionScopedAuth, PororocaHttpRequest req, CancellationToken cancellationToken = default);
}
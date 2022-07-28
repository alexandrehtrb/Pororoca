using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaRequester
{
    bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, out string? errorCode);

    // TODO: Customizable timeout period
    // TODO: Optional compressed request or response
    Task<PororocaHttpResponse> RequestAsync(IPororocaVariableResolver variableResolver, PororocaHttpRequest req, bool disableSslVerification, CancellationToken cancellationToken = default);
}
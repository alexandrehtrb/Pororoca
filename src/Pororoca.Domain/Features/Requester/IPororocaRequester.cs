using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaRequester
{
    bool IsValidRequest(IPororocaVariableResolver variableResolver, PororocaRequest req, out string? errorCode);

    // TODO: Customizable timeout period
    // TODO: Optional compressed request or response
    Task<PororocaResponse> RequestAsync(IPororocaVariableResolver variableResolver, PororocaRequest req, bool disableSslVerification, CancellationToken cancellationToken = default);
}
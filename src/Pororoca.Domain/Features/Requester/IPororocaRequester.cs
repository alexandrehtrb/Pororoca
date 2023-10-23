using Pororoca.Domain.Features.Entities.Pororoca;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.VariableResolution;

namespace Pororoca.Domain.Features.Requester;

public interface IPororocaRequester
{
    bool IsValidRequest(IPororocaVariableResolver variableResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaHttpRequest req, out string? errorCode);

    // TODO: Customizable timeout period
    // TODO: Optional compressed request or response
    Task<PororocaHttpResponse> RequestAsync(IPororocaVariableResolver variableResolver, IEnumerable<PororocaVariable> effectiveVars, PororocaHttpRequest req, bool disableSslVerification, CancellationToken cancellationToken = default);
}
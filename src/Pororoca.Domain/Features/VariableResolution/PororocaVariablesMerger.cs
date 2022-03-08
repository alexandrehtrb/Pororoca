using Pororoca.Domain.Features.Entities.Pororoca;

namespace Pororoca.Domain.Features.VariableResolution;

public static class PororocaVariablesMerger
{
    public static PororocaVariable[] MergeVariables(IEnumerable<PororocaVariable> collectionVariables, IEnumerable<PororocaVariable>? selectedEnvironmentVariables)
    {
        IEnumerable<PororocaVariable> effectiveEnvVars = selectedEnvironmentVariables?.Where(sev => sev.Enabled) ?? Array.Empty<PororocaVariable>();
        IEnumerable<PororocaVariable> effectiveColVars = collectionVariables.Where(cv => cv.Enabled);
        IEnumerable<PororocaVariable> effectiveColVarsNotInEnv =
            effectiveColVars.Where(ecv => !effectiveEnvVars.Any(eev => eev.Key == ecv.Key));

        // Environment variable overrides collection variable
        return effectiveEnvVars.Concat(effectiveColVarsNotInEnv).ToArray();
    }
}
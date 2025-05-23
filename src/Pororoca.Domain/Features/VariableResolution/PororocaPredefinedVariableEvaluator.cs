using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pororoca.Domain.Features.VariableResolution;

[ExcludeFromCodeCoverage(Justification = "Most methods return random values. Cannot be tested.")]
public static partial class PororocaPredefinedVariableEvaluator
{
    public static bool IsPredefinedVariable(string variableKey, out string? resolvedValue)
    {
        if (!variableKey.StartsWith('$'))
        {
            resolvedValue = null;
            return false;
        }

        resolvedValue = variableKey.ToLowerInvariant() switch
        {
            "$guid" => GetRandomGuid(),
            "$now" => GetNow(),
            "$today" => GetToday(),
            "$timestamp" => GetNowTimestamp(),
            "$randombirthdate" => GetRandomBirthDate(atLeast18YearsOld: false),
            "$randombirthdateover18" => GetRandomBirthDate(atLeast18YearsOld: true),
            "$randomint" => GetRandomInt(),
            "$randomquantity" => GetRandomQuantity(),
            "$randomfullname" => GetRandomFullName(),
            "$randommanfullname" => GetRandomManFullName(),
            "$randomwomanfullname" => GetRandomWomanFullName(),
            "$randomfirstname" => GetRandomFirstName(),
            "$randommanfirstname" => GetRandomManFirstName(),
            "$randomwomanfirstname" => GetRandomWomanFirstName(),
            "$randomlastname" => GetRandomSurname(),
            "$randomcpf" => GetRandomCPF(includeSeparators: true),
            "$randomcpfdigitsonly" => GetRandomCPF(includeSeparators: false),
            "$randomcnpj" => GetRandomCNPJ(includeSeparators: true),
            "$randomcnpjdigitsonly" => GetRandomCNPJ(includeSeparators: false),
            _ => null
        };

        return resolvedValue is not null;
    }

    private static string GetRandomGuid() => Guid.NewGuid().ToString();

    // 2009-06-15T13:45:30.0000000-07:00
    private static string GetNow() => DateTimeOffset.Now.ToString("O");

    private static string GetToday() => DateTime.Today.ToString("yyyy-MM-dd");

    // timestamps are always in UTC
    private static string GetNowTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

    private static string GetRandomBirthDate(bool atLeast18YearsOld)
    {
        var daysToSubtract = TimeSpan.FromDays(Random.Shared.Next(atLeast18YearsOld ? 366 * 18 : 1, 365 * 100));
        return DateTime.Today.Subtract(daysToSubtract).ToString("yyyy-MM-dd");
    }

    private static string GetRandomInt() => Random.Shared.Next().ToString();

    private static string GetRandomQuantity() => Random.Shared.Next(1, 1001).ToString();

    #region CPF

    private static string GetRandomCPF(bool includeSeparators)
    {
        ReadOnlySpan<byte> digits = GetRandomCPFDigits();
        StringBuilder sb = new(14);
        sb.Append(digits[0]);
        sb.Append(digits[1]);
        sb.Append(digits[2]);
        if (includeSeparators) sb.Append('.');
        sb.Append(digits[3]);
        sb.Append(digits[4]);
        sb.Append(digits[5]);
        if (includeSeparators) sb.Append('.');
        sb.Append(digits[6]);
        sb.Append(digits[7]);
        sb.Append(digits[8]);
        if (includeSeparators) sb.Append('-');
        sb.Append(digits[9]);
        sb.Append(digits[10]);
        return sb.ToString();
    }

    private static ReadOnlySpan<byte> GetRandomCPFDigits()
    {
        static int CalculateVerifierDigit(ReadOnlySpan<byte> digits)
        {
            int sum = 0;
            int factor = 2;
            for (int x = digits.Length - 1; x >= 0; x--)
            {
                sum += factor * digits[x];
                factor++;
            }
            int mod = sum % 11;
            return mod < 2 ? 0 : (11 - mod);
        }

        Span<byte> digits = stackalloc byte[11];
        Random.Shared.NextBytes(digits[0..9]);
        for (int i = 0; i < 9; i++) digits[i] = (byte) (digits[i] % 10);

        digits[9] = (byte) CalculateVerifierDigit(digits[0..9]);
        digits[10] = (byte) CalculateVerifierDigit(digits[0..10]);

        return digits.ToArray();
    }

    #endregion

    #region CNPJ

    private static string GetRandomCNPJ(bool includeSeparators)
    {
        ReadOnlySpan<byte> digits = GetRandomCNPJDigits();
        StringBuilder sb = new(18);
        sb.Append(digits[0]);
        sb.Append(digits[1]);
        if (includeSeparators) sb.Append('.');
        sb.Append(digits[2]);
        sb.Append(digits[3]);
        sb.Append(digits[4]);
        if (includeSeparators) sb.Append('.');
        sb.Append(digits[5]);
        sb.Append(digits[6]);
        sb.Append(digits[7]);
        if (includeSeparators) sb.Append('/');
        sb.Append(digits[8]);
        sb.Append(digits[9]);
        sb.Append(digits[10]);
        sb.Append(digits[11]);
        if (includeSeparators) sb.Append('-');
        sb.Append(digits[12]);
        sb.Append(digits[13]);
        return sb.ToString();
    }

    private static ReadOnlySpan<byte> GetRandomCNPJDigits()
    {
        static int CalculateVerifierDigit(ReadOnlySpan<byte> digits)
        {
            int sum = 0;
            int factor = 2;
            for (int x = digits.Length - 1; x >= 0; x--)
            {
                sum += factor * digits[x];
                factor++;
                if (factor > 9) factor = 2; // DIFERENTE DO CPF
            }
            int mod = sum % 11;
            return mod < 2 ? 0 : (11 - mod);
        }

        Span<byte> digits = stackalloc byte[14];
        Random.Shared.NextBytes(digits[0..8]);
        for (int i = 0; i < 8; i++) digits[i] = (byte) (digits[i] % 10);
        digits[8] = 0;
        digits[9] = 0;
        digits[10] = 0;
        digits[11] = 1;

        digits[12] = (byte) CalculateVerifierDigit(digits[0..12]);
        digits[13] = (byte) CalculateVerifierDigit(digits[0..13]);

        return digits.ToArray();
    }

    #endregion
}
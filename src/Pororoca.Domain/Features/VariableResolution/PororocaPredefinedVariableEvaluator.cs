using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pororoca.Domain.Features.VariableResolution;

[ExcludeFromCodeCoverage(Justification = "Most methods return random values. Cannot be tested.")]
public static class PororocaPredefinedVariableEvaluator
{
    private static readonly (string lang, string[] names)[] womenFirstNamesByLang =
    [
        ("pt", [
            "Tatiana",
            "Camila",
            "Fernanda",
            "Manuela",
            "Samira"
        ]),
        ("it", [
            "Caterina",
            "Margherita",
            "Viviana",
            "Valentina"
        ]),
        ("en", [
            "Elizabeth",
            "Jessica",
            "Bonnie",
            "Heather",
            "Rochelle",
            "Scarlett",
            "Florence",
            "Madison",
            "Sarah",
            "Laura",
            "Alice",
            "Julia"
        ]),
        ("ru", [
            "Xenia",
            "Oksana",
            "Lyudmila",
            "Svetlana"
        ]),
        ("es", [
            "Esmeralda",
            "Carolina",
            "Valeria",
            "Teresa"
        ])
    ];

    private static readonly (string lang, string[] names)[] menFirstNamesByLang =
    [
        ("pt", [
            "Pedro",
            "João",
            "Matheus",
            "Ricardo",
            "Henrique"
        ]),
        ("it", [
            "Antonio",
            "Marco",
            "Enrico",
            "Massimo"
        ]),
        ("en", [
            "John",
            "Christopher",
            "James",
            "Julius",
            "Ryan",
            "Francis",
            "Michael",
            "Leigh",
            "Richard",
            "Landon",
            "Jack",
            "Harold"
        ]),
        ("ru", [
            "Dmitri",
            "Vasili",
            "Mikhail",
            "Fedor"
        ]),
        ("es", [
            "Alejandro",
            "César",
            "Fernando",
            "Javier"
        ])
    ];

    private static readonly (string lang, string[] surnames)[] surnamesByLang =
    [
        ("pt", [
            "Dias",
            "Torres",
            "Gomes",
            "Costa",
            "Almeida"
        ]),
        ("it", [
            "Acquarone",
            "Fermi",
            "Neri",
            "Cremaschi",
            "Veronesi",
            "Bon Jovi",
            "Tattaglia",
            "Morello"
        ]),
        ("ru", [
            "Cherenkov",
            "Zaitsev",
            "Radek",
            "Lomonosov",
            "Pavlichenko",
            "Mendeleev"
        ]),
        ("en", [
            "Markinson",
            "MacMillan",
            "MacFarlane",
            "Faraday",
            "Marston",
            "Deming",
            "Rock",
            "Cochrane"
        ]),
        ("es", [
            "Trastámara",
            "García",
            "Castillo",
            "Cortez",
            "Franco",
            "Zambrano",
            "La Cruz",
            "Martínez"
        ])
    ];

    public static bool IsPredefinedVariable(string variableKey, out string? resolvedValue)
    {
        resolvedValue = variableKey switch
        {
            "$guid" => GetRandomGuid(),
            "$now" => GetNow(),
            "$today" => GetToday(),
            "$randomBirthDate" => GetRandomBirthDate(atLeast18YearsOld: false),
            "$randomBirthDateOver18" => GetRandomBirthDate(atLeast18YearsOld: true),
            "$randomInt" => GetRandomInt(),
            "$randomQuantity" => GetRandomQuantity(),
            "$randomFullName" => GetRandomFullName(),
            "$randomManFullName" => GetRandomManFullName(),
            "$randomWomanFullName" => GetRandomWomanFullName(),
            "$randomFirstName" => GetRandomFirstName(),
            "$randomManFirstName" => GetRandomManFirstName(),
            "$randomWomanFirstName" => GetRandomWomanFirstName(),
            "$randomLastName" => GetRandomSurname(),
            "$randomCPF" => GetRandomCPF(includeSeparators: true),
            "$randomCPFDigitsOnly" => GetRandomCPF(includeSeparators: false),
            "$randomCNPJ" => GetRandomCNPJ(includeSeparators: true),
            "$randomCNPJDigitsOnly" => GetRandomCNPJ(includeSeparators: false),
            _ => null
        };

        return resolvedValue is not null;
    }

    private static string GetRandomGuid() => Guid.NewGuid().ToString();

    // 2009-06-15T13:45:30.0000000-07:00
    private static string GetNow() => DateTimeOffset.Now.ToString("O");

    private static string GetToday() => DateTime.Today.ToString("yyyy-MM-dd");

    private static string GetRandomBirthDate(bool atLeast18YearsOld)
    {
        var daysToSubtract = TimeSpan.FromDays(Random.Shared.Next(atLeast18YearsOld ? 366 * 18 : 1, 365 * 100));
        return DateTime.Today.Subtract(daysToSubtract).ToString("yyyy-MM-dd");
    }

    private static string GetRandomInt() => Random.Shared.Next().ToString();

    private static string GetRandomQuantity() => Random.Shared.Next(1, 1001).ToString();

    #region NAMES

    private static string GetRandomFullName() =>
        Random.Shared.Next(0,10) < 5 ?
        GetRandomWomanFullName() :
        GetRandomManFullName();

    private static string GetRandomWomanFullName() => GetRandomFullName(true);

    private static string GetRandomManFullName() => GetRandomFullName(false);

    private static string GetRandomFullName(bool falseIfManTrueIfWoman)
    {
        var group = falseIfManTrueIfWoman ? womenFirstNamesByLang : menFirstNamesByLang;
        var (lang, names) = Random.Shared.GetItems(group, 1)[0];
        string firstName = Random.Shared.GetItems(names, 1)[0];

        string surname = GetRandomSurname(lang, firstName, falseIfManTrueIfWoman);
        return $"{firstName} {surname}";
    }

    private static string GetRandomFirstName() =>
        Random.Shared.Next(0,10) < 5 ?
        GetRandomWomanFirstName() :
        GetRandomManFirstName();

    private static string GetRandomWomanFirstName() =>
        Random.Shared.GetItems(Random.Shared.GetItems(womenFirstNamesByLang, 1)[0].names, 1)[0];

    private static string GetRandomManFirstName() =>
        Random.Shared.GetItems(Random.Shared.GetItems(menFirstNamesByLang, 1)[0].names, 1)[0];

    private static string GetRandomSurname()
    {
        string randomLang = Random.Shared.GetItems(surnamesByLang, 1)[0].lang;
        return GetRandomSurname(randomLang, GetRandomFirstName(), false);
    }

    private static string GetRandomSurname(string lang1, string firstName, bool falseIfManTrueIfWoman)
    {
        // https://www.thoughtco.com/spanish-surnames-meanings-and-origins-1420795
        // english surnames are usually just the father's last name,
        // the middle name is another first name
        // for other languages, the last names of both parents are used
        // in rare occasions, spanish surnames are joined
        string surname1 = Random.Shared.GetItems(surnamesByLang.First(x => x.lang == lang1).surnames, 1)[0];
        bool middleNameIsAnotherFirstName = lang1 == "en" && Random.Shared.Next(0,10) < 8; // 80% chance when english

        if (middleNameIsAnotherFirstName)
        {
            var firstNamesByLang = falseIfManTrueIfWoman ? womenFirstNamesByLang : menFirstNamesByLang;
            string middleName;
            do
            {
                middleName = Random.Shared.GetItems(firstNamesByLang.First(x => x.lang == lang1).names, 1)[0];
            }
            while (middleName == firstName);
            return GetRandomSurname(middleName, surname1, false, false);
        }
        else
        {
            (string lang, string[] surnames) langSurnames2;
            string lang2, surname2;
            do
            {
                langSurnames2 = Random.Shared.GetItems(surnamesByLang, 1)[0];
                lang2 = langSurnames2.lang;
                surname2 = Random.Shared.GetItems(langSurnames2.surnames, 1)[0];
            }
            while (surname1 == surname2 || lang2 == "en");
            // 40% chance of joining surnames when both spanish
            bool joinSpanishSurnames = lang1 == "es" && lang2 == "es" && Random.Shared.Next(0,10) < 4;
            // swapping places
            bool swapPlaces = Random.Shared.Next(0,10) < 5;
            string s1 = swapPlaces ? surname2 : surname1;
            string s2 = swapPlaces ? surname1 : surname2;

            bool russianWoman = (lang1 == "ru" || lang2 == "ru") && falseIfManTrueIfWoman;

            return GetRandomSurname(s1, s2, russianWoman, joinSpanishSurnames);
        }
    }

    private static string GetRandomSurname(string? surname1, string surname2, bool russianWoman, bool joinSpanishSurnames)
    {
        if (surname1 is null)
        {
            return surname2;
        }
        else
        {
            if (joinSpanishSurnames)
            {
                return $"de {surname1} y {surname2}";
            }
            else
            {
                surname1 = surname1 == "La Cruz" ? "de La Cruz" : surname1;
                surname2 = surname2 == "La Cruz" ? "de La Cruz" : surname2;
                surname1 = russianWoman && surname1.EndsWith('v') ? (surname1 + 'a') : surname1;
                surname2 = russianWoman && surname2.EndsWith('v') ? (surname2 + 'a') : surname2;
                return $"{surname1} {surname2}";
            }
        }
    }

    #endregion

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
using System.Linq;

namespace DentaireApp.Business.Validation;

/// <summary>Validates phone numbers as Morocco national format (0[5–7]XXXXXXXX) or +212 with 9 following digits.</summary>
public static class TelephoneValidation
{
    /// <summary>
    /// Normalizes input to 10 digits starting with 0 (e.g. 0612345678).
    /// Rejects too-short sequences, letters, and prefixes that are not 05/06/07-style national or +212.
    /// </summary>
    public static bool TryNormalizeMorocco(string? raw, out string normalized, out string errorMessage)
    {
        normalized = string.Empty;
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            errorMessage = "Le téléphone est obligatoire.";
            return false;
        }

        var s = raw.Trim();
        if (s.Any(char.IsLetter))
        {
            errorMessage = "Le téléphone ne doit pas contenir de lettres.";
            return false;
        }

        var digits = new string(s.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            errorMessage = "Le téléphone doit contenir des chiffres.";
            return false;
        }

        if (digits.StartsWith("212", StringComparison.Ordinal))
        {
            if (digits.Length != 12)
            {
                errorMessage =
                    "Numéro +212 invalide : il faut 9 chiffres après 212 (ex. +212 612 345 678).";
                return false;
            }

            var afterCountry = digits.Substring(3);
            if (afterCountry[0] == '0' || !IsMoroccoSubscriberPrefix(afterCountry[0]))
            {
                errorMessage = "Numéro +212 invalide après l'indicatif pays (attendu 5, 6 ou 7).";
                return false;
            }

            normalized = string.Concat("0", afterCountry);
            return true;
        }

        if (digits.Length == 9 && IsMoroccoSubscriberPrefix(digits[0]))
        {
            normalized = string.Concat("0", digits);
            return true;
        }

        if (digits.Length == 10 && digits[0] == '0' && IsMoroccoSubscriberPrefix(digits[1]))
        {
            normalized = digits;
            return true;
        }

        errorMessage =
            "Numéro invalide : utilisez 10 chiffres (ex. 0612345678) ou le format international +212 6… / 7… / 5….";
        return false;
    }

    private static bool IsMoroccoSubscriberPrefix(char c) => c is '5' or '6' or '7';
}

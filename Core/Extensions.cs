using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Core;

public static class Extensions
{
    public static string? Slugify(this string? input)
    {
        if (input is null) return null;
        
        var slug = input.ToLowerInvariant();
        slug = RemoveDiacritics(slug);
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = slug.Trim('-');
        return slug;
    }

    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

}

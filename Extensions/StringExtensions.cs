using System.Globalization;
using System.Text;

namespace RH.Extensions
{
    /// <summary>
    /// Extensions pour les chaînes de caractères
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Normalise un nom en supprimant les espaces supplémentaires, 
        /// en convertissant en minuscules et en supprimant les caractères spéciaux
        /// </summary>
        /// <param name="name">Nom à normaliser</param>
        /// <returns>Nom normalisé</returns>
        public static string NormalizeName(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.Trim()
                      .ToLowerInvariant()
                      .Replace("  ", " ") // Remplacer les doubles espaces par des espaces simples
                      .Replace(".", "")   // Supprimer les points
                      .Replace(",", "")   // Supprimer les virgules
                      .Replace("-", " ")  // Remplacer les tirets par des espaces
                      .Replace("_", " ")  // Remplacer les underscores par des espaces
                      .Trim();
        }

        /// <summary>
        /// Supprime les accents d'une chaîne de caractères
        /// </summary>
        /// <param name="text">Texte avec accents</param>
        /// <returns>Texte sans accents</returns>
        public static string RemoveAccents(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

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

        /// <summary>
        /// Normalise un nom de manière avancée en supprimant les accents et en gérant les variations
        /// </summary>
        /// <param name="name">Nom à normaliser</param>
        /// <returns>Nom normalisé sans accents</returns>
        public static string NormalizeNameAdvanced(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            return name.RemoveAccents()
                      .NormalizeName();
        }

        /// <summary>
        /// Calcule la distance de Levenshtein entre deux chaînes
        /// </summary>
        /// <param name="source">Chaîne source</param>
        /// <param name="target">Chaîne cible</param>
        /// <returns>Distance de Levenshtein</returns>
        public static int LevenshteinDistance(this string source, string target)
        {
            if (string.IsNullOrEmpty(source))
                return string.IsNullOrEmpty(target) ? 0 : target.Length;

            if (string.IsNullOrEmpty(target))
                return source.Length;

            var sourceLength = source.Length;
            var targetLength = target.Length;
            var distance = new int[sourceLength + 1, targetLength + 1];

            // Initialize first column and row
            for (var i = 0; i <= sourceLength; i++)
                distance[i, 0] = i;

            for (var j = 0; j <= targetLength; j++)
                distance[0, j] = j;

            // Calculate distances
            for (var i = 1; i <= sourceLength; i++)
            {
                for (var j = 1; j <= targetLength; j++)
                {
                    var cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    distance[i, j] = Math.Min(
                        Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                        distance[i - 1, j - 1] + cost);
                }
            }

            return distance[sourceLength, targetLength];
        }

        /// <summary>
        /// Calcule le pourcentage de similarité entre deux chaînes
        /// </summary>
        /// <param name="source">Chaîne source</param>
        /// <param name="target">Chaîne cible</param>
        /// <returns>Pourcentage de similarité (0-100)</returns>
        public static double SimilarityPercentage(this string source, string target)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(target))
                return 100.0;

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(target))
                return 0.0;

            var maxLength = Math.Max(source.Length, target.Length);
            var distance = source.LevenshteinDistance(target);
            
            return (1.0 - (double)distance / maxLength) * 100.0;
        }

        /// <summary>
        /// Vérifie si deux noms sont similaires selon un seuil de similarité
        /// </summary>
        /// <param name="name1">Premier nom</param>
        /// <param name="name2">Deuxième nom</param>
        /// <param name="threshold">Seuil de similarité (par défaut 80%)</param>
        /// <returns>True si les noms sont similaires</returns>
        public static bool IsSimilarTo(this string name1, string name2, double threshold = 80.0)
        {
            if (string.IsNullOrWhiteSpace(name1) || string.IsNullOrWhiteSpace(name2))
                return false;

            var normalizedName1 = name1.NormalizeNameAdvanced();
            var normalizedName2 = name2.NormalizeNameAdvanced();

            // Vérification exacte d'abord
            if (normalizedName1 == normalizedName2)
                return true;

            // Vérification de similarité
            var similarity = normalizedName1.SimilarityPercentage(normalizedName2);
            return similarity >= threshold;
        }

        /// <summary>
        /// Tronque une chaîne à une longueur maximale en ajoutant des points de suspension
        /// </summary>
        /// <param name="text">Texte à tronquer</param>
        /// <param name="maxLength">Longueur maximale</param>
        /// <param name="suffix">Suffixe à ajouter (par défaut "...")</param>
        /// <returns>Texte tronqué</returns>
        public static string Truncate(this string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Convertit la première lettre en majuscule
        /// </summary>
        /// <param name="text">Texte à convertir</param>
        /// <returns>Texte avec première lettre en majuscule</returns>
        public static string ToTitleCase(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            return textInfo.ToTitleCase(text.ToLower());
        }

        /// <summary>
        /// Vérifie si une chaîne est un email valide
        /// </summary>
        /// <param name="email">Email à valider</param>
        /// <returns>True si l'email est valide</returns>
        public static bool IsValidEmail(this string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}


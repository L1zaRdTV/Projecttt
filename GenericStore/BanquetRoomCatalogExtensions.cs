using System.Text.RegularExpressions;

namespace GenericStore
{
    public partial class Catalogs
    {
        public int TablesCount
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Descripton)) return 0;
                Match match = Regex.Match(Descripton, @"(?:стол(?:ов|а)?\s*[:—-]?\s*)(\d+)|(\d+)\s*стол", RegexOptions.IgnoreCase);
                if (match.Success && int.TryParse(match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value, out int value)) return value;
                return 0;
            }
        }

        public string TablesDisplay => TablesCount > 0 ? $"Столов: {TablesCount}" : "Количество столов не указано";
    }
}

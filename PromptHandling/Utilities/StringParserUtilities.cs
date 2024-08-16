using System.Text.RegularExpressions;

namespace TWI.ClinicalTranslator.SharedGenerators.Utility
{
    public static class StringParserUtilities
    {
        public static string? ParseContentWithinBackticks(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return null;
            }

            // Regular expression to match content within triple backticks
            var regex = new Regex(@"```(.*?)```", RegexOptions.Singleline);
            var match = regex.Match(input);

            // Return the matched content or null if no match is found
            return match.Success ? match.Groups[1].Value : null;
        }

        public static List<string> SeparateTableDefinitions(string sqlDefinitions)
        {
            var tableDefinitions = new List<string>();
            string pattern = @"CREATE\s+TABLE\s+\w+\s*\([\s\S]+?\);";
            Regex regex = new(pattern, RegexOptions.IgnoreCase);

            MatchCollection matches = regex.Matches(sqlDefinitions);

            foreach (Match match in matches)
            {
                tableDefinitions.Add(match.Value);
            }

            return tableDefinitions;
        }

        public static List<string> SeparateBulletedList(string bulletedList)
        {
            return bulletedList.Split("\n")
                .Where(x => x.StartsWith("-") || x.StartsWith("•"))
                .Select(x => x.Replace("-", "").Replace("•", "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        }
    }
}

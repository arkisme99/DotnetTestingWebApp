using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Helpers
{
    public static partial class StringHelper
    {
        public static string[] SplitCamelCase(string input)
        {
            return [.. MyRegex().Matches(input)
                    .Cast<Match>()
                    .Select(m => m.Value)];
        }

        [GeneratedRegex(@"([A-Z][a-z0-9]+)")]
        private static partial Regex MyRegex();
    }
}
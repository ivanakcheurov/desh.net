using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Linq;

namespace Desh.OutOfTheBox
{
    public static class StandardOperators
    {
        public static IReadOnlyDictionary<string, Func<string, string[], bool>> Get()
        {
            var standardOperators = new Dictionary<string, Func<string, string[], bool>>
            {
                { ".equals", Equal },
                { "notEquals", (varValue, allowedLengths) => !Equal(varValue, allowedLengths) },
                { ".hasLength", (varValue, allowedLengths) => allowedLengths.Any(length => varValue.Length == int.Parse(length)) },
                { ".between", IsBetween },
                { "isBetween", IsBetween },
                { "numberBetween", NumberBetween },
                { "equalsAny", Equal },
                { "contains", Contains },
            };
            return standardOperators;
        }

        private static bool Equal(string varValue, string[] allowedVariants)
        {
            return allowedVariants.Any(variant => StringComparer.InvariantCultureIgnoreCase.Compare(varValue, variant) == 0);
        }
        private static bool Contains(string varValue, string[] searchedSubstrings)
        {
            return searchedSubstrings.Any(variant => CultureInfo.InvariantCulture.CompareInfo.IndexOf(varValue, variant, CompareOptions.IgnoreCase) >= 0);
        }

        private static bool IsBetween(string varValue, string[] maxMinRange)
        {
            return StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[0]) >= 0 && StringComparer.InvariantCultureIgnoreCase.Compare(varValue, maxMinRange[1]) <= 0;
        }

        private static bool NumberBetween(string varValue, string[] minMaxRange)
        {
            decimal varDecimal = decimal.Parse(varValue);
            decimal minDecimal = decimal.Parse(minMaxRange[0]);
            decimal maxDecimal = decimal.Parse(minMaxRange[1]);
            return minDecimal <= varDecimal && varDecimal <= maxDecimal;
        }
    }
}

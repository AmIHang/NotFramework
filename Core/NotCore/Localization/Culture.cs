using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Localization
{
    public class Culture
    {
        public readonly string Name;
        public readonly string Code;

        public Culture(string name, string code)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(name, nameof(name));
            Name = name;
            Code = code;
        }

        public override int GetHashCode()
            => Code.GetHashCode();

        public override bool Equals(object? obj)
            => obj is string otherString && Code.Equals(otherString, StringComparison.InvariantCultureIgnoreCase);

    }

    public static class CultureService
    {
        public readonly static Culture Invariant = new("Invariant", "");
        public readonly static Culture German = new("Deutsch", "de");

    }
}

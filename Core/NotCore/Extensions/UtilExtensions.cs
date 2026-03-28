using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Extensions
{
    public static class UtilExtensions
    {
        public static T Apply<T>(this T value, Action<T> action)
            where T : class
        {
            action(value);
            return value;
        }

    }
}

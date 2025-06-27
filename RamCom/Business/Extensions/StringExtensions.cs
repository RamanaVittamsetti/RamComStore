using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Extensions
{
    public static class StringExtensions
    {
        public static bool IsNotNullOrEmpty(this string str)
        {
            return !string.IsNullOrEmpty(str);
        }
        public static bool IsNotNullOrWhiteSpace(this string str) => !string.IsNullOrWhiteSpace(str);

    }
}

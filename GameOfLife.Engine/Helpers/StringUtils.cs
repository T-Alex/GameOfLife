using System;
using System.Text;
using System.Text.RegularExpressions;

namespace TAlex.GameOfLife.Helpers
{
    internal static class StringUtils
    {
        public static string[] MultilineStringToArray(string str)
        {
            return Regex.Split(str, "\r\n|\r|\n");
        }
    }
}

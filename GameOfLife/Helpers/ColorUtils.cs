using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;
using System.Globalization;
using System.Collections.Specialized;

namespace TAlex.GameOfLife.Helpers
{
    static class ColorUtils
    {
        public static Color Parse(string s)
        {
            string str = s.Trim();

            if (str.StartsWith("#"))
            {
                str = str.Remove(0, 1);

                return Color.FromArgb(
                    byte.Parse(str.Substring(0, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(str.Substring(2, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(str.Substring(4, 2), NumberStyles.AllowHexSpecifier),
                    byte.Parse(str.Substring(6, 2), NumberStyles.AllowHexSpecifier));
            }
            else if (str.StartsWith("sc#"))
            {
                str = str.Remove(0, 3);

                string[] parts = str.Split(',');

                return Color.FromScRgb(
                    float.Parse(parts[0].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(parts[1].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(parts[2].Trim(), CultureInfo.InvariantCulture),
                    float.Parse(parts[3].Trim(), CultureInfo.InvariantCulture));
            }
            else
            {
                throw new FormatException();
            }
        }

        public static Color[] Parse(StringCollection strs)
        {
            Color[] colors = new Color[strs.Count];

            for (int i = 0; i < strs.Count; i++)
            {
                colors[i] = Parse(strs[i]);
            }

            return colors;
        }
    }
}

using System;
using System.Windows;
using System.IO;

namespace TAlex.WPFThemes.Twilight
{
    public static class TwilightThemeManager
    {
        #region Fields

        private static string _assemblyName;

        private static readonly string[] _colorSchemes;

        private static string _uriThemeDir;

        #endregion

        #region Properties

        public static string[] ColorSchemes
        {
            get
            {
                return _colorSchemes;
            }
        }

        #endregion

        #region Constructors

        static TwilightThemeManager()
        {
            _assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            _uriThemeDir = @"/" + _assemblyName + ";" + PathCombine("component", "Themes", "Twilight");
            _colorSchemes = new string[] { "Black" , "Blue", "Silver" };
        }

        #endregion

        #region Methods

        public static bool ApplyTheme(string colorScheme)
        {
            if (Array.IndexOf(_colorSchemes, colorScheme) == -1)
            {
                return false;
            }

            ResourceDictionary resources = Application.Current.Resources;

            resources.Clear();
            resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(PathCombine(_uriThemeDir, "ColorSchemes", colorScheme + ".xaml"), UriKind.Relative)) as ResourceDictionary);
            resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(PathCombine(_uriThemeDir, "Shared.xaml"), UriKind.Relative)) as ResourceDictionary);
            resources.MergedDictionaries.Add(Application.LoadComponent(new Uri(PathCombine(_uriThemeDir, "NumericUpDown.xaml"), UriKind.Relative)) as ResourceDictionary);

            return true;
        }

        private static string PathCombine(params string[] paths)
        {
            string path = String.Empty;

            for (int i = 0; i < paths.Length; i++)
            {
                path = Path.Combine(path, paths[i]);
            }

            return path;
        }

        #endregion
    }
}

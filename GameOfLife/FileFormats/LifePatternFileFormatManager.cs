using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TAlex.GameOfLife.FileFormats
{
    public static class LifePatternFileFormatManager
    {
        #region Fields

        private static List<LifePatternFileFormat> _formats = new List<LifePatternFileFormat>();

        #endregion

        #region Properties

        public static string OpenFilter
        {
            get;
            private set;
        }

        public static string SaveFilter
        {
            get;
            private set;
        }

        public static string DefaultExt
        {
            get;
            private set;
        }

        #endregion

        #region Constructors

        static LifePatternFileFormatManager()
        {
            _formats.Add(new RLELifePatternFileFormat());
            _formats.Add(new PlaintextLifePatternFileFormat());
            _formats.Add(new Life105LifePatternFileFormat());
            _formats.Add(new Life106LifePatternFileFormat());

            DefaultExt = _formats[0].FileExtensions[0];

            // Creation open/save filters
            StringBuilder saveFilterSB = new StringBuilder();
            StringBuilder openFilterSB = new StringBuilder();

            foreach (LifePatternFileFormat format in _formats)
            {
                if (format.CanSave)
                    saveFilterSB.Append(String.Format("{0}|", format.Filter));

                if (format.CanLoad)
                    openFilterSB.Append(String.Format("{0}|", format.Filter));
            }

            saveFilterSB.Remove(saveFilterSB.Length - 1, 1);
            openFilterSB.Remove(openFilterSB.Length - 1, 1);

            SaveFilter = saveFilterSB.ToString();
            OpenFilter = String.Format("{0}|All Formats|*.*", openFilterSB);
        }

        #endregion

        #region Methods

        public static LifePattern LoadPatternFromFile(string path, out LifePatternFileFormat f)
        {
            string extension = Path.GetExtension(path);

            foreach (LifePatternFileFormat format in _formats)
            {
                if (format.CanLoad && format.ContainsExtension(extension))
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    try
                    {
                        LifePattern pattern = format.LoadPattern(fs);
                        fs.Close();
                        f = format;
                        return pattern;
                    }
                    catch (FormatException)
                    {
                        fs.Close();
                        continue;
                    }
                }
            }

            throw new FormatException();
        }

        public static LifePattern LoadPatternFromString(string str)
        {
            LifePattern pattern = null;

            foreach (LifePatternFileFormat format in _formats)
            {
                if (format is TextLifePatternFileFormat && format.CanLoad)
                {
                    TextLifePatternFileFormat textFormat = format as TextLifePatternFileFormat;
                    StringReader sr = new StringReader(str);

                    try
                    {
                        pattern = textFormat.LoadPattern(sr);
                        sr.Close();
                        break;
                    }
                    catch (FormatException)
                    {
                        sr.Close();
                        continue;
                    }
                }
            }


            if (pattern == null)
                throw new FormatException();
            else
                return pattern;
        }

        public static void SavePatternToString(LifePattern pattern, out string str)
        {
            StringBuilder sb = new StringBuilder();
            TextWriter txtWriter = new StringWriter(sb);

            TextLifePatternFileFormat lifeFormat = new RLELifePatternFileFormat();
            lifeFormat.SavePattern(pattern, txtWriter);

            str = sb.ToString();
            txtWriter.Close();
        }

        public static LifePatternFileFormat GetPatternFileFormatFromFilterIndex(int filterIndex)
        {
            int idx = 1;

            foreach (LifePatternFileFormat format in _formats)
            {
                if (format.CanSave)
                {
                    if (idx == filterIndex)
                    {
                        return format;
                    }
                    idx++;
                }
            }

            return null;
        }

        public static int GetSaveFilterIndex(LifePatternFileFormat f)
        {
            int index = 1;

            if (f == null)
            {
                return index;
            }

            foreach (LifePatternFileFormat format in _formats)
            {
                if (format.CanSave)
                {
                    if (format.GetType() == f.GetType())
                    {
                        return index;
                    }
                    index++;
                }
            }

            return 1;
        }

        #endregion
    }
}

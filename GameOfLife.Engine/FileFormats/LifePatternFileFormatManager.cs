using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;


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

        public static async Task<LifePattern> LoadPatternFromStreamAsync(Stream stream, string extension)
        {
            using (var bufferStream = new MemoryStream())
            {
                await stream.CopyToAsync(bufferStream);

                foreach (LifePatternFileFormat format in _formats.Where(x => x.CanLoad && x.IsAcceptable(extension)))
                {
                    bufferStream.Seek(0, SeekOrigin.Begin);

                    try
                    {
                        var pattern = await format.LoadPatternAsync(bufferStream);
                        pattern.Format = format;
                        return pattern;
                    }
                    catch (FormatException)
                    {
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
                    
                    using (StringReader sr = new StringReader(str))
                    {
                        try
                        {
                            pattern = textFormat.LoadPattern(sr);
                            break;
                        }
                        catch (FormatException)
                        {
                            continue;
                        }
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
            
            using (TextWriter txtWriter = new StringWriter(sb))
            {
                TextLifePatternFileFormat lifeFormat = new RLELifePatternFileFormat();
                lifeFormat.SavePattern(pattern, txtWriter);
                str = sb.ToString();
            }
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

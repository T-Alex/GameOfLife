using System;
using System.IO;
using System.Text;

using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.FileFormats
{
    public abstract class LifePatternFileFormat
    {
        #region Properties

        public abstract bool CanLoad
        {
            get;
        }

        public abstract bool CanSave
        {
            get;
        }

        public abstract string[] FileExtensions
        {
            get;
        }

        public abstract string Filter
        {
            get;
        }

        #endregion

        #region Methods

        public abstract LifePattern LoadPattern(Stream stream);

        public LifePattern LoadPattern(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            LifePattern pattern = LoadPattern(fs);
            fs.Close();

            return pattern;
        }


        public abstract void SavePattern(LifePattern pattern, Stream stream);

        public void SavePattern(LifePattern pattern, string path)
        {
            FileStream fs = new FileStream(path, FileMode.Create);
            SavePattern(pattern, fs);
            fs.Close();
        }


        public bool ContainsExtension(string ext)
        {
            foreach (string item in FileExtensions)
            {
                if (item == ext)
                    return true;
            }

            return false;
        }

        #endregion
    }
}
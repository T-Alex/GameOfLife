using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
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

        public abstract Task<LifePattern> LoadPatternAsync(Stream stream);

        public abstract Task SavePatternAsync(LifePattern pattern, Stream stream);

        public bool IsAcceptable(string fileExtension)
        {
            foreach (string item in FileExtensions)
            {
                if (item == fileExtension)
                    return true;
            }
            return false;
        }

        #endregion
    }
}
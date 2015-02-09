using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TAlex.GameOfLife.FileFormats
{
    public abstract class TextLifePatternFileFormat : LifePatternFileFormat
    {
        #region Methods

        public override LifePattern LoadPattern(Stream stream)
        {
            TextReader txtReader = new StreamReader(stream);
            LifePattern pattern = LoadPattern(txtReader);
            txtReader.Close();

            return pattern;
        }

        public abstract LifePattern LoadPattern(TextReader txtReader);


        public override void SavePattern(LifePattern pattern, Stream stream)
        {
            TextWriter txtWriter = new StreamWriter(stream);
            SavePattern(pattern, txtWriter);
            txtWriter.Close();
        }

        public abstract void SavePattern(LifePattern pattern, TextWriter txtWriter);

        #endregion
    }
}

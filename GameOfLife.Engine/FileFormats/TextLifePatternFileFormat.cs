using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;


namespace TAlex.GameOfLife.FileFormats
{
    public abstract class TextLifePatternFileFormat : LifePatternFileFormat
    {
        #region Methods

        public override async Task<LifePattern> LoadPatternAsync(Stream stream)
        {
            return await Task.Run<LifePattern>(() => LoadPattern(new StreamReader(stream)));
        }

        public abstract LifePattern LoadPattern(TextReader txtReader);


        public override async Task SavePatternAsync(LifePattern pattern, Stream stream)
        {
            using (TextWriter txtWriter = new StreamWriter(stream))
            {
                await Task.Run(() => SavePattern(pattern, txtWriter));
            }
        }

        public abstract void SavePattern(LifePattern pattern, TextWriter txtWriter);

        #endregion
    }
}

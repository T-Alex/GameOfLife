using System;
using System.Collections.Generic;
using TAlex.GameOfLife.Engine;


namespace TAlex.GameOfLife.FileFormats
{
    public class LifePattern
    {
        #region Properties

        public string Name { get; set; }

        public string Author { get; set; }

        public string Description { get; set; }

        public LifeRule Rule { get; set; }

        public Dictionary<Cell, byte> Cells { get; set; }

        public LifePatternFileFormat Format { get; set; }

        #endregion
    }
}

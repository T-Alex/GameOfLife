using System;
using System.Collections.Generic;

using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.FileFormats
{
    public class LifePattern
    {
        #region Fields

        private string _name;
        private string _author;
        private string _description;

        private LifeRule _rule;
        private Dictionary<Cell, byte> _cells;

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Author
        {
            get { return _author; }
            set { _author = value; }
        }

        public string Description
        {
            get { return _description; }
            set { _description = value; }
        }

        public LifeRule Rule
        {
            get { return _rule; }
            set { _rule = value; }
        }

        public Dictionary<Cell, byte> Cells
        {
            get
            {
                return _cells;
            }

            set
            {
                _cells = value;
            }
        }

        #endregion
    }
}

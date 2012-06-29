using System;
using System.Collections.Generic;
using System.Text;

namespace TAlex.GameOfLife.Engine
{
    public abstract class LifeEngine
    {
        #region Fields

        #endregion

        #region Properties

        public abstract byte this[int x, int y]
        {
            get;
            set;
        }

        public abstract int Population
        {
            get;
        }

        public bool IsEmpty
        {
            get
            {
                return (Population == 0);
            }
        }

        #endregion

        #region Methods

        public abstract bool NextGeneration();

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Windows;

using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.Controls
{
    public class GameFieldMemento
    {
        #region Fields

        private int _generation;

        private IDictionary<Cell, byte> _cells;

        private Int32Rect _selectedRegion;

        #endregion

        #region Properties

        public int Generation
        {
            get
            {
                return _generation;
            }
        }

        public IDictionary<Cell, byte> Cells
        {
            get
            {
                return _cells;
            }
        }

        public Int32Rect SelectedRegion
        {
            get
            {
                return _selectedRegion;
            }
        }

        #endregion

        #region Constructors

        public GameFieldMemento(int generation, IDictionary<Cell, byte> cells, Int32Rect selectedRegion)
        {
            _generation = generation;
            _cells = new Dictionary<Cell, byte>(cells);
            _selectedRegion = selectedRegion;
        }

        #endregion
    }
}

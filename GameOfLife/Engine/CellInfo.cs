using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TAlex.GameOfLife.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    struct CellInfo
    {
        #region Fields

        private Cell _cell;

        private byte _oldState;

        private byte _newState;

        #endregion

        #region Properties

        public Cell Cell
        {
            get
            {
                return _cell;
            }

            set
            {
                _cell = value;
            }
        }

        public byte OldState
        {
            get
            {
                return _oldState;
            }

            set
            {
                _oldState = value;
            }
        }

        public byte NewState
        {
            get
            {
                return _newState;
            }

            set
            {
                _newState = value;
            }
        }

        #endregion

        #region Constructors

        public CellInfo(Cell cell, byte oldState, byte newState)
        {
            _cell = cell;
            _oldState = oldState;
            _newState = newState;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            return String.Format("{0}, {1} -> {2}", _cell, _oldState, _newState);
        }

        #endregion
    }
}

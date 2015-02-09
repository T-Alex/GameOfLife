using System;
using System.Runtime.InteropServices;

namespace TAlex.GameOfLife.Engine
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Cell : IComparable, IComparable<Cell>
    {
        #region Fields

        private const int HashSize = 2048;

        public int X;
        public int Y;

        #endregion

        #region Constructors

        public Cell(int x, int y)
        {
            X = x;
            Y = y;
        }

        #endregion

        #region Methods

        public int CompareTo(Cell cell)
        {
            if (Y != cell.Y)
                return Y.CompareTo(cell.Y);
            else
                return X.CompareTo(cell.X);
        }

        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (!(obj is Cell))
                throw new ArgumentException("The type of object must be Cell.");

            return CompareTo((Cell)obj);
        }

        public override int GetHashCode()
        {
            return (Y % HashSize) * HashSize + (X % HashSize);
        }

        public override bool Equals(object obj)
        {
            if (obj is Cell)
            {
                Cell p = (Cell)obj;
                return (X == p.X) && (Y == p.Y);
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("({0}; {1})", X, Y);
        }

        #endregion

        #region Operators

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;

using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.Helpers
{
    public class LifeHelpers
    {
        public static Cell GetTopLeftCell(ICollection<Cell> cells)
        {
            int min_x = int.MaxValue, min_y = int.MaxValue;

            foreach (Cell cell in cells)
            {
                if (cell.X < min_x) min_x = cell.X;
                if (cell.Y < min_y) min_y = cell.Y;
            }

            return new Cell(min_x, min_y);
        }

        public static void GetBoundedRect(ICollection<Cell> cells, out int x, out int y, out int width, out int height)
        {
            if (cells.Count == 0)
            {
                x = y = width = height = 0;
            }
            else
            {
                int min_x = int.MaxValue, min_y = int.MaxValue;
                int max_x = int.MinValue, max_y = int.MinValue;

                foreach (Cell cell in cells)
                {
                    if (cell.X < min_x) min_x = cell.X;
                    if (cell.Y < min_y) min_y = cell.Y;

                    if (cell.X > max_x) max_x = cell.X;
                    if (cell.Y > max_y) max_y = cell.Y;
                }

                x = min_x;
                y = min_y;
                width = (max_x - min_x) + 1;
                height = (max_y - min_y) + 1;
            }
        }

        public static Dictionary<Cell, byte> GetSelectedCells(IDictionary<Cell, byte> cells, int x, int y, int width, int height)
        {
            Dictionary<Cell, byte> result = new Dictionary<Cell, byte>();

            int right = x + width;
            int bottom = y + height;

            foreach (KeyValuePair<Cell, byte> pair in cells)
            {
                if (pair.Key.X >= x && pair.Key.X < right &&
                    pair.Key.Y >= y && pair.Key.Y < bottom)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        public static Dictionary<Cell, byte> GetSelectedCells(IDictionary<Cell, byte> cells, int x1, int y1, int width1, int height1,
            int x2, int y2, int width2, int height2)
        {
            Dictionary<Cell, byte> result = new Dictionary<Cell, byte>();

            int right1 = x1 + width1;
            int bottom1 = y1 + height1;

            int right2 = x2 + width2;
            int bottom2 = y2 + height2;

            foreach (KeyValuePair<Cell, byte> pair in cells)
            {
                if ((pair.Key.X >= x1 && pair.Key.X < right1 &&
                    pair.Key.Y >= y1 && pair.Key.Y < bottom1) ||
                    (pair.Key.X >= x2 && pair.Key.X < right2 &&
                    pair.Key.Y >= y2 && pair.Key.Y < bottom2))
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        public static IDictionary<Cell, byte> CellsAlignToOrigin(Dictionary<Cell, byte> cells)
        {
            Cell topLeftCell = GetTopLeftCell(cells.Keys);
            int left = topLeftCell.X;
            int top = topLeftCell.Y;

            var resultCells = new Dictionary<Cell, byte>(cells.Count);

            foreach (var pair in cells)
            {
                resultCells.Add(new Cell(pair.Key.X - left, pair.Key.Y - top), pair.Value);
            }

            return resultCells;
        }
    }
}

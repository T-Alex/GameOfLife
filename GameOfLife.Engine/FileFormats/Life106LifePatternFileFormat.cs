using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using TAlex.GameOfLife.Engine;
using System.Threading.Tasks;

namespace TAlex.GameOfLife.FileFormats
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The Life 1.06 file format is an ASCII format that is just a list of coordinates of alive cells.
    /// Life 1.06 was designed to be easy and quick for a Life program to read and write,
    /// with the drawback being that the file size is very large for large patterns.
    /// Life 1.06 files are saved with a .lif or .life file extension.
    /// Information source: http://www.conwaylife.com/wiki/Life_1.06
    /// </remarks>
    public class Life106LifePatternFileFormat : TextLifePatternFileFormat
    {
        #region Fields

        private const string FormatSignature = "#Life";
        private const string FormatHeader = "#Life 1.06";

        #endregion

        #region Properties

        public override bool CanLoad
        {
            get { return true; }
        }

        public override bool CanSave
        {
            get { return true; }
        }

        public override string[] FileExtensions
        {
            get { return new string[] { ".lif", ".life" }; }
        }

        public override string Filter
        {
            get { return "Life 1.06 (*.lif;*.life)|*.lif;*.life"; }
        }

        #endregion

        #region Methods

        public override LifePattern LoadPattern(TextReader txtReader)
        {
            List<string> lines = new List<string>();

            while (true)
            {
                string line = txtReader.ReadLine();

                if (line != null) lines.Add(line);
                else break;
            }

            // Filling the pattern cells
            const int DefaultCapacity = 1000000;
            Dictionary<Cell, byte> cells = new Dictionary<Cell, byte>(DefaultCapacity);

            int currLine = 0;

            if (lines.Count > 0 && lines[0].Trim().StartsWith(FormatSignature))
                currLine++;

            for (int i = currLine; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (!String.IsNullOrEmpty(line))
                {
                    string[] xyStr = line.Split(' ');

                    if (xyStr.Length != 2)
                        throw new FormatException();

                    int x = int.Parse(xyStr[0]);
                    int y = int.Parse(xyStr[1]);

                    cells.Add(new Cell(x, y), 1);
                }
            }
            
            LifePattern pattern = new LifePattern
            {
                Rule = LifeRule.StandardLifeRule,
                Cells = cells
            };
            return pattern;
        }

        public override void SavePattern(LifePattern pattern, TextWriter txtWriter)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(FormatHeader);

            List<Cell> cells = new List<Cell>(pattern.Cells.Keys);
            cells.Sort(CellCmp);

            foreach (Cell cell in cells)
            {
                sb.AppendLine(String.Format("{0} {1}", cell.X, cell.Y));
            }

            // Saving data to the stream
            txtWriter.Write(sb.ToString());
        }

        private static int CellCmp(Cell c1, Cell c2)
        {
            if (c1.X != c2.X)
                return c1.X.CompareTo(c2.X);
            else
                return c1.Y.CompareTo(c2.Y);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TAlex.Common.Extensions;
using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.FileFormats
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The Life 1.05 file format is an ASCII format for storing Life patterns by simply
    /// using dots (.) to represent dead cells and asterisks (*) to represent alive cells.
    /// This file format was designed to be easily ported;
    /// you can look at a pattern saved in this format in a text editor and figure out what it is.
    /// Life 1.05 files are saved with a .lif or .life file extension.
    /// Information source: http://www.conwaylife.com/wiki/Life_1.05
    /// </remarks>
    public class Life105LifePatternFileFormat : TextLifePatternFileFormat
    {
        #region Fields

        private const string FormatSignature = "#Life 1.05";

        private const string HeaderSign = "#";

        private const string PositionSign = HeaderSign + "P";
        private const string CommentsSign = HeaderSign + "D";
        private const string NormalLifeRuleSign = HeaderSign + "N";
        private const string RuleSign = HeaderSign + "R"; 

        private const char DeadCell = '.';
        private const char AliveCell = '*';

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
            get { return "Life 1.05 (*.lif;*.life)|*.lif;*.life"; }
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


            LifePattern pattern = new LifePattern();
            pattern.Rule = LifeRule.StandardLifeRule;

            StringBuilder comments = new StringBuilder();

            int currLine = 0;

            for (int i = currLine; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWith(HeaderSign) && !line.StartsWith(PositionSign))
                    {
                        if (line.StartsWith(NormalLifeRuleSign))
                            pattern.Rule = LifeRule.StandardLifeRule;
                        else if (line.StartsWith(RuleSign))
                            pattern.Rule = LifeRule.Parse(line.Substring(RuleSign.Length).TrimStart());
                        else if (line.StartsWith(CommentsSign))
                            comments.AppendLine(line.Substring(CommentsSign.Length).TrimStart());
                    }
                    else
                    {
                        break;
                    }
                }

                currLine++;
            }

            pattern.Description = comments.ToString();


            Dictionary<Cell, byte> cells = new Dictionary<Cell, byte>();

            Cell topLeftCell = new Cell(0, 0);
            int curr_y = 0;

            for (int i = currLine; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWith(PositionSign))
                    {
                        int x = 0;
                        int y = 0;

                        if (line != PositionSign)
                        {
                            string[] parts = line.Split(' ');
                            x = int.Parse(parts[1]);
                            y = int.Parse(parts[2]);
                        }

                        topLeftCell = new Cell(x, y);
                        curr_y = 0;

                        continue;
                    }

                    for (int curr_x = 0; curr_x < line.Length; curr_x++)
                    {
                        if (line[curr_x] == DeadCell)
                            continue;
                        else if (line[curr_x] == AliveCell)
                            cells.Add(new Cell(topLeftCell.X + curr_x, topLeftCell.Y + curr_y), 1);
                        else
                            throw new FormatException();
                    }

                    curr_y++;
                }
            }

            pattern.Cells = cells;

            return pattern;
        }

        public override void SavePattern(LifePattern pattern, TextWriter txtWriter)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(FormatSignature);

            // Input the pattern comments
            if (!String.IsNullOrEmpty(pattern.Description))
            {
                string[] lines = pattern.Description.SplitByLines();

                foreach (string line in lines)
                {
                    sb.AppendLine(String.Format("{0} {1}", HeaderSign + line));
                }
            }

            if (LifeRule.Equals(pattern.Rule, LifeRule.StandardLifeRule))
            {
                sb.AppendLine(NormalLifeRuleSign);
            }
            else
            {
                sb.AppendLine(String.Format("{0} {1:x/y}", RuleSign, pattern.Rule));
            }

            List<Cell> cellList = new List<Cell>(pattern.Cells.Keys);
            cellList.Sort();

            Dictionary<Cell, int> cellMarkers = new Dictionary<Cell, int>();

            foreach (Cell cell in cellList)
            {
                cellMarkers.Add(cell, 0);
            }

            int currBlock = 1;
            List<List<Cell>> blocks = new List<List<Cell>>();

            for (int i = 0; i < cellMarkers.Count; i++)
            {
                if (cellMarkers[cellList[i]] == 0)
                {
                    blocks.Add(new List<Cell>());
                    blocks[currBlock - 1] = new List<Cell>();
                    MarkCellBlock(cellMarkers, blocks[currBlock - 1], cellList[i], currBlock);
                    currBlock++;
                }
            }

            foreach (List<Cell> block in blocks)
            {
                block.Sort();

                int min_x, min_y, w, h;
                Helpers.LifeHelpers.GetBoundedRect(block, out min_x, out min_y, out w, out h);

                sb.AppendLine(String.Format("{0} {1} {2}", PositionSign, min_x, min_y));

                int curr_x = 0;
                int curr_y = 0;

                foreach (Cell cell in block)
                {
                    Cell offsetCell = new Cell(cell.X - min_x, cell.Y - min_y);

                    if (curr_y != offsetCell.Y)
                    {
                        sb.AppendLine();

                        int skippedLines = offsetCell.Y - curr_y - 1;
                        for (int i = 0; i < skippedLines; i++)
                        {
                            sb.Append(DeadCell);
                            sb.AppendLine();
                        }

                        curr_x = 0;
                        curr_y = offsetCell.Y;
                    }

                    int deadCells = offsetCell.X - curr_x;
                    curr_x = offsetCell.X + 1;

                    for (int i = 0; i < deadCells; i++)
                    {
                        sb.Append(DeadCell);
                    }

                    sb.Append(AliveCell);
                }

                sb.AppendLine();
            }

            // Saving data to the stream
            txtWriter.Write(sb.ToString());
        }

        private void MarkCellBlock(Dictionary<Cell, int> cellMarkers, List<Cell> block, Cell currCell, int currBlock)
        {
            if (cellMarkers[currCell] == 0)
            {
                cellMarkers[currCell] = currBlock;
                block.Add(currCell);

                int col = currCell.X;
                int row = currCell.Y;

                for (int x = col - 1; x <= col + 1; x++)
                {
                    for (int y = row - 1; y <= row + 1; y++)
                    {
                        if (x != col || y != row)
                        {
                            Cell c = new Cell(x, y);

                            if (cellMarkers.ContainsKey(c) && cellMarkers[c] == 0)
                            {
                                MarkCellBlock(cellMarkers, block, c, currBlock);
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}

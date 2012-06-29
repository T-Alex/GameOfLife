using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using TAlex.GameOfLife.Engine;

namespace TAlex.GameOfLife.FileFormats
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// The plaintext file format is a recent ASCII file format that is similar to Life 1.05
    /// in that it stores patterns simply by representing dead and alive cells by different characters
    /// and "drawing" the pattern with those characters.
    /// The particulars of the plaintext file format described here are based on the format used
    /// by Edwin Martin's Game of Life program, which uses the .cells file extension.
    /// Information source: http://www.conwaylife.com/wiki/Plaintext
    /// </remarks>
    public class PlaintextLifePatternFileFormat : TextLifePatternFileFormat
    {
        #region Fields

        private const string CommentsSign = "!";
        private const string PatternNameSign = CommentsSign + "Name:";
        private const string PatternAuthorSign = CommentsSign + "Author:";
        private const string PatternRuleSign = CommentsSign + "Rule:";

        private const char DeadCell = '.';
        private const char AliveCell = 'O';

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
            get { return new string[] { ".cells" }; }
        }

        public override string Filter
        {
            get { return "Plaintext (*.cells)|*.cells"; }
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

            int currLine = 0;

            // Search header lines
            StringBuilder comments = new StringBuilder();

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (!String.IsNullOrEmpty(line))
                {
                    if (line.StartsWith(PatternNameSign))
                        pattern.Name = line.Substring(PatternNameSign.Length).TrimStart();
                    else if (line.StartsWith(PatternAuthorSign))
                        pattern.Author = line.Substring(PatternAuthorSign.Length).TrimStart();
                    else if (line.StartsWith(PatternRuleSign))
                        pattern.Rule = LifeRule.Parse(line.Substring(PatternRuleSign.Length).TrimStart());
                    if (line.StartsWith(CommentsSign))
                        comments.AppendLine(line.Substring(CommentsSign.Length).TrimStart());
                    else
                        break;
                }

                currLine++;
            }

            pattern.Description = comments.ToString();

            // Filling the pattern cells
            const int DefaultCapacity = 1000000;
            Dictionary<Cell, byte> cells = new Dictionary<Cell, byte>(DefaultCapacity);

            int curr_y = 0;

            for (int i = currLine; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                for (int curr_x = 0; curr_x < line.Length; curr_x++)
                {
                    char c = line[curr_x];

                    if (c == DeadCell)
                        continue;
                    else if (c == AliveCell)
                        cells.Add(new Cell(curr_x, curr_y), 1);
                    else
                        throw new FormatException();
                }

                curr_y++;
            }

            pattern.Cells = cells;

            return pattern;
        }

        public override void SavePattern(LifePattern pattern, TextWriter txtWriter)
        {
            StringBuilder sb = new StringBuilder();

            // Input the pattern name
            if (!String.IsNullOrEmpty(pattern.Name))
                sb.AppendLine(String.Format("{0} {1}", PatternNameSign, pattern.Name));

            // Input the pattern author
            if (!String.IsNullOrEmpty(pattern.Author))
                sb.AppendLine(String.Format("{0} {1}", PatternAuthorSign, pattern.Author));

            // Input the pattern rule
            if (!LifeRule.Equals(pattern.Rule, LifeRule.StandardLifeRule))
                sb.AppendLine(String.Format("{0} {1}", PatternRuleSign, pattern.Rule.ToString()));

            // Input the pattern comments
            if (!String.IsNullOrEmpty(pattern.Description))
            {
                string[] lines = Helpers.StringUtils.MultilineStringToArray(pattern.Description);

                foreach (string line in lines)
                {
                    sb.AppendLine(CommentsSign + line);
                }
            }

            List<Cell> cells = new List<Cell>(pattern.Cells.Keys);
            cells.Sort();

            // Finding top-left cell
            Cell topLeftCell = Helpers.LifeHelpers.GetTopLeftCell(cells);
            int min_x = topLeftCell.X;
            int min_y = topLeftCell.Y;

            int curr_x = 0;
            int curr_y = 0;

            foreach (Cell cell in cells)
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

            // Saving data to the stream
            txtWriter.Write(sb.ToString());
        }

        #endregion
    }
}

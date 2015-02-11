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
    /// The Run Length Encoded (or RLE for short) file format is commonly-used for storing large patterns.
    /// It is more cryptic than some other file formats such as plaintext and Life 1.06, but is still quite readable.
    /// Many features of the RLE file format are incorporated in the MCell file format.
    /// RLE files are saved with a .rle file extension.
    /// Information source: http://www.conwaylife.com/wiki/RLE
    /// </remarks>
    public class RLELifePatternFileFormat : TextLifePatternFileFormat
    {
        #region Fields

        private const int LineLengthLimit = 70;

        private const string HeaderSign = "#";
        private const string CommentsSign = HeaderSign + "C";

        private const string PatternNameSign = HeaderSign + "N";
        private const string PatternAuthorSign = HeaderSign + "O";

        private const char EndOfLineSign = '$';
        private const char EndOfPatternSign = '!';

        private const char AliveCell = 'o';
        private const char DeadCell = 'b';
        private const char DeadCell2 = '.';

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
            get { return new string[] { ".rle" }; }
        }

        public override string Filter
        {
            get { return "RLE (*.rle)|*.rle"; }
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
                    else if (line.ToUpper().StartsWith(CommentsSign))
                        comments.AppendLine(line.Substring(CommentsSign.Length).TrimStart());
                    else if (line.StartsWith(HeaderSign))
                        comments.AppendLine(line.Substring(HeaderSign.Length).TrimStart());
                    else
                        break;
                }

                currLine++;
            }

            pattern.Description = comments.ToString();

            // Parse the line with the data of height, width and rule of pattern
            if (currLine < lines.Count)
            {
                string lineparams = lines[currLine].Trim();
                string[] parts = lineparams.Split(',');

                if (parts.Length != 3)
                    throw new FormatException();

                try
                {
                    int w = int.Parse(parts[0].Split('=')[1]);
                    int h = int.Parse(parts[1].Split('=')[1]);
                    pattern.Rule = LifeRule.Parse(parts[2].Split('=')[1]);
                }
                catch (IndexOutOfRangeException)
                {
                    throw new FormatException();
                }

                currLine++;
            }

            // Filling the pattern
            const int DefaultCapacity = 1000000;
            Dictionary<Cell, byte> cells = new Dictionary<Cell, byte>(DefaultCapacity);

            int curr_y = 0;
            int curr_x = 0;

            for (int i = currLine; i < lines.Count; i++)
            {
                bool finish = false;
                string line = lines[i].Trim();
                int len = line.Length;

                if (len == 0)
                {
                    continue;
                }

                for (int j = 0; j < len; j++)
                {
                    int repeats = 1;

                    if (Char.IsDigit(line[j]))
                    {
                        string numstr = line[j].ToString();
                        j++;

                        while (j < len && Char.IsDigit(line[j]))
                        {
                            numstr += line[j];
                            j++;
                        }

                        repeats = int.Parse(numstr);
                    }

                    if (Char.IsLetter(line[j]) || line[j] == DeadCell2)
                    {
                        char ch = line[j];
                        byte state;

                        switch (ch)
                        {
                            case AliveCell:
                                state = 1;
                                break;

                            case DeadCell:
                            case DeadCell2:
                                state = 0;
                                break;

                            default:
                                throw new FormatException();
                        }

                        if (state != 0)
                        {
                            for (int k = 0; k < repeats; k++)
                            {
                                cells.Add(new Cell(curr_x + k, curr_y), state);
                            }
                        }

                        curr_x += repeats;
                    }
                    else if (line[j] == EndOfLineSign)
                    {
                        curr_y += repeats;
                        curr_x = 0;
                    }
                    else if (line[j] == EndOfPatternSign)
                    {
                        finish = true;
                        break;
                    }
                    else
                    {
                        throw new FormatException();
                    }
                }

                if (finish) break;
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

            // Input the pattern comments
            if (!String.IsNullOrEmpty(pattern.Description))
            {
                string[] lines = pattern.Description.SplitByLines();

                foreach (string line in lines)
                {
                    sb.AppendLine(HeaderSign + line);
                }
            }


            List<Cell> cells = new List<Cell>(pattern.Cells.Keys);
            cells.Sort();

            int min_x, min_y;
            int w, h;
            Helpers.LifeHelpers.GetBoundedRect(cells, out min_x, out min_y, out w, out h);

            sb.AppendFormat("x = {0}, y = {1}, rule = {2}", w, h, pattern.Rule.ToString());
            sb.AppendLine();


            // Input cells
            int curr_x = 0;
            int curr_y = 0;

            int currLineLen = 0;

            for (int i = 0; i < cells.Count; i++)
            {
                Cell offsetCell = new Cell(cells[i].X - min_x, cells[i].Y - min_y);

                if (curr_y != offsetCell.Y)
                {
                    int skippedLines = offsetCell.Y - curr_y;
                    string s = String.Empty;

                    if (skippedLines > 1)
                        s += skippedLines;

                    s += EndOfLineSign;
                    currLineLen += s.Length;

                    if (currLineLen > LineLengthLimit)
                    {
                        currLineLen = s.Length;
                        sb.AppendLine();
                    }

                    sb.Append(s);

                    curr_x = 0;
                    curr_y = offsetCell.Y;
                }


                int deadCells = offsetCell.X - curr_x;

                if (deadCells > 0)
                {
                    String s = String.Empty;

                    if (deadCells > 1)
                        s += deadCells;

                    s += DeadCell;
                    currLineLen += s.Length;

                    if (currLineLen > LineLengthLimit)
                    {
                        currLineLen = s.Length;
                        sb.AppendLine();
                    }

                    sb.Append(s);
                }

                int aliveCells = 1;
                
                while (i < cells.Count - 1 &&
                    (cells[i].Y - min_y) == curr_y &&
                    (cells[i + 1].Y - min_y) == curr_y &&
                    cells[i].X == cells[i + 1].X - 1)
                {
                    i++;
                    aliveCells++;
                }

                String str = String.Empty;

                if (aliveCells > 1)
                    str += aliveCells;

                str += AliveCell;
                currLineLen += str.Length;

                if (currLineLen > LineLengthLimit)
                {
                    currLineLen = str.Length;
                    sb.AppendLine();
                }

                sb.Append(str);

                curr_x = (cells[i].X - min_x) + 1;
            }

            if (currLineLen > LineLengthLimit)
                sb.AppendLine();

            sb.Append(EndOfPatternSign);

            // Saving data to the stream
            txtWriter.Write(sb.ToString());
        }

        #endregion
    }
}

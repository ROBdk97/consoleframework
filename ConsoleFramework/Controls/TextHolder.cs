using ConsoleFramework.Core;
using System;
using System.Collections.Generic;
using System.Linq;

// TODO : Autoindent
// TODO : Ctrl+Home/Ctrl+End
// TODO : Alt+Backspace deletes word
// TODO : Shift+Delete deletes line
// TODO : Scrollbars full support
// TODO : Ctrl+arrows
// TODO : Selection
// TODO : Selection copy/paste/cut/delete
// TODO : Undo/Redo, commands autogrouping
// TODO : Read only mode
// TODO : Tabs (converting to spaces when loading?)
namespace ConsoleFramework.Controls
{
    public class TextHolder
    {
        // TODO : change to more appropriate data structure
        private List<string> lines;
        private readonly string newLine = Environment.NewLine;

        public TextHolder(string text, string newLine)
        {
            this.newLine = newLine;
            setText(text);
        }

        public TextHolder(string text)
        {
            setText(text);
        }

        private void setText(string text)
        {
            lines = [.. text.Split([newLine], StringSplitOptions.None)];
        }

        public string Text
        {
            get => string.Join(newLine, lines);
            set => setText(value);
        }

        public IList<string> Lines => lines.AsReadOnly();

        public int LinesCount => lines.Count;
        public int ColumnsCount => lines.Max(it => it.Length);

        /// <summary>
        /// Inserts string after specified position with respect to newline symbols.
        /// Returns the coords (col+ln) of next symbol after inserted.
        /// TODO : write unit test to check return value
        /// </summary>
        public Point Insert(int ln, int col, string s)
        {
            // There are at least one empty line even if no text at all
            if (ln >= lines.Count)
            {
                throw new ArgumentException("ln is out of range", nameof(ln));
            }

            string currentLine = lines[ln];
            if (col > currentLine.Length)
            {
                throw new ArgumentException("col is out of range", nameof(col));
            }

            string leftPart = currentLine[..col];
            string rightPart = currentLine[col..];

            string[] linesToInsert = s.Split([Environment.NewLine], StringSplitOptions.None);

            if (linesToInsert.Length == 1)
            {
                lines[ln] = leftPart + linesToInsert[0] + rightPart;
                return new Point(leftPart.Length + linesToInsert[0].Length, ln);
            }
            else
            {
                lines[ln] = leftPart + linesToInsert[0];
                lines.InsertRange(ln + 1, linesToInsert.Skip(1).Take(linesToInsert.Length - 1));
                string lastStrLeftPart = lines[ln + linesToInsert.Length - 1];
                lines[ln + linesToInsert.Length - 1] = lastStrLeftPart + rightPart;
                return new Point(lastStrLeftPart.Length, ln + linesToInsert.Length - 1);
            }
        }

        /// <summary>
        /// Will write the content of text editor to matrix constrained with width/height,
        /// starting from (left, top) coord. Coords may be negative.
        /// If there are any gap before (or after) text due to margin, window will be filled
        /// with spaces there.
        /// Window size should be equal to width/height passed.
        /// </summary>
        public void WriteToWindow(int left, int top, int width, int height, char[,] window)
        {
            if (window.GetLength(0) != height)
            {
                throw new ArgumentException("window height differs from viewport height");
            }

            if (window.GetLength(1) != width)
            {
                throw new ArgumentException("window width differs from viewport width");
            }

            for (int y = top; y < 0; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    window[y - top, x] = ' ';
                }
            }

            for (int y = Math.Max(0, top); y < Math.Min(top + height, lines.Count); y++)
            {
                string line = lines[y];
                for (int x = left; x < 0; x++)
                {
                    window[y - top, x - left] = ' ';
                }

                for (int x = Math.Max(0, left); x < Math.Min(left + width, line.Length); x++)
                {
                    window[y - top, x - left] = line[x];
                }

                for (int x = Math.Max(line.Length, left); x < left + width; x++)
                {
                    window[y - top, x - left] = ' ';
                }
            }

            for (int y = lines.Count; y < top + height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    window[y - top, x] = ' ';
                }
            }
        }

        /// <summary>
        /// Deletes text from lnFrom+colFrom to lnTo+colTo (exclusive).
        /// </summary>
        public void Delete(int lnFrom, int colFrom, int lnTo, int colTo)
        {
            if (lnFrom > lnTo)
            {
                throw new ArgumentException("lnFrom should be <= lnTo");
            }
            if (lnFrom == lnTo && colFrom >= colTo)
            {
                throw new ArgumentException("colFrom should be < colTo on the same line");
            }
            //
            lines[lnFrom] = lines[lnFrom][..colFrom] + lines[lnTo][colTo..];
            lines.RemoveRange(lnFrom + 1, lnTo - lnFrom);
        }
    }
}


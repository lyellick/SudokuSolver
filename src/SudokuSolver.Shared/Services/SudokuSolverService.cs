﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using Tesseract;
using SudokuSolver.Shared.Extensions;

namespace SudokuSolver.Shared.Services
{
    public interface ISudokuSolverService
    {
        int[][] ExtractGrid(MemoryStream imageStream);

        int[][] ExtractGrid(string path);

        (int row, int col) GetSectionCenter((int row, int col) location);

        int[][] GetSection((int row, int col) location, int[][] source);

        int[] GetRow((int row, int col) location, int[][] source);

        int[] GetColumn((int row, int col) location, int[][] source);

        bool ExistsInSection((int row, int col) location, int[][] source, int value);

        bool ExistsInRow((int row, int col) location, int[][] source, int value);

        bool ExistsInColumn((int row, int col) location, int[][] source, int value);

        int[] GetRemainingRowValues((int row, int col) location, int[][] source);

        int[] GetRemainingColumnValues((int row, int col) location, int[][] source);

        int[] GetRemainingSectionValues((int row, int col) location, int[][] source);

        bool ValidateRow(int row, int[][] source);

        bool ValidateColumn(int col, int[][] source);

        bool ValidateSection((int row, int col) location, int[][] source);

        int[][] Backtrack(int[][] source);

        bool IsValidPlacement((int row, int col) location, int[][] source, int value);

        (int row, int col)? FindEmptyCell(int[][] source);
    }

    public class SudokuSolverService : ISudokuSolverService
    {
        public int[][] ExtractGrid(MemoryStream imageStream)
        {
            List<List<int>> sudoku = [];

            (int row, int col) start = (403, 21);
            (int row, int col) end = (1442, 1059);


            imageStream.Seek(0, SeekOrigin.Begin);

            using var puzzle = Image.Load<Rgba32>(imageStream);

            puzzle.Mutate(x => x.Crop(new Rectangle(start.col, start.row, end.col - start.col, end.row - start.row)));

            puzzle.Mutate(x => x.GaussianBlur(3));
            puzzle.Mutate(x => x.BinaryThreshold(0.5f));

            puzzle.Mutate(x =>
            {
                int startCol = 343;
                int startRow = 0;

                Color fill = Color.White;

                puzzle.FloodFill(startCol, startRow, fill);
            });

            (int width, int height) = (108, 110);

            int verticalOffset = 4;
            int horizontalOffset = 5;
            int borderWidth = 8;

            using var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default);

            engine.SetVariable("tessedit_char_whitelist", "123456789");

            for (int row = 0; row < 9; row++)
            {
                var section = puzzle.Clone(x => x.Grayscale().Crop(new Rectangle(0, row * (height + verticalOffset), puzzle.Width, height)));

                using MemoryStream sectionStream = new();

                section.SaveAsPng(sectionStream);

                sectionStream.Seek(0, SeekOrigin.Begin);

                using var sectionImage = Pix.LoadFromMemory(sectionStream.ToArray());
                using var sectionPage = engine.Process(sectionImage, PageSegMode.SingleLine);

                var sections = sectionPage.GetText().Trim().Replace("\n", "").Replace(" ", "");

                List<bool> sectionsMap = [];

                for (int col = 0; col < 9; col++)
                {
                    int startCol = col % 3 != 0 ? col * (width + horizontalOffset) + borderWidth : col * (width + horizontalOffset);
                    int startRow = row % 3 != 0 ? row * (height + verticalOffset) + borderWidth : row * (height + verticalOffset);
                    int endCol = startCol + width;
                    int endRow = startRow + height;

                    var cell = puzzle.Clone(x => x.Grayscale().Crop(new Rectangle(startCol, startRow, endCol - startCol, endRow - startRow)));

                    cell.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(width * 3, height * 3) }));

                    bool hasNumber = cell.IsColorPresent(Color.Black);

                    sectionsMap.Add(hasNumber);
                }

                List<int> cells = [];

                int plot = 0;

                for (int i = 0; i < sectionsMap.Count; i++)
                {
                    if (sectionsMap[i])
                    {
                        cells.Add(int.Parse(sections[plot].ToString()));
                        plot++;
                    }
                    else
                    {
                        cells.Add(0);
                    }
                }

                sudoku.Add(cells);
            }

            return sudoku.Select(row => row.ToArray()).ToArray();
        }

        public int[][] ExtractGrid(string path)
        {
            using var image = File.OpenRead(path);

            using var imageStream = new MemoryStream();
            image.CopyTo(imageStream);

            var extract = ExtractGrid(imageStream);

            return extract;
        }

        public (int row, int col) GetSectionCenter((int row, int col) location)
        {
            int row = Math.DivRem(location.row, 3, out _);
            int col = Math.DivRem(location.col, 3, out _);

            return ((row * 3) + 3 / 2, (col * 3) + 3 / 2);
        }

        public int[][] GetSection((int row, int col) location, int[][] source)
        {
            var (row, col) = GetSectionCenter(location);

            int[][] section = new int[3][];

            int start = row - (3) / 2;
            int end = row + (3) / 2;
            int left = col - (3) / 2;
            int right = col + (3) / 2;

            int i = 0;

            for (int r = start; r <= end; r++)
            {
                List<int> cols = [];

                for (int c = left; c <= right; c++)
                    cols.Add(source[r][c]);

                section[i] = [.. cols];
                i++;
            }

            return section;
        }

        public int[] GetRow((int row, int col) location, int[][] source)
        {
            int[] cells = new int[9];

            for (int col = 0; col < 9; col++)
                cells[col] = source[location.row][col];

            return cells;
        }

        public int[] GetColumn((int row, int col) location, int[][] source)
        {
            int[] cells = new int[9];

            for (int row = 0; row < 9; row++)
                cells[row] = source[row][location.col];

            return cells;
        }

        public bool ExistsInSection((int row, int col) location, int[][] source, int value)
        {
            var section = GetSection(location, source);

            return section.Any(row => row.Contains(value));
        }

        public bool ExistsInRow((int row, int col) location, int[][] source, int value)
        {
            var row = GetRow(location, source);

            return row.Any(col => col == value);
        }

        public bool ExistsInColumn((int row, int col) location, int[][] source, int value)
        {
            var column = GetColumn(location, source);

            return column.Any(row => row == value);
        }

        public bool ValidateRow(int row, int[][] source)
        {
            return source[row].Select(col => col).Sum() == 45;
        }

        public bool ValidateColumn(int col, int[][] source)
        {
            return source.Select(row => row[col]).Sum() == 45;
        }

        public bool ValidateSection((int row, int col) location, int[][] source)
        {
            var center = GetSection(location, source);

            return center.Sum(row => row.Sum(col => col)) == 35;
        }

        public int[] GetRemainingRowValues((int row, int col) location, int[][] source)
        {
            int[] available = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            var row = GetRow(location, source);

            return available.Except(row).ToArray();
        }

        public int[] GetRemainingColumnValues((int row, int col) location, int[][] source)
        {
            int[] available = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            var col = GetColumn(location, source);

            return available.Except(col).ToArray();
        }

        public int[] GetRemainingSectionValues((int row, int col) location, int[][] source)
        {
            int[] available = [1, 2, 3, 4, 5, 6, 7, 8, 9];

            var section = GetSection(location, source).Flatten().ToArray();

            return available.Except(section).ToArray();
        }

        public int[][] Backtrack(int[][] source)
        {
            var emptyCell = FindEmptyCell(source);

            if (emptyCell == null)
                return source;

            int row = emptyCell.Value.row;
            int col = emptyCell.Value.col;

            for (int num = 1; num <= 9; num++)
            {
                if (IsValidPlacement((row, col), source, num))
                {
                    source[row][col] = num;

                    var result = Backtrack(source);

                    if (result != null)
                        return result;

                    source[row][col] = 0;
                }
            }

            return null;
        }

        public (int row, int col)? FindEmptyCell(int[][] source)
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    if (source[row][col] == 0)
                        return (row, col);
                }
            }

            return null;
        }


        public bool IsValidPlacement((int row, int col) location, int[][] source, int value)
        {
            var validRow = !ExistsInRow(location, source, value);
            var validColumn = !ExistsInColumn(location, source, value);
            var validSection = !ExistsInSection(location, source, value);

            return validRow && validColumn && validSection;
        }
    }
}

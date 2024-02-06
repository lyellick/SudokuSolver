using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using Tesseract;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SudokuSolver.Tests
{
    public class Sandbox
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Light()
        {
            List<List<int>> sudoku = new();

            bool show = false;

            var background = "light";

            (int row, int col) start = (403, 21);
            (int row, int col) end = (1442, 1059);

            using var puzzle = Image.Load<Rgba32>(@$"..\..\..\..\..\assets\puzzles\sudoku-{background}.jpg");

            puzzle.Mutate(x => x.Crop(new Rectangle(start.col, start.row, end.col - start.col, end.row - start.row)));

            puzzle.Mutate(x => x.GaussianBlur(3));
            puzzle.Mutate(x => x.BinaryThreshold(0.5f));

            puzzle.Mutate(x =>
            {
                int startCol = 343;
                int startRow = 0;

                Color fill = Color.White;

                FloodFill(puzzle, startCol, startRow, fill);
            });

            puzzle.SaveAsPng($@"..\..\..\..\..\assets\puzzles\puzzle.png");

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

                if (show)
                    section.SaveAsPng($@"..\..\..\..\..\assets\puzzles\R{row}.png");

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

                    bool hasNumber = IsColorPresent(cell, Color.Black);

                    sectionsMap.Add(hasNumber);

                    if (show)
                        cell.SaveAsPng($@"..\..\..\..\..\assets\puzzles\R{row}C{col}.png");
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
        }

        private void FloodFill(Image<Rgba32> image, int startCol, int startRow, Color fill)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            HashSet<(int, int)> visited = [];

            Rgba32 targetColor = image[startCol, startRow];
            
            queue.Enqueue((startCol, startRow));

            while (queue.Count > 0)
            {
                var (col, row) = queue.Dequeue();

                if (col < 0 || col >= image.Width || row < 0 || row >= image.Height || image[col, row] != targetColor || visited.Contains((col, row)))
                {
                    continue;
                }

                image[col, row] = fill;
                visited.Add((col, row));

                queue.Enqueue((col + 1, row));
                queue.Enqueue((col - 1, row));
                queue.Enqueue((col, row + 1));
                queue.Enqueue((col, row - 1));
            }
        }

        private static bool IsColorPresent(Image<Rgba32> cell, Rgba32 target)
        {
            for (int row = 0; row < cell.Height; row++)
                for (int col = 0; col < cell.Width; col++)
                {
                    Rgba32 color = cell[col, row];

                    if (color.Equals(target))
                        return true;
                }

            return false;
        }
    }
}
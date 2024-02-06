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

            puzzle.Mutate(x => x.Invert());

            puzzle.SaveAsPng($@"..\..\..\..\..\assets\puzzles\puzzle.png");

            (int width, int height) = (108, 110);

            int verticalOffset = 4;
            int horizontalOffset = 5;
            int borderWidth = 8;

            using var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default);

            engine.SetVariable("tessedit_char_whitelist", "123456789");

            List<List<string>> grid = new();
            string p = "";
            for (int row = 0; row < 9; row++)
            {
                List<string> gridRows = new();

                for (int col = 0; col < 9; col++)
                {
                    int startCol = col % 3 != 0 ? col * (width + horizontalOffset) + borderWidth : col * (width + horizontalOffset);
                    int startRow = row % 3 != 0 ? row * (height + verticalOffset) + borderWidth : row * (height + verticalOffset);
                    int endCol = startCol + width;
                    int endRow = startRow + height;

                    var copy = puzzle.Clone(x => x.Grayscale().Crop(new Rectangle(startCol, startRow, endCol - startCol, endRow - startRow)));

                    copy.Mutate(x => x.Resize(new ResizeOptions { Size = new Size(width * 3, height * 3) }));



                    using MemoryStream stream = new();

                    copy.SaveAsPng(stream);
                    copy.SaveAsPng($@"..\..\..\..\..\assets\puzzles\R{row}C{col}.png");

                    stream.Seek(0, SeekOrigin.Begin);

                    using var img = Pix.LoadFromMemory(stream.ToArray());
                    using var page = engine.Process(img, PageSegMode.SingleChar);

                    var value = page.GetText();

                    gridRows.Add(value);
                    p += value.Replace("\n", "");

                    //File.Delete($@"..\..\..\..\..\assets\puzzles\R{row}C{col}.png");
                }
                p += "\n";
                grid.Add(gridRows);
            }
        }

        private void FloodFill(Image<Rgba32> image, int startCol, int startRow, Color fill)
        {
            Queue<(int, int)> queue = new Queue<(int, int)>();
            HashSet<(int, int)> visited = new HashSet<(int, int)>();

            Rgba32 targetColor = image[startCol, startRow];
            queue.Enqueue((startCol, startRow));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                if (x < 0 || x >= image.Width || y < 0 || y >= image.Height || image[x, y] != targetColor || visited.Contains((x, y)))
                {
                    continue;
                }

                image[x, y] = fill;
                visited.Add((x, y));

                queue.Enqueue((x + 1, y));
                queue.Enqueue((x - 1, y));
                queue.Enqueue((x, y + 1));
                queue.Enqueue((x, y - 1));
            }
        }
    }
}
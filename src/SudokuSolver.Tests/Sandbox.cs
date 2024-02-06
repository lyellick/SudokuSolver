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

            puzzle.SaveAsPng($@"..\..\..\..\..\assets\puzzles\puzzle.png");

            (int width, int height) = (108, 110);

            int verticalOffset = 4;
            int horizontalOffset = 5;
            int borderWidth = 8;

            using var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default);

            //engine.SetVariable("tessedit_char_whitelist", "123456789");


            // revert row / col loop code and place current code above the col loop which I can then use to check each cell accross. 
            for (int row = 0; row < 9; row++)
            {
                var copy = puzzle.Clone(x => x.Grayscale().Crop(new Rectangle(0, row * (height + verticalOffset), puzzle.Width, height)));

                using MemoryStream stream = new();

                copy.SaveAsPng(stream);
                copy.SaveAsPng($@"..\..\..\..\..\assets\puzzles\R{row}.png");

                stream.Seek(0, SeekOrigin.Begin);

                using var img = Pix.LoadFromMemory(stream.ToArray());
                using var page = engine.Process(img, PageSegMode.SingleLine);
                // to figure out what part the number exists within the 9 places I need to split each cell into its cell width and height and see if it has whitespace.
                // Whitespace is an empty cell while a cell with something in it will be the next number.
                var value = page.GetText();
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
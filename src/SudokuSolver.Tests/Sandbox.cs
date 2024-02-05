using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using Tesseract;
using System.Text.RegularExpressions;
using System.IO;

namespace SudokuSolver.Tests
{
    public class Sandbox
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void CanFindOne()
        {
            var id = Guid.NewGuid();

            (int row, int col) start = (403, 21);
            (int row, int col) end = (1442, 1059);

            using var puzzle = Image.Load<Rgba32>(@"..\..\..\..\..\assets\puzzles\light\sudoku-light.jpg");

            puzzle.Mutate(x => x.Crop(new Rectangle(start.col, start.row, end.col - start.col, end.row - start.row)));

            (int width, int height) = (108, 110);

            int verticalOffset = 4;
            int horizontalOffset = 5;
            int borderWidth = 8;

            using var engine = new TesseractEngine(@"C:\Program Files\Tesseract-OCR\tessdata", "eng", EngineMode.Default);
            engine.SetVariable("tessedit_char_whitelist", "0123456789");

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int startCol = 0 + col * (width + horizontalOffset) + borderWidth;
                    int startRow = 0 + row * (height + verticalOffset) + borderWidth;
                    int endCol = startCol + width;
                    int endRow = startRow + height;

                    var copy = puzzle.Clone(x => x.Grayscale().Crop(new Rectangle(startCol, startRow, endCol - startCol, endRow - startRow)));

                    using MemoryStream stream = new();

                    copy.SaveAsPng(stream);

                    stream.Seek(0, SeekOrigin.Begin);

                    using var img = Pix.LoadFromMemory(stream.ToArray());
                    using var page = engine.Process(img, PageSegMode.SingleChar);

                    int value = !string.IsNullOrEmpty(page.GetText()) ? int.Parse(page.GetText()) : 0;
                }
            }
        }
    }
}
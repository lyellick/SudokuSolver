using SixLabors.ImageSharp;
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
    }

    public class SudokuSolverService : ISudokuSolverService
    {
        public int[][] ExtractGrid(MemoryStream imageStream)
        {
            List<List<int>> sudoku = new();

            bool show = false;

            var background = "light";

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
    }
}

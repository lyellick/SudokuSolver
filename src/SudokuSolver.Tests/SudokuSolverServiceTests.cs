using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Png;
using Tesseract;
using System.Text.RegularExpressions;
using System.IO;
using Newtonsoft.Json.Linq;
using SudokuSolver.Shared.Extensions;
using SudokuSolver.Shared.Services;

namespace SudokuSolver.Tests
{
    public class SudokuSolverServiceTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void PuzzleIsEqualToExtract()
        {
            int[][] puzzle =
{
                [9,2,0,0,4,0,0,0,1],
                [0,0,0,8,9,1,0,5,0],
                [0,5,1,7,0,6,3,0,0],
                [0,6,0,0,1,0,4,0,7],
                [0,4,0,0,8,7,0,0,0],
                [1,0,0,2,5,4,6,8,0],
                [6,0,0,0,0,0,0,0,0],
                [0,3,4,1,0,0,0,9,0],
                [2,0,9,5,3,8,7,6,0]
            };


            var extract = new SudokuSolverService()
                .ExtractGrid(@$"..\..\..\..\..\assets\puzzles\sudoku-light.jpg");

            Assert.That(puzzle.IsEqualTo(extract), Is.True);

            // TODO:
            // Select cell based on row and column.
            // Get 3x3 cells based on cell.
            // Get horizontal and vertical cells.
        }
    }
}
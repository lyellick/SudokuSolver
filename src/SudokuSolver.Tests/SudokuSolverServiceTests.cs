using SudokuSolver.Shared.Extensions;
using SudokuSolver.Shared.Services;

namespace SudokuSolver.Tests
{
    public class SudokuSolverServiceTests
    {
        private ISudokuSolverService _service;
        private int[][] _puzzle;

        [SetUp]
        public void Setup()
        {
            _service = new SudokuSolverService();
            _puzzle =
            [
                [9, 2, 0, 0, 4, 0, 0, 0, 1],
                [0, 0, 0, 8, 9, 1, 0, 5, 0],
                [0, 5, 1, 7, 0, 6, 3, 0, 0],
                [0, 6, 0, 0, 1, 0, 4, 0, 7],
                [0, 4, 0, 0, 8, 7, 0, 0, 0],
                [1, 0, 0, 2, 5, 4, 6, 8, 0],
                [6, 0, 0, 0, 0, 0, 0, 0, 0],
                [0, 3, 4, 1, 0, 0, 0, 9, 0],
                [2, 0, 9, 5, 3, 8, 7, 6, 0]
            ];
        }

        [Test]
        public void PuzzleIsEqualToExtract()
        {
            var extract = _service
                .ExtractGrid(@$"..\..\..\..\..\assets\puzzles\sudoku-light.jpg");

            Assert.That(_puzzle.IsEqualTo(extract), Is.True);
        }

        [Test]
        public void PuzzleSectionIsEqualToExtractSection()
        {
            var puzzleSection = _service.GetSection((4,1), _puzzle);

            var extract = _service
                .ExtractGrid(@$"..\..\..\..\..\assets\puzzles\sudoku-light.jpg");

            var extractSection = _service.GetSection((4, 1), extract);

            Assert.That(puzzleSection.IsEqualTo(extractSection), Is.True);
        }

        [Test]
        public void PuzzleRowIsEqualToExtractRow()
        {
            var puzzleSection = _service.GetRow((4, 1), _puzzle);

            var extract = _service
                .ExtractGrid(@$"..\..\..\..\..\assets\puzzles\sudoku-light.jpg");

            var extractSection = _service.GetRow((4, 1), extract);

            Assert.That(puzzleSection.IsEqualTo(extractSection), Is.True);
        }

        [Test]
        public void PuzzleColumnIsEqualToExtractColumn()
        {
            var puzzleSection = _service.GetColumn((4, 1), _puzzle);

            var extract = _service
                .ExtractGrid(@$"..\..\..\..\..\assets\puzzles\sudoku-light.jpg");

            var extractSection = _service.GetColumn((4, 1), extract);

            Assert.That(puzzleSection.IsEqualTo(extractSection), Is.True);
        }
    }
}
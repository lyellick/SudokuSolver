using SudokuSolver.Shared.Extensions;
using SudokuSolver.Shared.Services;

namespace SudokuSolver.Tests
{
    public class SudokuSolverServiceTests
    {
        private ISudokuSolverService _service;
        private int[][] _puzzle;
        private int[][] _answer;

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

            _answer =
            [
                [9, 2, 6, 3, 4, 5, 8, 7, 1],
                [4, 7, 3, 8, 9, 1, 2, 5, 6],
                [8, 5, 1, 7, 2, 6, 3, 4, 9],
                [5, 6, 8, 9, 1, 3, 4, 2, 7],
                [3, 4, 2, 6, 8, 7, 9, 1, 5],
                [1, 9, 7, 2, 5, 4, 6, 8, 3],
                [6, 8, 5, 4, 7, 9, 1, 3, 2],
                [7, 3, 4, 1, 6, 2, 5, 9, 8],
                [2, 1, 9, 5, 3, 8, 7, 6, 4]
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

        [Test]
        public void GetAvailableValuesIsValid()
        {
            var available = _service.GetAvailableValues((4,0), _puzzle);
        }
    }
}
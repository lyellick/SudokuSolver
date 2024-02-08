using Microsoft.AspNetCore.Mvc;
using SudokuSolver.Shared.Services;
using System.Net;

namespace SudokuSolver.Service.Controllers
{
    [Route("")]
    [ApiController]
    public class SolverController : ControllerBase
    {
        private readonly ILogger<SolverController> _logger;
        private readonly ISudokuSolverService _service;

        public SolverController(ILogger<SolverController> logger, ISudokuSolverService service)
        {
            _logger = logger;
            _service = service;
        }

        [HttpPost]
        [Route("solve")]
        public async Task<IActionResult> SolveAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                using var stream = new MemoryStream();

                await file.CopyToAsync(stream);

                stream.Position = 0;

                var puzzle = _service.ExtractGrid(stream);

                var answer = _service.Backtrack(puzzle);

                var result = new { puzzle, answer };

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to process puzzle.");
                return BadRequest();
            }
        }
    }
}

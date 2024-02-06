using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;

namespace SudokuSolver.Shared.Extensions
{
    public static class ImageRgba32Extension
    {
        public static void FloodFill(this Image<Rgba32> image, int startCol, int startRow, Color fill)
        {
            Queue<(int, int)> queue = new();
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

        public static bool IsColorPresent(this Image<Rgba32> cell, Rgba32 target)
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

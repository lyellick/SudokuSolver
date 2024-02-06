namespace SudokuSolver.Shared.Extensions
{
    public static class JaggedArrayExtension
    {
        public static bool IsEqualTo(this int[][] target, int[][] to)
        {
            if (target.Length != to.Length)
                return false;

            for (int row = 0; row < target.Length; row++)
            {
                if (target[row].Length != to[row].Length)
                    return false;

                for (int col = 0; col < target[row].Length; col++)
                {
                    if (target[row][col] != to[row][col])
                        return false;
                }
            }

            return true;
        }
    }
}

public class UniqueSolutionChecker
{
    private int solutionCount = 0;

    public bool HasUniqueSolution(string puzzle)
    {
        int[,] grid = new int[9, 9];

        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                grid[r, c] = puzzle[r * 9 + c] - '0';

        solutionCount = 0;
        Solve(grid);
        return solutionCount == 1;
    }

    private void Solve(int[,] grid)
    {
        // İkinci çözüm bulunduysa aramayı durdur
        if (solutionCount > 1) return;

        // Boş hücre bul
        int row = -1, col = -1;
        for (int r = 0; r < 9 && row == -1; r++)
            for (int c = 0; c < 9 && row == -1; c++)
                if (grid[r, c] == 0) { row = r; col = c; }

        // Boş hücre yoksa çözüm bulundu
        if (row == -1) { solutionCount++; return; }

        for (int digit = 1; digit <= 9; digit++)
        {
            if (solutionCount > 1) return;

            if (IsValid(grid, row, col, digit))
            {
                grid[row, col] = digit;
                Solve(grid);
                grid[row, col] = 0;
            }
        }
    }

    private bool IsValid(int[,] grid, int row, int col, int digit)
    {
        for (int c = 0; c < 9; c++)
            if (grid[row, c] == digit) return false;

        for (int r = 0; r < 9; r++)
            if (grid[r, col] == digit) return false;

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (grid[r, c] == digit) return false;

        return true;
    }
}
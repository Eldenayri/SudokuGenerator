using System.Collections.Generic;

public class BasicCalculate
{
    public void CalculateCandidates(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                allCellCandidates[r, c] = cells[r, c].IsFixed()
                    ? new List<int>()
                    : GetCandidates(r, c, cells);
    }

    public void ReCalculateCandidates(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (allCellCandidates[r, c] == null) continue;

                List<int> valid = GetCandidates(r, c, cells);
                allCellCandidates[r, c].RemoveAll(x => !valid.Contains(x));
            }
        }
    }

    public List<int> GetCandidates(int row, int col, SudokuCell[,] cells)
    {
        List<int> candidates = new List<int>();
        for (int number = 1; number <= 9; number++)
            if (IsValid(row, col, number, cells))
                candidates.Add(number);
        return candidates;
    }

    public bool IsValid(int row, int col, int number, SudokuCell[,] cells)
    {
        for (int c = 0; c < 9; c++)
            if (cells[row, c].IsFixed() && cells[row, c].GetValue() == number)
                return false;

        for (int r = 0; r < 9; r++)
            if (cells[r, col].IsFixed() && cells[r, col].GetValue() == number)
                return false;

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (cells[r, c].IsFixed() && cells[r, c].GetValue() == number)
                    return false;

        return true;
    }
}
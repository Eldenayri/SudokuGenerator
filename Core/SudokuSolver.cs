using System;
using System.Collections.Generic;

public class SudokuSolver
{
    private sealed class TechniqueSpec
    {
        public required string Name { get; init; }
        public required Func<List<int>[,], SudokuCell[,], bool> Fill { get; init; }
    }

    private readonly BasicCalculate basicCalculate = new BasicCalculate();
    private readonly List<TechniqueSpec> techniques;

    public SudokuSolver()
    {
        techniques = new List<TechniqueSpec>
        {
            new TechniqueSpec { Name = "NakedPairs", Fill = new NakedPairs().Fill },
            new TechniqueSpec { Name = "HiddenPairs", Fill = new HiddenPairs().Fill },
            // PointingPairs.Fill hem Pointing Pair hem Pointing Triple'ı kapsar.
            new TechniqueSpec { Name = "PointingPairs", Fill = new PointingPairs().Fill },
            new TechniqueSpec { Name = "BoxLineReduction", Fill = new BoxLineReduction().Fill },
        };
    }

    public SolveResult Solve(string puzzle, ISet<string>? enabledTechniques = null)
    {
        enabledTechniques ??= new HashSet<string>(TechniqueCatalog.RequiredForLevel(6));

        var cells = new SudokuCell[9, 9];
        var allCellCandidates = new List<int>[9, 9];

        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                int value = puzzle[row * 9 + col] - '0';
                cells[row, col] = new SudokuCell();
                cells[row, col].Setup(row, col, value, value != 0);
                allCellCandidates[row, col] = new List<int>();
            }
        }

        basicCalculate.CalculateCandidates(allCellCandidates, cells);
        var result = new SolveResult();

        while (true)
        {
            if (enabledTechniques.Contains("NakedSingle") &&
                FillNakedSingle(allCellCandidates, cells))
            {
                result.Increment("NakedSingle");
                if (IsSolved(cells))
                {
                    result.Solved = true;
                    break;
                }
                continue;
            }

            if (enabledTechniques.Contains("HiddenSingle") &&
                FillHiddenSingle(allCellCandidates, cells))
            {
                result.Increment("HiddenSingle");
                if (IsSolved(cells))
                {
                    result.Solved = true;
                    break;
                }
                continue;
            }

            bool techniqueApplied = false;
            foreach (TechniqueSpec technique in techniques)
            {
                if (!enabledTechniques.Contains(technique.Name))
                    continue;

                if (technique.Fill(allCellCandidates, cells))
                {
                    result.Increment(technique.Name);
                    techniqueApplied = true;
                    break;
                }
            }

            if (!techniqueApplied)
                break;
        }

        result.DifficultyLevel = CalculateDifficulty(result);

        if (result.Solved)
        {
            var finalGrid = new char[81];
            for (int row = 0; row < 9; row++)
                for (int col = 0; col < 9; col++)
                    finalGrid[row * 9 + col] = (char)(cells[row, col].GetValue() + '0');

            result.SolutionString = new string(finalGrid);
        }

        return result;
    }

    private void PlaceValue(
        int row,
        int col,
        int value,
        List<int>[,] allCellCandidates,
        SudokuCell[,] cells)
    {
        cells[row, col].SetValue(value);
        cells[row, col].SetFixed(true);
        allCellCandidates[row, col].Clear();
        basicCalculate.UpdateNeighbours(row, col, value, allCellCandidates, cells);
    }

    private bool FillNakedSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int col = 0; col < 9; col++)
            {
                if (cells[row, col].IsFixed() || allCellCandidates[row, col].Count != 1)
                    continue;

                PlaceValue(row, col, allCellCandidates[row, col][0], allCellCandidates, cells);
                return true;
            }
        }

        return false;
    }

    private bool FillHiddenSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        for (int row = 0; row < 9; row++)
        {
            for (int digit = 1; digit <= 9; digit++)
            {
                int count = 0;
                int lastCol = -1;
                for (int col = 0; col < 9; col++)
                {
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                    {
                        count++;
                        lastCol = col;
                    }
                }

                if (count == 1)
                {
                    PlaceValue(row, lastCol, digit, allCellCandidates, cells);
                    return true;
                }
            }
        }

        for (int col = 0; col < 9; col++)
        {
            for (int digit = 1; digit <= 9; digit++)
            {
                int count = 0;
                int lastRow = -1;
                for (int row = 0; row < 9; row++)
                {
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                    {
                        count++;
                        lastRow = row;
                    }
                }

                if (count == 1)
                {
                    PlaceValue(lastRow, col, digit, allCellCandidates, cells);
                    return true;
                }
            }
        }

        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                for (int digit = 1; digit <= 9; digit++)
                {
                    int count = 0;
                    int lastRow = -1;
                    int lastCol = -1;

                    for (int row = boxRow * 3; row < boxRow * 3 + 3; row++)
                    {
                        for (int col = boxCol * 3; col < boxCol * 3 + 3; col++)
                        {
                            if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                            {
                                count++;
                                lastRow = row;
                                lastCol = col;
                            }
                        }
                    }

                    if (count == 1)
                    {
                        PlaceValue(lastRow, lastCol, digit, allCellCandidates, cells);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static bool IsSolved(SudokuCell[,] cells)
    {
        for (int row = 0; row < 9; row++)
            for (int col = 0; col < 9; col++)
                if (!cells[row, col].IsFixed() || cells[row, col].GetValue() == 0)
                    return false;

        return true;
    }

    private static int CalculateDifficulty(SolveResult result)
    {
        if (!result.Solved)
            return -1;

        for (int level = 6; level >= 1; level--)
            if (result.Get(TechniqueCatalog.TechniqueForLevel(level)) > 0)
                return level;

        return -1;
    }
}

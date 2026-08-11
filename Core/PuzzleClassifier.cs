using System.Collections.Generic;

public class PuzzleClassifier
{
    private readonly SudokuSolver solver;

    public PuzzleClassifier(SudokuSolver solver)
    {
        this.solver = solver;
    }

    public bool MatchesLevel(string puzzle, int level)
    {
        IReadOnlyList<string> required = TechniqueCatalog.RequiredForLevel(level);
        var enabled = new HashSet<string>(required);
        SolveResult result = solver.Solve(puzzle, enabled);

        if (!result.Solved)
            return false;

        // "Hem ... hem ..." şartı: level'a kadarki her teknik doğal çözüm
        // izinde en az bir kez gerçekten uygulanmış olmalı.
        foreach (string technique in required)
            if (result.Get(technique) == 0)
                return false;

        // Bir önceki teknik kümesi yeterliyse bulmaca daha kolay level'a aittir.
        if (level > 1)
        {
            var easier = new HashSet<string>(TechniqueCatalog.RequiredForLevel(level - 1));
            if (solver.Solve(puzzle, easier).Solved)
                return false;
        }

        return true;
    }

    public int? Assign(string puzzle, ISet<int> openLevels)
    {
        for (int level = 6; level >= 1; level--)
            if (openLevels.Contains(level) && MatchesLevel(puzzle, level))
                return level;

        return null;
    }
}

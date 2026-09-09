using System.Collections.Generic;

public class PuzzleClassifier
{
    private static readonly string[] Level6AdvancedTechniques =
    {
        "NakedPairs",
        "HiddenPairs",
        "PointingPairs",
        "BoxLineReduction",
    };

    public const int Level1MinimumEmptyCells = 43;
    public const int Level1MaximumEmptyCells = 44;
    public const int Level1MinimumNakedSingleCells = 5;
    public const int Level1DensityCheckUntilEmptyCells = 10;
    public const int Level2MinimumEmptyCells = 45;
    public const int Level2MaximumEmptyCells = 46;
    public const int Level2MinimumNakedSingleCells = 3;
    public const int Level2MaximumNakedSingleCells = 6;
    public const int Level2DensityCheckUntilEmptyCells = 15;
    public const int Level3MinimumEmptyCells = 47;
    public const int Level3MaximumEmptyCells = 48;
    public const int Level3MinimumHiddenSingleUses = 1;
    public const int Level3MaximumHiddenSingleUses = 2;
    public const int Level4MinimumEmptyCells = 49;
    public const int Level4MaximumEmptyCells = 50;
    public const int Level4MinimumHiddenSingleUses = 2;
    public const int Level4MaximumHiddenSingleUses = 3;
    public const int Level5MinimumEmptyCells = 51;
    public const int Level5MaximumEmptyCells = 53;
    public const int Level5RequiredAdvancedTechniqueUses = 1;
    public const int Level6MinimumEmptyCells = 54;
    public const int Level6MaximumEmptyCells = 56;
    public const int Level6RequiredAdvancedTechniqueUses = 2;

    private readonly SudokuSolver solver;

    public PuzzleClassifier(SudokuSolver solver)
    {
        this.solver = solver;
    }

    public bool MatchesLevel(string puzzle, int level)
    {
        if (level == 1)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level1MinimumEmptyCells
                || emptyCells > Level1MaximumEmptyCells)
                return false;
        }

        else if (level == 2)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level2MinimumEmptyCells
                || emptyCells > Level2MaximumEmptyCells)
                return false;
        }
        else if (level == 3)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level3MinimumEmptyCells
                || emptyCells > Level3MaximumEmptyCells)
                return false;
        }
        else if (level == 4)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level4MinimumEmptyCells
                || emptyCells > Level4MaximumEmptyCells)
                return false;
        }
        else if (level == 5)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level5MinimumEmptyCells
                || emptyCells > Level5MaximumEmptyCells)
                return false;
        }
        else if (level == 6)
        {
            int emptyCells = CountEmptyCells(puzzle);
            if (emptyCells < Level6MinimumEmptyCells
                || emptyCells > Level6MaximumEmptyCells)
                return false;
        }

        IReadOnlyList<string> required = level switch
        {
            2 => new[] { "NakedSingle" },
            3 => new[] { "NakedSingle", "HiddenSingle" },
            4 => new[] { "NakedSingle", "HiddenSingle" },
            5 => TechniqueCatalog.RequiredForLevel(6),
            _ => TechniqueCatalog.RequiredForLevel(level),
        };
        var enabled = new HashSet<string>(required);
        SolveResult result = solver.Solve(puzzle, enabled);

        if (!result.Solved)
            return false;

        if (level == 3)
        {
            if (result.HiddenSingle < Level3MinimumHiddenSingleUses
                || result.HiddenSingle > Level3MaximumHiddenSingleUses)
                return false;
        }
        else if (level == 4)
        {
            if (result.HiddenSingle < Level4MinimumHiddenSingleUses
                || result.HiddenSingle > Level4MaximumHiddenSingleUses)
                return false;
        }
        else if (level == 5)
        {
            if (!HasTotalAdvancedTechniqueUses(
                result,
                Level5RequiredAdvancedTechniqueUses))
                return false;
        }
        else if (level == 6)
        {
            if (!HasTotalAdvancedTechniqueUses(
                result,
                Level6RequiredAdvancedTechniqueUses))
                return false;
        }
        else
        {
            // Diğer seviyelerde gerekli her teknik doğal çözüm izinde
            // en az bir kez gerçekten uygulanmış olmalı.
            foreach (string technique in required)
                if (result.Get(technique) == 0)
                    return false;
        }

        if (level == 1
            && !result.MeetsNakedSingleDensity(
                Level1MinimumNakedSingleCells,
                Level1DensityCheckUntilEmptyCells))
            return false;

        if (level == 2
            && !result.MeetsNakedSingleRange(
                Level2MinimumNakedSingleCells,
                Level2MaximumNakedSingleCells,
                Level2DensityCheckUntilEmptyCells))
            return false;

        return true;
    }

    public string TechniqueForAcceptedPuzzle(string puzzle, int level)
    {
        if (level < 5)
        {
            return level switch
            {
                2 => "NakedSingle",
                3 => "HiddenSingle",
                4 => "HiddenSingle",
                _ => TechniqueCatalog.TechniqueForLevel(level),
            };
        }

        var enabled = new HashSet<string>(TechniqueCatalog.RequiredForLevel(6));
        SolveResult result = solver.Solve(puzzle, enabled);
        var usedTechniques = new List<string>();
        foreach (string technique in Level6AdvancedTechniques)
            for (int use = 0; use < result.Get(technique); use++)
                usedTechniques.Add(technique);

        if (usedTechniques.Count == 0)
            throw new System.InvalidOperationException(
                $"Kabul edilmiş Level {level} bulmacasında ileri teknik bulunamadı.");

        return string.Join("+", usedTechniques);
    }

    private static bool HasTotalAdvancedTechniqueUses(
        SolveResult result,
        int requiredTotalUses)
    {
        int totalUses = 0;
        foreach (string technique in Level6AdvancedTechniques)
            totalUses += result.Get(technique);

        return totalUses == requiredTotalUses;
    }

    private static int CountEmptyCells(string puzzle)
    {
        int count = 0;
        foreach (char value in puzzle)
            if (value == '0')
                count++;

        return count;
    }

    public int? Assign(string puzzle, ISet<int> openLevels)
    {
        for (int level = 6; level >= 1; level--)
            if (openLevels.Contains(level) && MatchesLevel(puzzle, level))
                return level;

        return null;
    }
}

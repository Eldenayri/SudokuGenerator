using System.Collections.Generic;

public class SolveResult
{
    private readonly List<(int EmptyCells, int NakedSingleCells)> nakedSingleStages = new();

    public bool Solved { get; set; }
    public int DifficultyLevel { get; set; }
    public int NakedSingle { get; set; }
    public int HiddenSingle { get; set; }
    public int NakedPairs { get; set; }
    public int HiddenPairs { get; set; }
    public int PointingPairs { get; set; }
    public int BoxLineReduction { get; set; }
    public string? SolutionString { get; set; }
    public IReadOnlyList<(int EmptyCells, int NakedSingleCells)> NakedSingleStages =>
        nakedSingleStages;

    public void RecordNakedSingleStage(int emptyCells, int nakedSingleCells)
    {
        nakedSingleStages.Add((emptyCells, nakedSingleCells));
    }

    public bool MeetsNakedSingleDensity(
        int minimumNakedSingleCells,
        int checkUntilEmptyCells)
    {
        return MeetsNakedSingleRange(
            minimumNakedSingleCells,
            int.MaxValue,
            checkUntilEmptyCells);
    }

    public bool MeetsNakedSingleRange(
        int minimumNakedSingleCells,
        int maximumNakedSingleCells,
        int checkUntilEmptyCells)
    {
        foreach ((int emptyCells, int nakedSingleCells) in nakedSingleStages)
        {
            if (emptyCells >= checkUntilEmptyCells
                && (nakedSingleCells < minimumNakedSingleCells
                    || nakedSingleCells > maximumNakedSingleCells))
                return false;
        }

        return true;
    }

    public void Increment(string techniqueName)
    {
        switch (techniqueName)
        {
            case "NakedSingle": NakedSingle++; break;
            case "HiddenSingle": HiddenSingle++; break;
            case "NakedPairs": NakedPairs++; break;
            case "HiddenPairs": HiddenPairs++; break;
            case "PointingPairs": PointingPairs++; break;
            case "BoxLineReduction": BoxLineReduction++; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(techniqueName));
        }
    }

    public int Get(string techniqueName)
    {
        return techniqueName switch
        {
            "NakedSingle" => NakedSingle,
            "HiddenSingle" => HiddenSingle,
            "NakedPairs" => NakedPairs,
            "HiddenPairs" => HiddenPairs,
            "PointingPairs" => PointingPairs,
            "BoxLineReduction" => BoxLineReduction,
            _ => 0,
        };
    }
}

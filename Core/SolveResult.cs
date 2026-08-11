public class SolveResult
{
    public bool Solved { get; set; }
    public int DifficultyLevel { get; set; }
    public int NakedSingle { get; set; }
    public int HiddenSingle { get; set; }
    public int NakedPairs { get; set; }
    public int HiddenPairs { get; set; }
    public int PointingPairs { get; set; }
    public int BoxLineReduction { get; set; }
    public string? SolutionString { get; set; }

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

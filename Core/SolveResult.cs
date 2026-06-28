using System.Collections.Generic;

public class SolveResult
{
    public bool Solved { get; set; }
    public int DifficultyLevel { get; set; } // 1-6
    public int NakedPairs { get; set; }
    public int NakedTriples { get; set; }
    public int NakedQuads { get; set; }
    public int HiddenPairs { get; set; }
    public int HiddenTriples { get; set; }
    public int HiddenQuads { get; set; }
    public int PointingPairs { get; set; }
    public int BoxLineReduction { get; set; }
    public int XWing { get; set; }
    public int Swordfish { get; set; }
    public int Jellyfish { get; set; }
    public int YWing { get; set; }
    public int XYZWing { get; set; }
    public int UniqueRectangle { get; set; }
    public int Skyscraper { get; set; }
    public int XYChain { get; set; }
    public int WWing { get; set; }
    public int SimpleColouring { get; set; }
    public int BUG { get; set; }
    public string SolutionString { get; set; }
}

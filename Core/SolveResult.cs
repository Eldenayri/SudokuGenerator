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

    // Teknik adı <-> sayaç eşlemesi tek yerde tutulur (SudokuSolver ve
    // PuzzleClassifier bu ikisini kullanır, ayrı ayrı switch tekrarlamaz).
    public void Increment(string techniqueName)
    {
        switch (techniqueName)
        {
            case "NakedPairs": NakedPairs++; break;
            case "HiddenPairs": HiddenPairs++; break;
            case "PointingPairs": PointingPairs++; break;
            case "BoxLineReduction": BoxLineReduction++; break;
            case "XWing": XWing++; break;
            case "NakedTriples": NakedTriples++; break;
            case "HiddenTriples": HiddenTriples++; break;
            case "YWing": YWing++; break;
            case "Swordfish": Swordfish++; break;
            case "NakedQuads": NakedQuads++; break;
            case "HiddenQuads": HiddenQuads++; break;
            case "Skyscraper": Skyscraper++; break;
            case "XYZWing": XYZWing++; break;
            case "UniqueRectangle": UniqueRectangle++; break;
            case "Jellyfish": Jellyfish++; break;
            case "WWing": WWing++; break;
            case "XYChain": XYChain++; break;
            case "SimpleColouring": SimpleColouring++; break;
            case "BUG": BUG++; break;
        }
    }

    public int Get(string techniqueName)
    {
        switch (techniqueName)
        {
            case "NakedPairs": return NakedPairs;
            case "HiddenPairs": return HiddenPairs;
            case "PointingPairs": return PointingPairs;
            case "BoxLineReduction": return BoxLineReduction;
            case "XWing": return XWing;
            case "NakedTriples": return NakedTriples;
            case "HiddenTriples": return HiddenTriples;
            case "YWing": return YWing;
            case "Swordfish": return Swordfish;
            case "NakedQuads": return NakedQuads;
            case "HiddenQuads": return HiddenQuads;
            case "Skyscraper": return Skyscraper;
            case "XYZWing": return XYZWing;
            case "UniqueRectangle": return UniqueRectangle;
            case "Jellyfish": return Jellyfish;
            case "WWing": return WWing;
            case "XYChain": return XYChain;
            case "SimpleColouring": return SimpleColouring;
            case "BUG": return BUG;
            default: return 0;
        }
    }
}

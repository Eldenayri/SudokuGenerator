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
    public string SolutionString { get; set; }
}

public class SudokuSolver
{
    private BasicCalculate basicCalculate = new BasicCalculate();
    private NakedPairs nakedPairs = new NakedPairs();
    private HiddenPairs hiddenPairs = new HiddenPairs();
    private PointingPairs pointingPairs = new PointingPairs();
    private BoxLineReduction boxLineReduction = new BoxLineReduction();
    private XWing xWing = new XWing();
    private Swordfish swordfish = new Swordfish();
    private NakedTriples nakedTriples = new NakedTriples();
    private HiddenTriples hiddenTriples = new HiddenTriples();
    private YWing yWing = new YWing();
    private XYZWing xYZWing = new XYZWing();
    private UniqueRectangle uniqueRectangle = new UniqueRectangle();
    private Skyscraper skyscraper = new Skyscraper();
    private XYChain xYChain = new XYChain();
    private NakedQuads nakedQuads = new NakedQuads();
    private HiddenQuads hiddenQuads = new HiddenQuads();
    private WWing wWing = new WWing();
    private Jellyfish jellyfish = new Jellyfish();
    private SimpleColouring simpleColouring = new SimpleColouring();

    public SolveResult Solve(string puzzle)
    {
        SudokuCell[,] cells = new SudokuCell[9, 9];
        List<int>[,] allCellCandidates = new List<int>[9, 9];

        // Puzzle'ı yükle
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int val = puzzle[r * 9 + c] - '0';
                cells[r, c] = new SudokuCell();
                cells[r, c].Setup(r, c, val, val != 0);
                allCellCandidates[r, c] = new List<int>();
            }
        }

        basicCalculate.CalculateCandidates(allCellCandidates, cells);

        var result = new SolveResult();

        while (true)
        {
            basicCalculate.ReCalculateCandidates(allCellCandidates, cells);

            if (FillNakedSingle(allCellCandidates, cells))
            {
                if (IsSolved(cells)) { result.Solved = true; break; }
                continue;
            }

            if (FillHiddenSingle(allCellCandidates, cells))
            {
                if (IsSolved(cells)) { result.Solved = true; break; }
                continue;
            }

            bool changed = false;

            if (nakedPairs.Fill(allCellCandidates, cells)) { result.NakedPairs++; changed = true; continue; }
            if (hiddenPairs.Fill(allCellCandidates, cells)) { result.HiddenPairs++; changed = true; continue; }
            if (pointingPairs.Fill(allCellCandidates, cells)) { result.PointingPairs++; changed = true; continue; }
            if (boxLineReduction.Fill(allCellCandidates, cells)) { result.BoxLineReduction++; changed = true; continue; }
            if (xWing.Fill(allCellCandidates, cells)) { result.XWing++; changed = true; continue; }
            if (swordfish.Fill(allCellCandidates, cells)) { result.Swordfish++; changed = true; continue; }
            if (nakedTriples.Fill(allCellCandidates, cells)) { result.NakedTriples++; changed = true; continue; }
            if (hiddenTriples.Fill(allCellCandidates, cells)) { result.HiddenTriples++; changed = true; continue; }
            if (yWing.Fill(allCellCandidates, cells)) { result.YWing++; changed = true; continue; }
            if (xYZWing.Fill(allCellCandidates, cells)) { result.XYZWing++; changed = true; continue; }
            if (uniqueRectangle.Fill(allCellCandidates, cells)) { result.UniqueRectangle++; changed = true; continue; }
            if (skyscraper.Fill(allCellCandidates, cells)) { result.Skyscraper++; changed = true; continue; }
            if (xYChain.Fill(allCellCandidates, cells)) { result.XYChain++; changed = true; continue; }
            if (nakedQuads.Fill(allCellCandidates, cells)) { result.NakedQuads++; changed = true; continue; }
            if (hiddenQuads.Fill(allCellCandidates, cells)) { result.HiddenQuads++; changed = true; continue; }
            if (wWing.Fill(allCellCandidates, cells)) { result.WWing++; changed = true; continue; }
            if (jellyfish.Fill(allCellCandidates, cells)) { result.Jellyfish++; changed = true; continue; }
            if (simpleColouring.Fill(allCellCandidates, cells)) { result.SimpleColouring++; changed = true; continue; }

            break; // Hiçbir teknik işe yaramadı
        }

        result.DifficultyLevel = CalculateDifficulty(result);

        if (result.Solved)
        {
            char[] finalGrid = new char[81];
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    // Not: SudokuCell sınıfınızın içindeki değeri okuyan özellik adı veya metot neyse onu kullanın.
                    // Örneğin cells[r, c].Value veya cells[r, c].GetValue() olabilir. 
                    // Aşağıdaki 'Value' kısmını kendi sınıfınıza göre düzenleyin:
                    int cellValue = cells[r, c].GetValue();
                    finalGrid[r * 9 + c] = (char)(cellValue + '0');
                }
            }
            result.SolutionString = new string(finalGrid);
        }

        return result;
    }

    private bool FillNakedSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        // bool found = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (cells[r, c].IsFixed()) continue;
                if (allCellCandidates[r, c].Count == 1)
                {
                    int val = allCellCandidates[r, c][0];
                    cells[r, c].SetValue(val);
                    cells[r, c].SetFixed(true);
                    allCellCandidates[r, c].Clear();
                    return true;
                }
            }
        }
        return false;
    }

    private bool FillHiddenSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
       // bool found = false;

        // Satır
        for (int r = 0; r < 9; r++)
        {
            for (int digit = 1; digit <= 9; digit++)
            {
                int count = 0, lastC = -1;
                for (int c = 0; c < 9; c++)
                    if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                    { count++; lastC = c; }
                if (count == 1)
                {
                    cells[r, lastC].SetValue(digit);
                    cells[r, lastC].SetFixed(true);
                    allCellCandidates[r, lastC].Clear();
                    return true;
                }
            }
        }

        // Sütun
        for (int c = 0; c < 9; c++)
        {
            for (int digit = 1; digit <= 9; digit++)
            {
                int count = 0, lastR = -1;
                for (int r = 0; r < 9; r++)
                    if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                    { count++; lastR = r; }
                if (count == 1)
                {
                    cells[lastR, c].SetValue(digit);
                    cells[lastR, c].SetFixed(true);
                    allCellCandidates[lastR, c].Clear();
                    return true;
                }
            }
        }

        // Blok
        for (int boxR = 0; boxR < 3; boxR++)
        {
            for (int boxC = 0; boxC < 3; boxC++)
            {
                for (int digit = 1; digit <= 9; digit++)
                {
                    int count = 0, lastR = -1, lastC = -1;
                    for (int r = boxR * 3; r < boxR * 3 + 3; r++)
                        for (int c = boxC * 3; c < boxC * 3 + 3; c++)
                            if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                            { count++; lastR = r; lastC = c; }
                    if (count == 1)
                    {
                        cells[lastR, lastC].SetValue(digit);
                        cells[lastR, lastC].SetFixed(true);
                        allCellCandidates[lastR, lastC].Clear();
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsSolved(SudokuCell[,] cells)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (!cells[r, c].IsFixed()) return false;
        return true;
    }

    private int CalculateDifficulty(SolveResult r)
    {
        if (!r.Solved) return -1;

        // En üst seviye kullanılan tekniğe göre zorluk belirle
        if (r.Jellyfish > 0 || r.WWing > 0 || r.XYChain > 0 || r.SimpleColouring > 0)
            return 6;

        if (r.Skyscraper > 0 || r.XYZWing > 0 || r.UniqueRectangle > 0)
            return 5;

        if (r.YWing > 0 || r.Swordfish > 0 || r.NakedQuads > 0 || r.HiddenQuads > 0)
            return 4;

        if (r.XWing > 0 || r.NakedTriples > 0 || r.HiddenTriples > 0)
            return 3;

        if (r.NakedPairs > 0 || r.HiddenPairs > 0 || r.PointingPairs > 0 || r.BoxLineReduction > 0)
            return 2;

        return 1;
    }
}
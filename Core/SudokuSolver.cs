using System.Collections.Generic;


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
        private JellyFish jellyfish = new JellyFish();
        private SimpleColouring simpleColouring = new SimpleColouring();
        private BUG bug = new BUG();

        public SolveResult Solve(string puzzle)
        {
            var cells = new SudokuCell[9, 9];
            var allCellCandidates = new List<int>[9, 9];

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

            // İlk aday hesaplama
            basicCalculate.CalculateCandidates(allCellCandidates, cells);

            var result = new SolveResult();

            while (true)
            {
                // FIX 3: Artık her döngüde ReCalculateCandidates YOK.
                // NakedSingle/HiddenSingle yerleştirince UpdateNeighbours çağırıyor,
                // teknikler ise zaten aday siliyor — ReCalculate sadece teknikler sonrası gerekirse çağrılır.

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

                // Teknikler aday siler; birisi başarılı olursa başa dön
                // (teknikler kendi içinde aday siliyor, ReCalculate gerekmez)
                if (nakedPairs.Fill(allCellCandidates, cells)) { result.NakedPairs++; continue; }
                if (hiddenPairs.Fill(allCellCandidates, cells)) { result.HiddenPairs++; continue; }
                if (pointingPairs.Fill(allCellCandidates, cells)) { result.PointingPairs++; continue; }
                if (boxLineReduction.Fill(allCellCandidates, cells)) { result.BoxLineReduction++; continue; }
                if (xWing.Fill(allCellCandidates, cells)) { result.XWing++; continue; }
                if (swordfish.Fill(allCellCandidates, cells)) { result.Swordfish++; continue; }
                if (nakedTriples.Fill(allCellCandidates, cells)) { result.NakedTriples++; continue; }
                if (hiddenTriples.Fill(allCellCandidates, cells)) { result.HiddenTriples++; continue; }
                if (yWing.Fill(allCellCandidates, cells)) { result.YWing++; continue; }
                if (xYZWing.Fill(allCellCandidates, cells)) { result.XYZWing++; continue; }
                if (uniqueRectangle.Fill(allCellCandidates, cells)) { result.UniqueRectangle++; continue; }
                if (skyscraper.Fill(allCellCandidates, cells)) { result.Skyscraper++; continue; }
                if (xYChain.Fill(allCellCandidates, cells)) { result.XYChain++; continue; }
                if (nakedQuads.Fill(allCellCandidates, cells)) { result.NakedQuads++; continue; }
                if (hiddenQuads.Fill(allCellCandidates, cells)) { result.HiddenQuads++; continue; }
                if (wWing.Fill(allCellCandidates, cells)) { result.WWing++; continue; }
                if (jellyfish.Fill(allCellCandidates, cells)) { result.Jellyfish++; continue; }
                if (simpleColouring.Fill(allCellCandidates, cells)) { result.SimpleColouring++; continue; }
                if (bug.Fill(allCellCandidates, cells, basicCalculate)) { result.BUG++; continue; }

                break; // Hiçbir teknik işe yaramadı, puzzle çözülemedi
            }

            result.DifficultyLevel = CalculateDifficulty(result);

            if (result.Solved)
            {
                var finalGrid = new char[81];
                for (int r = 0; r < 9; r++)
                    for (int c = 0; c < 9; c++)
                        finalGrid[r * 9 + c] = (char)(cells[r, c].GetValue() + '0');

                result.SolutionString = new string(finalGrid);
            }

            return result;
        }

        // FIX 3: Hücre yerleştirince UpdateNeighbours ile komşuları güncelle
        private void PlaceValue(int r, int c, int val,
                                List<int>[,] allCellCandidates, SudokuCell[,] cells)
        {
            cells[r, c].SetValue(val);
            cells[r, c].SetFixed(true);
            allCellCandidates[r, c].Clear();
            basicCalculate.UpdateNeighbours(r, c, val, allCellCandidates, cells);
        }

        private bool FillNakedSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                {
                    if (cells[r, c].IsFixed()) continue;
                    if (allCellCandidates[r, c].Count == 1)
                    {
                        PlaceValue(r, c, allCellCandidates[r, c][0], allCellCandidates, cells);
                        return true;
                    }
                }
            return false;
        }

        private bool FillHiddenSingle(List<int>[,] allCellCandidates, SudokuCell[,] cells)
        {
            // Satır
            for (int r = 0; r < 9; r++)
                for (int digit = 1; digit <= 9; digit++)
                {
                    int count = 0, lastC = -1;
                    for (int c = 0; c < 9; c++)
                        if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                        { count++; lastC = c; }
                    if (count == 1)
                    {
                        PlaceValue(r, lastC, digit, allCellCandidates, cells);
                        return true;
                    }
                }

            // Sütun
            for (int c = 0; c < 9; c++)
                for (int digit = 1; digit <= 9; digit++)
                {
                    int count = 0, lastR = -1;
                    for (int r = 0; r < 9; r++)
                        if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                        { count++; lastR = r; }
                    if (count == 1)
                    {
                        PlaceValue(lastR, c, digit, allCellCandidates, cells);
                        return true;
                    }
                }

            // Blok
            for (int boxR = 0; boxR < 3; boxR++)
                for (int boxC = 0; boxC < 3; boxC++)
                    for (int digit = 1; digit <= 9; digit++)
                    {
                        int count = 0, lastR = -1, lastC = -1;
                        for (int r = boxR * 3; r < boxR * 3 + 3; r++)
                            for (int c = boxC * 3; c < boxC * 3 + 3; c++)
                                if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                                { count++; lastR = r; lastC = c; }
                        if (count == 1)
                        {
                            PlaceValue(lastR, lastC, digit, allCellCandidates, cells);
                            return true;
                        }
                    }

            return false;
        }

        // FIX 5: GetValue() == 0 kontrolü eklendi — fixed ama değersiz hücreye karşı güvenlik
        private bool IsSolved(SudokuCell[,] cells)
        {
            for (int r = 0; r < 9; r++)
                for (int c = 0; c < 9; c++)
                    if (!cells[r, c].IsFixed() || cells[r, c].GetValue() == 0)
                        return false;
            return true;
        }

        private int CalculateDifficulty(SolveResult r)
        {
            if (!r.Solved) return -1;

            if (r.XYChain > 0)
                return 1;

            return -1;
        }

        /*private int CalculateDifficulty(SolveResult r)
        {
            if (!r.Solved) return -1;

            if (r.Jellyfish > 0 || r.WWing > 0 || r.XYChain > 0 || r.SimpleColouring > 0 || r.BUG > 0)
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
        }*/
    }

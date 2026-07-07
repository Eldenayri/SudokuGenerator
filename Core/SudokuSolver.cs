using System;
using System.Collections.Generic;


    public class SudokuSolver
    {
        private class TechniqueSpec
        {
            public string Name;
            public int Tier;
            public Func<List<int>[,], SudokuCell[,], bool> Fill;
        }

        private BasicCalculate basicCalculate = new BasicCalculate();
        private readonly List<TechniqueSpec> techniques;

        public SudokuSolver()
        {
            var instances = new Dictionary<string, Func<List<int>[,], SudokuCell[,], bool>>
            {
                ["NakedPairs"] = new NakedPairs().Fill,
                ["HiddenPairs"] = new HiddenPairs().Fill,
                ["PointingPairs"] = new PointingPairs().Fill,
                ["BoxLineReduction"] = new BoxLineReduction().Fill,
                ["XWing"] = new XWing().Fill,
                ["NakedTriples"] = new NakedTriples().Fill,
                ["HiddenTriples"] = new HiddenTriples().Fill,
                ["YWing"] = new YWing().Fill,
                ["Swordfish"] = new Swordfish().Fill,
                ["NakedQuads"] = new NakedQuads().Fill,
                ["HiddenQuads"] = new HiddenQuads().Fill,
                ["Skyscraper"] = new Skyscraper().Fill,
                ["XYZWing"] = new XYZWing().Fill,
                ["UniqueRectangle"] = new UniqueRectangle().Fill,
                ["Jellyfish"] = new JellyFish().Fill,
                ["WWing"] = new WWing().Fill,
                ["XYChain"] = new XYChain().Fill,
                ["SimpleColouring"] = new SimpleColouring().Fill,
            };

            var bug = new BUG();
            instances["BUG"] = (cand, cells) => bug.Fill(cand, cells, basicCalculate);

            techniques = new List<TechniqueSpec>();
            foreach (var (name, tier) in TechniqueCatalog.All)
                techniques.Add(new TechniqueSpec { Name = name, Tier = tier, Fill = instances[name] });
        }

        // disabledTechniques null/boş ise tam cephanelikle (varsayılan davranış) çözer.
        public SolveResult Solve(string puzzle, ISet<string> disabledTechniques = null)
        {
            disabledTechniques ??= new HashSet<string>();

            var cells = new SudokuCell[9, 9];
            var allCellCandidates = new List<int>[9, 9];

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

                // Teknikler tier sırasıyla (kolaydan zora) denenir; ilk işe yarayan
                // uygulanır ve döngü başa döner. Sıra, zorluk derecelendirmesinin
                // doğru olması için kritik — asla zor bir teknik kolay bir tekniğin
                // önüne geçmemeli.
                bool techniqueApplied = false;
                foreach (var spec in techniques)
                {
                    if (disabledTechniques.Contains(spec.Name)) continue;
                    if (spec.Fill(allCellCandidates, cells))
                    {
                        result.Increment(spec.Name);
                        techniqueApplied = true;
                        break;
                    }
                }
                if (techniqueApplied) continue;

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

        // Bir teknik, verilen tier'de gerçekten zorunlu mu? Test: o teknik VE ondan
        // zor (tier > tier) her şey kapatılır, aynı tier'deki emsalleri ve daha
        // kolay teknikler açık kalır. Çözülemiyorsa teknik zorunludur.
        public bool IsTechniqueRequired(string puzzle, string techniqueName, int tier)
        {
            var disabled = new HashSet<string> { techniqueName };
            foreach (var (name, t) in TechniqueCatalog.All)
                if (t > tier) disabled.Add(name);

            var result = Solve(puzzle, disabled);
            return !result.Solved;
        }

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
        }
    }

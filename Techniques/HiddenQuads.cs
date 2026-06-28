using System.Collections.Generic;

public class HiddenQuads
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIRLAR
        // ======================
        for (int row = 0; row < 9; row++)
        {
            List<List<int>> numberCols = new List<List<int>>();
            for (int number = 1; number <= 9; number++)
            {
                List<int> cols = new List<int>();
                for (int col = 0; col < 9; col++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        cols.Add(col);
                numberCols.Add(cols);
            }

            for (int n1 = 1; n1 <= 9; n1++)
            {
                if (numberCols[n1 - 1].Count == 0 || numberCols[n1 - 1].Count > 4) continue;

                for (int n2 = n1 + 1; n2 <= 9; n2++)
                {
                    if (numberCols[n2 - 1].Count == 0 || numberCols[n2 - 1].Count > 4) continue;

                    for (int n3 = n2 + 1; n3 <= 9; n3++)
                    {
                        if (numberCols[n3 - 1].Count == 0 || numberCols[n3 - 1].Count > 4) continue;

                        for (int n4 = n3 + 1; n4 <= 9; n4++)
                        {
                            if (numberCols[n4 - 1].Count == 0 || numberCols[n4 - 1].Count > 4) continue;

                            HashSet<int> unionCols = new HashSet<int>(numberCols[n1 - 1]);
                            unionCols.UnionWith(numberCols[n2 - 1]);
                            unionCols.UnionWith(numberCols[n3 - 1]);
                            unionCols.UnionWith(numberCols[n4 - 1]);

                            if (unionCols.Count != 4) continue;

                            List<int> quad = new List<int> { n1, n2, n3, n4 };

                            foreach (int col in unionCols)
                            {
                                var candidates = allCellCandidates[row, col];
                                int before = candidates.Count;
                                candidates.RemoveAll(x => !quad.Contains(x));
                                if (candidates.Count != before) changed = true;
                            }
                        }
                    }
                }
            }
        }

        // ======================
        // SÜTUNLAR
        // ======================
        for (int col = 0; col < 9; col++)
        {
            List<List<int>> numberRows = new List<List<int>>();
            for (int number = 1; number <= 9; number++)
            {
                List<int> rows = new List<int>();
                for (int row = 0; row < 9; row++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        rows.Add(row);
                numberRows.Add(rows);
            }

            for (int n1 = 1; n1 <= 9; n1++)
            {
                if (numberRows[n1 - 1].Count == 0 || numberRows[n1 - 1].Count > 4) continue;

                for (int n2 = n1 + 1; n2 <= 9; n2++)
                {
                    if (numberRows[n2 - 1].Count == 0 || numberRows[n2 - 1].Count > 4) continue;

                    for (int n3 = n2 + 1; n3 <= 9; n3++)
                    {
                        if (numberRows[n3 - 1].Count == 0 || numberRows[n3 - 1].Count > 4) continue;

                        for (int n4 = n3 + 1; n4 <= 9; n4++)
                        {
                            if (numberRows[n4 - 1].Count == 0 || numberRows[n4 - 1].Count > 4) continue;

                            HashSet<int> unionRows = new HashSet<int>(numberRows[n1 - 1]);
                            unionRows.UnionWith(numberRows[n2 - 1]);
                            unionRows.UnionWith(numberRows[n3 - 1]);
                            unionRows.UnionWith(numberRows[n4 - 1]);

                            if (unionRows.Count != 4) continue;

                            List<int> quad = new List<int> { n1, n2, n3, n4 };

                            foreach (int row in unionRows)
                            {
                                var candidates = allCellCandidates[row, col];
                                int before = candidates.Count;
                                candidates.RemoveAll(x => !quad.Contains(x));
                                if (candidates.Count != before) changed = true;
                            }
                        }
                    }
                }
            }
        }

        // ======================
        // BLOKLAR
        // ======================
        for (int br = 0; br < 3; br++)
        {
            for (int bc = 0; bc < 3; bc++)
            {
                int startRow = br * 3;
                int startCol = bc * 3;

                List<List<(int r, int c)>> numberCells = new List<List<(int, int)>>();
                for (int number = 1; number <= 9; number++)
                {
                    List<(int, int)> found = new List<(int, int)>();
                    for (int r = startRow; r < startRow + 3; r++)
                        for (int c = startCol; c < startCol + 3; c++)
                            if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(number))
                                found.Add((r, c));
                    numberCells.Add(found);
                }

                for (int n1 = 1; n1 <= 9; n1++)
                {
                    if (numberCells[n1 - 1].Count == 0 || numberCells[n1 - 1].Count > 4) continue;

                    for (int n2 = n1 + 1; n2 <= 9; n2++)
                    {
                        if (numberCells[n2 - 1].Count == 0 || numberCells[n2 - 1].Count > 4) continue;

                        for (int n3 = n2 + 1; n3 <= 9; n3++)
                        {
                            if (numberCells[n3 - 1].Count == 0 || numberCells[n3 - 1].Count > 4) continue;

                            for (int n4 = n3 + 1; n4 <= 9; n4++)
                            {
                                if (numberCells[n4 - 1].Count == 0 || numberCells[n4 - 1].Count > 4) continue;

                                HashSet<(int, int)> unionCells = new HashSet<(int, int)>(numberCells[n1 - 1]);
                                unionCells.UnionWith(numberCells[n2 - 1]);
                                unionCells.UnionWith(numberCells[n3 - 1]);
                                unionCells.UnionWith(numberCells[n4 - 1]);

                                if (unionCells.Count != 4) continue;

                                List<int> quad = new List<int> { n1, n2, n3, n4 };

                                foreach (var (r, c) in unionCells)
                                {
                                    var candidates = allCellCandidates[r, c];
                                    int before = candidates.Count;
                                    candidates.RemoveAll(x => !quad.Contains(x));
                                    if (candidates.Count != before) changed = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }
}
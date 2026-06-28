using System.Collections.Generic;

public class NakedQuads
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIRLAR
        // ======================
        for (int row = 0; row < 9; row++)
        {
            List<int> emptyCols = new List<int>();
            for (int col = 0; col < 9; col++)
                if (!cells[row, col].IsFixed())
                    emptyCols.Add(col);

            for (int i = 0; i < emptyCols.Count - 3; i++)
            {
                for (int j = i + 1; j < emptyCols.Count - 2; j++)
                {
                    for (int k = j + 1; k < emptyCols.Count - 1; k++)
                    {
                        for (int l = k + 1; l < emptyCols.Count; l++)
                        {
                            int c1 = emptyCols[i];
                            int c2 = emptyCols[j];
                            int c3 = emptyCols[k];
                            int c4 = emptyCols[l];

                            if (allCellCandidates[row, c1].Count > 4) continue;
                            if (allCellCandidates[row, c2].Count > 4) continue;
                            if (allCellCandidates[row, c3].Count > 4) continue;
                            if (allCellCandidates[row, c4].Count > 4) continue;

                            HashSet<int> union = new HashSet<int>(allCellCandidates[row, c1]);
                            union.UnionWith(allCellCandidates[row, c2]);
                            union.UnionWith(allCellCandidates[row, c3]);
                            union.UnionWith(allCellCandidates[row, c4]);

                            if (union.Count != 4) continue;

                            for (int col = 0; col < 9; col++)
                            {
                                if (col == c1 || col == c2 || col == c3 || col == c4) continue;
                                if (cells[row, col].IsFixed()) continue;

                                int before = allCellCandidates[row, col].Count;
                                allCellCandidates[row, col].RemoveAll(x => union.Contains(x));
                                if (allCellCandidates[row, col].Count != before) changed = true;
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
            List<int> emptyRows = new List<int>();
            for (int row = 0; row < 9; row++)
                if (!cells[row, col].IsFixed())
                    emptyRows.Add(row);

            for (int i = 0; i < emptyRows.Count - 3; i++)
            {
                for (int j = i + 1; j < emptyRows.Count - 2; j++)
                {
                    for (int k = j + 1; k < emptyRows.Count - 1; k++)
                    {
                        for (int l = k + 1; l < emptyRows.Count; l++)
                        {
                            int r1 = emptyRows[i];
                            int r2 = emptyRows[j];
                            int r3 = emptyRows[k];
                            int r4 = emptyRows[l];

                            if (allCellCandidates[r1, col].Count > 4) continue;
                            if (allCellCandidates[r2, col].Count > 4) continue;
                            if (allCellCandidates[r3, col].Count > 4) continue;
                            if (allCellCandidates[r4, col].Count > 4) continue;

                            HashSet<int> union = new HashSet<int>(allCellCandidates[r1, col]);
                            union.UnionWith(allCellCandidates[r2, col]);
                            union.UnionWith(allCellCandidates[r3, col]);
                            union.UnionWith(allCellCandidates[r4, col]);

                            if (union.Count != 4) continue;

                            for (int row = 0; row < 9; row++)
                            {
                                if (row == r1 || row == r2 || row == r3 || row == r4) continue;
                                if (cells[row, col].IsFixed()) continue;

                                int before = allCellCandidates[row, col].Count;
                                allCellCandidates[row, col].RemoveAll(x => union.Contains(x));
                                if (allCellCandidates[row, col].Count != before) changed = true;
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

                List<(int r, int c)> emptyCells = new List<(int, int)>();
                for (int r = startRow; r < startRow + 3; r++)
                    for (int c = startCol; c < startCol + 3; c++)
                        if (!cells[r, c].IsFixed())
                            emptyCells.Add((r, c));

                for (int i = 0; i < emptyCells.Count - 3; i++)
                {
                    for (int j = i + 1; j < emptyCells.Count - 2; j++)
                    {
                        for (int k = j + 1; k < emptyCells.Count - 1; k++)
                        {
                            for (int l = k + 1; l < emptyCells.Count; l++)
                            {
                                var (r1, c1) = emptyCells[i];
                                var (r2, c2) = emptyCells[j];
                                var (r3, c3) = emptyCells[k];
                                var (r4, c4) = emptyCells[l];

                                if (allCellCandidates[r1, c1].Count > 4) continue;
                                if (allCellCandidates[r2, c2].Count > 4) continue;
                                if (allCellCandidates[r3, c3].Count > 4) continue;
                                if (allCellCandidates[r4, c4].Count > 4) continue;

                                HashSet<int> union = new HashSet<int>(allCellCandidates[r1, c1]);
                                union.UnionWith(allCellCandidates[r2, c2]);
                                union.UnionWith(allCellCandidates[r3, c3]);
                                union.UnionWith(allCellCandidates[r4, c4]);

                                if (union.Count != 4) continue;

                                for (int idx = 0; idx < emptyCells.Count; idx++)
                                {
                                    if (idx == i || idx == j || idx == k || idx == l) continue;

                                    var (r, c) = emptyCells[idx];
                                    int before = allCellCandidates[r, c].Count;
                                    allCellCandidates[r, c].RemoveAll(x => union.Contains(x));
                                    if (allCellCandidates[r, c].Count != before) changed = true;
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
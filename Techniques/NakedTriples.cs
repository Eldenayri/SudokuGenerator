using System.Collections.Generic;

public class NakedTriples
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIRLAR
        // ======================
        for (int row = 0; row < 9; row++)
        {
            // Boş hücreleri listele
            List<int> emptyCols = new List<int>();
            for (int col = 0; col < 9; col++)
                if (!cells[row, col].IsFixed())
                    emptyCols.Add(col);

            // 3'lü kombinasyonları dene
            for (int i = 0; i < emptyCols.Count - 2; i++)
            {
                for (int j = i + 1; j < emptyCols.Count - 1; j++)
                {
                    for (int k = j + 1; k < emptyCols.Count; k++)
                    {
                        int c1 = emptyCols[i];
                        int c2 = emptyCols[j];
                        int c3 = emptyCols[k];

                        // 3 hücrenin birleşik adayları
                        HashSet<int> union = new HashSet<int>(allCellCandidates[row, c1]);
                        union.UnionWith(allCellCandidates[row, c2]);
                        union.UnionWith(allCellCandidates[row, c3]);

                        if (union.Count != 3) continue;

                        // Naked Triple bulundu, diğer hücrelerden sil
                        for (int col = 0; col < 9; col++)
                        {
                            if (col == c1 || col == c2 || col == c3) continue;
                            if (cells[row, col].IsFixed()) continue;

                            int before = allCellCandidates[row, col].Count;
                            allCellCandidates[row, col].RemoveAll(x => union.Contains(x));
                            if (allCellCandidates[row, col].Count != before) changed = true;
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

            for (int i = 0; i < emptyRows.Count - 2; i++)
            {
                for (int j = i + 1; j < emptyRows.Count - 1; j++)
                {
                    for (int k = j + 1; k < emptyRows.Count; k++)
                    {
                        int r1 = emptyRows[i];
                        int r2 = emptyRows[j];
                        int r3 = emptyRows[k];

                        HashSet<int> union = new HashSet<int>(allCellCandidates[r1, col]);
                        union.UnionWith(allCellCandidates[r2, col]);
                        union.UnionWith(allCellCandidates[r3, col]);

                        if (union.Count != 3) continue;

                        for (int row = 0; row < 9; row++)
                        {
                            if (row == r1 || row == r2 || row == r3) continue;
                            if (cells[row, col].IsFixed()) continue;

                            int before = allCellCandidates[row, col].Count;
                            allCellCandidates[row, col].RemoveAll(x => union.Contains(x));
                            if (allCellCandidates[row, col].Count != before) changed = true;
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

                for (int i = 0; i < emptyCells.Count - 2; i++)
                {
                    for (int j = i + 1; j < emptyCells.Count - 1; j++)
                    {
                        for (int k = j + 1; k < emptyCells.Count; k++)
                        {
                            var (r1, c1) = emptyCells[i];
                            var (r2, c2) = emptyCells[j];
                            var (r3, c3) = emptyCells[k];

                            HashSet<int> union = new HashSet<int>(allCellCandidates[r1, c1]);
                            union.UnionWith(allCellCandidates[r2, c2]);
                            union.UnionWith(allCellCandidates[r3, c3]);

                            if (union.Count != 3) continue;

                            for (int idx = 0; idx < emptyCells.Count; idx++)
                            {
                                if (idx == i || idx == j || idx == k) continue;

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

        return changed;
    }

}


using System.Collections.Generic;

public class Swordfish
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIR BAZLI
        // 3 satırda aynı aday yalnızca aynı 3 sütunda geçiyorsa
        // o 3 sütunun diğer satırlarından sil
        // ======================
        for (int number = 1; number <= 9; number++)
        {
            List<List<int>> rowCols = new List<List<int>>();

            for (int row = 0; row < 9; row++)
            {
                List<int> cols = new List<int>();
                for (int col = 0; col < 9; col++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        cols.Add(col);
                rowCols.Add(cols);
            }

            // 2 veya 3 sütunda geçen satırları bul
            for (int r1 = 0; r1 < 9; r1++)
            {
                if (rowCols[r1].Count < 2 || rowCols[r1].Count > 3) continue;

                for (int r2 = r1 + 1; r2 < 9; r2++)
                {
                    if (rowCols[r2].Count < 2 || rowCols[r2].Count > 3) continue;

                    for (int r3 = r2 + 1; r3 < 9; r3++)
                    {
                        if (rowCols[r3].Count < 2 || rowCols[r3].Count > 3) continue;

                        // 3 satırın birleşik sütunları
                        HashSet<int> unionCols = new HashSet<int>(rowCols[r1]);
                        unionCols.UnionWith(rowCols[r2]);
                        unionCols.UnionWith(rowCols[r3]);

                        if (unionCols.Count != 3) continue;

                        // Swordfish bulundu
                        foreach (int col in unionCols)
                        {
                            for (int row = 0; row < 9; row++)
                            {
                                if (row == r1 || row == r2 || row == r3) continue;
                                if (cells[row, col].IsFixed()) continue;

                                int before = allCellCandidates[row, col].Count;
                                allCellCandidates[row, col].Remove(number);
                                if (allCellCandidates[row, col].Count != before) changed = true;
                            }
                        }
                    }
                }
            }
        }

        // ======================
        // SÜTUN BAZLI
        // ======================
        for (int number = 1; number <= 9; number++)
        {
            List<List<int>> colRows = new List<List<int>>();

            for (int col = 0; col < 9; col++)
            {
                List<int> rows = new List<int>();
                for (int row = 0; row < 9; row++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        rows.Add(row);
                colRows.Add(rows);
            }

            for (int c1 = 0; c1 < 9; c1++)
            {
                if (colRows[c1].Count < 2 || colRows[c1].Count > 3) continue;

                for (int c2 = c1 + 1; c2 < 9; c2++)
                {
                    if (colRows[c2].Count < 2 || colRows[c2].Count > 3) continue;

                    for (int c3 = c2 + 1; c3 < 9; c3++)
                    {
                        if (colRows[c3].Count < 2 || colRows[c3].Count > 3) continue;

                        HashSet<int> unionRows = new HashSet<int>(colRows[c1]);
                        unionRows.UnionWith(colRows[c2]);
                        unionRows.UnionWith(colRows[c3]);

                        if (unionRows.Count != 3) continue;

                        foreach (int row in unionRows)
                        {
                            for (int col = 0; col < 9; col++)
                            {
                                if (col == c1 || col == c2 || col == c3) continue;
                                if (cells[row, col].IsFixed()) continue;

                                int before = allCellCandidates[row, col].Count;
                                allCellCandidates[row, col].Remove(number);
                                if (allCellCandidates[row, col].Count != before) changed = true;
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }
}

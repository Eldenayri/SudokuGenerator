using System.Collections.Generic;

public class JellyFish
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // Jellyfish: X-Wing'in 4 satır/sütuna genişletilmiş hali
        // Bir rakam 4 farklı satırda yalnızca 2-4 sütunda bulunuyorsa,
        // o 4 sütunun diğer satırlarından bu rakam silinir.
        // Sonra aynı mantık sütun bazlı olarak da uygulanır.

        for (int digit = 1; digit <= 9; digit++)
        {
            // --- SATIR bazlı Jellyfish ---
            // Her satır için bu rakamın bulunduğu sütunları bul
            List<List<int>> rowCols = new List<List<int>>();
            for (int row = 0; row < 9; row++)
            {
                List<int> cols = new List<int>();
                for (int col = 0; col < 9; col++)
                {
                    if (!cells[row, col].IsFixed() &&
                        allCellCandidates[row, col].Contains(digit))
                        cols.Add(col);
                }
                rowCols.Add(cols);
            }

            // 4 satır seç (kombinasyon: 9'dan 4 seç)
            for (int r1 = 0; r1 < 9; r1++)
            {
                if (rowCols[r1].Count < 2 || rowCols[r1].Count > 4) continue;
                for (int r2 = r1 + 1; r2 < 9; r2++)
                {
                    if (rowCols[r2].Count < 2 || rowCols[r2].Count > 4) continue;
                    for (int r3 = r2 + 1; r3 < 9; r3++)
                    {
                        if (rowCols[r3].Count < 2 || rowCols[r3].Count > 4) continue;
                        for (int r4 = r3 + 1; r4 < 9; r4++)
                        {
                            if (rowCols[r4].Count < 2 || rowCols[r4].Count > 4) continue;

                            // 4 satırın birleşik sütun kümesi
                            HashSet<int> colUnion = new HashSet<int>(rowCols[r1]);
                            colUnion.UnionWith(rowCols[r2]);
                            colUnion.UnionWith(rowCols[r3]);
                            colUnion.UnionWith(rowCols[r4]);

                            // Birleşim tam 4 sütun içermeli (Jellyfish koşulu)
                            if (colUnion.Count != 4) continue;

                            // Bu 4 sütunun diğer satırlarından digit'i sil
                            int[] baseRows = { r1, r2, r3, r4 };
                            foreach (int col in colUnion)
                            {
                                for (int row = 0; row < 9; row++)
                                {
                                    // Jellyfish'i oluşturan 4 satırı atla
                                    if (row == r1 || row == r2 || row == r3 || row == r4) continue;
                                    if (cells[row, col].IsFixed()) continue;

                                    int before = allCellCandidates[row, col].Count;
                                    allCellCandidates[row, col].Remove(digit);
                                    if (allCellCandidates[row, col].Count != before)
                                        changed = true;
                                }
                            }
                        }
                    }
                }
            }

            // --- SÜTUN bazlı Jellyfish ---
            // Her sütun için bu rakamın bulunduğu satırları bul
            List<List<int>> colRows = new List<List<int>>();
            for (int col = 0; col < 9; col++)
            {
                List<int> rows = new List<int>();
                for (int row = 0; row < 9; row++)
                {
                    if (!cells[row, col].IsFixed() &&
                        allCellCandidates[row, col].Contains(digit))
                        rows.Add(row);
                }
                colRows.Add(rows);
            }

            // 4 sütun seç (kombinasyon: 9'dan 4 seç)
            for (int c1 = 0; c1 < 9; c1++)
            {
                if (colRows[c1].Count < 2 || colRows[c1].Count > 4) continue;
                for (int c2 = c1 + 1; c2 < 9; c2++)
                {
                    if (colRows[c2].Count < 2 || colRows[c2].Count > 4) continue;
                    for (int c3 = c2 + 1; c3 < 9; c3++)
                    {
                        if (colRows[c3].Count < 2 || colRows[c3].Count > 4) continue;
                        for (int c4 = c3 + 1; c4 < 9; c4++)
                        {
                            if (colRows[c4].Count < 2 || colRows[c4].Count > 4) continue;

                            // 4 sütunun birleşik satır kümesi
                            HashSet<int> rowUnion = new HashSet<int>(colRows[c1]);
                            rowUnion.UnionWith(colRows[c2]);
                            rowUnion.UnionWith(colRows[c3]);
                            rowUnion.UnionWith(colRows[c4]);

                            // Birleşim tam 4 satır içermeli (Jellyfish koşulu)
                            if (rowUnion.Count != 4) continue;

                            // Bu 4 satırın diğer sütunlarından digit'i sil
                            foreach (int row in rowUnion)
                            {
                                for (int col = 0; col < 9; col++)
                                {
                                    // Jellyfish'i oluşturan 4 sütunu atla
                                    if (col == c1 || col == c2 || col == c3 || col == c4) continue;
                                    if (cells[row, col].IsFixed()) continue;

                                    int before = allCellCandidates[row, col].Count;
                                    allCellCandidates[row, col].Remove(digit);
                                    if (allCellCandidates[row, col].Count != before)
                                        changed = true;
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
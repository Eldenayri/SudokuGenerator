using System.Collections.Generic;

public class XWing
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIR BAZLI
        // 2 satırda aynı aday yalnızca aynı 2 sütunda geçiyorsa
        // o 2 sütunun diğer satırlarından sil
        // ======================
        for (int number = 1; number <= 9; number++)
        {
            // Her satır için adayın geçtiği sütunları bul
            List<List<int>> rowCols = new List<List<int>>();

            for (int row = 0; row < 9; row++)
            {
                List<int> cols = new List<int>();
                for (int col = 0; col < 9; col++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        cols.Add(col);

                rowCols.Add(cols);
            }

            // Tam olarak 2 sütunda geçen satırları bul
            for (int r1 = 0; r1 < 9; r1++)
            {
                if (rowCols[r1].Count != 2) continue;

                for (int r2 = r1 + 1; r2 < 9; r2++)
                {
                    if (rowCols[r2].Count != 2) continue;

                    // Aynı 2 sütun mu?
                    if (rowCols[r1][0] != rowCols[r2][0]) continue;
                    if (rowCols[r1][1] != rowCols[r2][1]) continue;

                    // X-Wing bulundu
                    int colA = rowCols[r1][0];
                    int colB = rowCols[r1][1];

                    for (int row = 0; row < 9; row++)
                    {
                        if (row == r1 || row == r2) continue;
                        if (cells[row, colA].IsFixed()) continue;

                        int before = allCellCandidates[row, colA].Count;
                        allCellCandidates[row, colA].Remove(number);
                        if (allCellCandidates[row, colA].Count != before) changed = true;

                        if (cells[row, colB].IsFixed()) continue;

                        before = allCellCandidates[row, colB].Count;
                        allCellCandidates[row, colB].Remove(number);
                        if (allCellCandidates[row, colB].Count != before) changed = true;
                    }
                }
            }
        }

        // ======================
        // SÜTUN BAZLI
        // 2 sütunda aynı aday yalnızca aynı 2 satırda geçiyorsa
        // o 2 satırın diğer sütunlarından sil
        // ======================
        for (int number = 1; number <= 9; number++)
        {
            // Her sütun için adayın geçtiği satırları bul
            List<List<int>> colRows = new List<List<int>>();

            for (int col = 0; col < 9; col++)
            {
                List<int> rows = new List<int>();
                for (int row = 0; row < 9; row++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        rows.Add(row);

                colRows.Add(rows);
            }

            // Tam olarak 2 satırda geçen sütunları bul
            for (int c1 = 0; c1 < 9; c1++)
            {
                if (colRows[c1].Count != 2) continue;

                for (int c2 = c1 + 1; c2 < 9; c2++)
                {
                    if (colRows[c2].Count != 2) continue;

                    // Aynı 2 satır mı?
                    if (colRows[c1][0] != colRows[c2][0]) continue;
                    if (colRows[c1][1] != colRows[c2][1]) continue;

                    // X-Wing bulundu
                    int rowA = colRows[c1][0];
                    int rowB = colRows[c1][1];

                    for (int col = 0; col < 9; col++)
                    {
                        if (col == c1 || col == c2) continue;

                        if (!cells[rowA, col].IsFixed())
                        {
                            int before = allCellCandidates[rowA, col].Count;
                            allCellCandidates[rowA, col].Remove(number);
                            if (allCellCandidates[rowA, col].Count != before) changed = true;
                        }

                        if (!cells[rowB, col].IsFixed())
                        {
                            int before = allCellCandidates[rowB, col].Count;
                            allCellCandidates[rowB, col].Remove(number);
                            if (allCellCandidates[rowB, col].Count != before) changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }
}

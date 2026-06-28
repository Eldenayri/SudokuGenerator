using System.Collections.Generic;
using System.Linq;

public class BoxLineReduction
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // ======================
        // SATIRLAR
        // ======================
        for (int row = 0; row < 9; row++)
        {
            for (int number = 1; number <= 9; number++)
            {
                // Bu satırda number adayının geçtiği hücreleri bul
                List<int> foundCols = new List<int>();

                for (int col = 0; col < 9; col++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        foundCols.Add(col);

                if (foundCols.Count < 2) continue;

                // Hepsi aynı blokta mı?
                int blockCol = foundCols[0] / 3;
                bool sameBlock = foundCols.All(c => c / 3 == blockCol);

                if (sameBlock)
                {
                    int startRow = (row / 3) * 3;
                    int startCol = blockCol * 3;

                    // Aynı bloğun satır dışındaki hücrelerinden sil
                    for (int r = startRow; r < startRow + 3; r++)
                    {
                        if (r == row) continue; // mevcut satır, atla
                        for (int c = startCol; c < startCol + 3; c++)
                        {
                            if (cells[r, c].IsFixed()) continue;

                            int before = allCellCandidates[r, c].Count;
                            allCellCandidates[r, c].Remove(number);
                            if (allCellCandidates[r, c].Count != before) changed = true;
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
            for (int number = 1; number <= 9; number++)
            {
                // Bu sütunda number adayının geçtiği hücreleri bul
                List<int> foundRows = new List<int>();

                for (int row = 0; row < 9; row++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(number))
                        foundRows.Add(row);

                if (foundRows.Count < 2) continue;

                // Hepsi aynı blokta mı?
                int blockRow = foundRows[0] / 3;
                bool sameBlock = foundRows.All(r => r / 3 == blockRow);

                if (sameBlock)
                {
                    int startRow = blockRow * 3;
                    int startCol = (col / 3) * 3;

                    // Aynı bloğun sütun dışındaki hücrelerinden sil
                    for (int r = startRow; r < startRow + 3; r++)
                    {
                        for (int c = startCol; c < startCol + 3; c++)
                        {
                            if (c == col) continue; // mevcut sütun, atla
                            if (cells[r, c].IsFixed()) continue;

                            int before = allCellCandidates[r, c].Count;
                            allCellCandidates[r, c].Remove(number);
                            if (allCellCandidates[r, c].Count != before) changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }

}

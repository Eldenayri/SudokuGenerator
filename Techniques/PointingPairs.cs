using System.Collections.Generic;
using System.Linq;

public class PointingPairs
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        for (int br = 0; br < 3; br++)
        {
            for (int bc = 0; bc < 3; bc++)
            {
                int startRow = br * 3;
                int startCol = bc * 3;

                for (int number = 1; number <= 9; number++)
                {
                    // Bu blokta number adayının geçtiği hücreleri bul
                    List<(int r, int c)> found = new List<(int, int)>();

                    for (int r = startRow; r < startRow + 3; r++)
                        for (int c = startCol; c < startCol + 3; c++)
                            if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(number))
                                found.Add((r, c));

                    if (found.Count < 2) continue;

                    // ======================
                    // SATIR KONTROLÜ
                    // Hepsi aynı satırda mı?
                    // ======================
                    bool sameRow = found.All(x => x.r == found[0].r);
                    if (sameRow)
                    {
                        int row = found[0].r;

                        // Aynı satırın blok dışındaki hücrelerinden sil
                        for (int c = 0; c < 9; c++)
                        {
                            if (c >= startCol && c < startCol + 3) continue; // blok içi, atla
                            if (cells[row, c].IsFixed()) continue;

                            int before = allCellCandidates[row, c].Count;
                            allCellCandidates[row, c].Remove(number);
                            if (allCellCandidates[row, c].Count != before) changed = true;
                        }
                    }

                    // ======================
                    // SÜTUN KONTROLÜ
                    // Hepsi aynı sütunda mı?
                    // ======================
                    bool sameCol = found.All(x => x.c == found[0].c);
                    if (sameCol)
                    {
                        int col = found[0].c;

                        // Aynı sütunun blok dışındaki hücrelerinden sil
                        for (int r = 0; r < 9; r++)
                        {
                            if (r >= startRow && r < startRow + 3) continue; // blok içi, atla
                            if (cells[r, col].IsFixed()) continue;

                            int before = allCellCandidates[r, col].Count;
                            allCellCandidates[r, col].Remove(number);
                            if (allCellCandidates[r, col].Count != before) changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }
}

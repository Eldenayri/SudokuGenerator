using System.Collections.Generic;

public class Skyscraper
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        for (int number = 1; number <= 9; number++)
        {
            // Her satır için number adayının tam 2 sütunda geçtiği satırları topla
            List<(int row, int c1, int c2)> rows = new();

            for (int row = 0; row < 9; row++)
            {
                List<int> cols = new();
                for (int col = 0; col < 9; col++)
                {
                    if (cells[row, col].IsFixed()) continue;
                    if (allCellCandidates[row, col].Contains(number))
                        cols.Add(col);
                }
                if (cols.Count == 2)
                    rows.Add((row, cols[0], cols[1]));
            }

            // 2 satır kombinasyonu
            for (int i = 0; i < rows.Count; i++)
            {
                for (int j = i + 1; j < rows.Count; j++)
                {
                    int r1 = rows[i].row;
                    int r2 = rows[j].row;

                    int a1 = rows[i].c1;
                    int a2 = rows[i].c2;
                    int b1 = rows[j].c1;
                    int b2 = rows[j].c2;

                    // Ortak sütunu bul
                    int common = -1;
                    int r1Other = -1;
                    int r2Other = -1;

                    if (a1 == b1)      { common = a1; r1Other = a2; r2Other = b2; }
                    else if (a1 == b2) { common = a1; r1Other = a2; r2Other = b1; }
                    else if (a2 == b1) { common = a2; r1Other = a1; r2Other = b2; }
                    else if (a2 == b2) { common = a2; r1Other = a1; r2Other = b1; }

                    if (common == -1) continue;

                    // FIX: Ortak sütundaki iki hücre aynı blokta olmamalı.
                    // Aynı blokta olsalar bu Skyscraper değil, başka bir pattern'dir.
                    if (r1 / 3 == r2 / 3) continue;

                    // İki uç hücre (common sütun dışındaki hücreler)
                    int p1r = r1, p1c = r1Other;
                    int p2r = r2, p2c = r2Other;

                    // Her ikisini de gören hücrelerden number'ı sil
                    for (int r = 0; r < 9; r++)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            if (cells[r, c].IsFixed()) continue;
                            if (!allCellCandidates[r, c].Contains(number)) continue;

                            // Pattern hücrelerini atla
                            if ((r == r1 && c == r1Other) ||
                                (r == r2 && c == r2Other)) continue;

                            if (CanSee(r, c, p1r, p1c) && CanSee(r, c, p2r, p2c))
                            {
                                allCellCandidates[r, c].Remove(number);
                                changed = true;
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }

    private bool CanSee(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2) return true;
        if (c1 == c2) return true;
        return (r1 / 3 == r2 / 3) && (c1 / 3 == c2 / 3);
    }
}

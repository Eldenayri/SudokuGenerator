using System.Collections.Generic;
using System.Linq;

public class UniqueRectangle
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // Unique Rectangle mantığı:
        // 2 satır ve 2 sütunun kesişiminde 4 hücre oluşturuyor
        // Bu 4 hücrenin 3'ünde yalnızca aynı 2 aday varsa [A, B]
        // 4. hücrede A ve B dışında başka adaylar da varsa
        // 4. hücredeki A ve B silinir
        // (aksi halde çözüm tekil olmaz)

        for (int r1 = 0; r1 < 9; r1++)
        {
            for (int r2 = r1 + 1; r2 < 9; r2++)
            {
                for (int c1 = 0; c1 < 9; c1++)
                {
                    for (int c2 = c1 + 1; c2 < 9; c2++)
                    {
                        // 4 köşe hücre
                        var corners = new (int r, int c)[]
                        {
                            (r1, c1), (r1, c2),
                            (r2, c1), (r2, c2)
                        };

                        // Hepsi aynı blokta olmamalı
                        bool allSameBlock = (r1 / 3 == r2 / 3) && (c1 / 3 == c2 / 3);
                        if (allSameBlock) continue;

                        // Her köşenin adaylarını al
                        List<List<int>> candsList = new List<List<int>>();
                        foreach (var (r, c) in corners)
                            candsList.Add(allCellCandidates[r, c]);

                        // 3 köşe tam olarak [A, B] içeriyor mu?
                        List<int> pair = null;
                        int floorCount = 0;
                        int roofIdx = -1;

                        for (int i = 0; i < 4; i++)
                        {
                            // FIX: break → continue. Fixed hücreyle karşılaşınca
                            // döngünün geri kalanını da atlamak yanlış.
                            if (cells[corners[i].r, corners[i].c].IsFixed()) continue;

                            if (candsList[i].Count == 2)
                            {
                                if (pair == null)
                                {
                                    pair = candsList[i];
                                    floorCount++;
                                }
                                else if (candsList[i][0] == pair[0] && candsList[i][1] == pair[1])
                                {
                                    floorCount++;
                                }
                            }
                            else if (candsList[i].Count > 2)
                            {
                                roofIdx = i;
                            }
                        }

                        if (pair == null) continue;
                        if (floorCount != 3) continue;
                        if (roofIdx == -1) continue;

                        // Roof hücresinde pair dışında başka aday var mı?
                        var (roofR, roofC) = corners[roofIdx];
                        List<int> roofCands = allCellCandidates[roofR, roofC];

                        bool hasExtra = roofCands.Any(x => !pair.Contains(x));
                        if (!hasExtra) continue;

                        // Roof hücresinden pair adaylarını sil
                        int before = roofCands.Count;
                        roofCands.RemoveAll(x => pair.Contains(x));
                        if (roofCands.Count != before) changed = true;
                    }
                }
            }
        }

        return changed;
    }
}

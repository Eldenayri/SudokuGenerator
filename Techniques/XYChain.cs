using System.Collections.Generic;

public class XYChain
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        // 1. Sadece 2 adayı olan (bi-value) hücrelerin listesini çıkar
        List<(int r, int c, int v1, int v2)> bivalueCells = new();

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (!cells[r, c].IsFixed())
                {
                    var cand = allCellCandidates[r, c];
                    // FIX: null kontrolü kaldırıldı — BasicCalculate hiçbir zaman null atamaz.
                    if (cand.Count == 2)
                        bivalueCells.Add((r, c, cand[0], cand[1]));
                }
            }
        }

        // 2. Her bir bivalue hücreden başlayarak olası zincirleri dene (DFS)
        foreach (var start in bivalueCells)
        {
            // FIX: List yerine HashSet — visited.Contains O(1) olur, uzun zincirlerde kritik
            var visitedSet = new HashSet<(int, int)> { (start.r, start.c) };
            var visitedList = new List<(int, int)> { (start.r, start.c) };

            if (FindChain(start, targetValue: start.v1, linkValue: start.v2,
                          bivalueCells, allCellCandidates, cells, visitedSet, visitedList))
                return true;

            visitedSet.Clear();
            visitedSet.Add((start.r, start.c));
            visitedList.Clear();
            visitedList.Add((start.r, start.c));

            if (FindChain(start, targetValue: start.v2, linkValue: start.v1,
                          bivalueCells, allCellCandidates, cells, visitedSet, visitedList))
                return true;
        }

        return false;
    }

    private bool FindChain(
        (int r, int c, int v1, int v2) current,
        int targetValue,
        int linkValue,
        List<(int r, int c, int v1, int v2)> allBivalue,
        List<int>[,] a,
        SudokuCell[,] cells,
        HashSet<(int, int)> visitedSet,   // FIX: O(1) Contains için HashSet
        List<(int, int)> visitedList)     // Sıralı path (başlangıç için visitedList[0] lazım)
    {
        foreach (var next in allBivalue)
        {
            // FIX: HashSet ile O(1) kontrol
            if (visitedSet.Contains((next.r, next.c))) continue;

            if (!CanSee(current.r, current.c, next.r, next.c)) continue;
            if (next.v1 != linkValue && next.v2 != linkValue) continue;

            int nextLink = (next.v1 == linkValue) ? next.v2 : next.v1;

            if (nextLink == targetValue && visitedList.Count >= 2)
            {
                var startCell = visitedList[0];
                if (ApplyElimination(startCell, next, targetValue, a, cells))
                    return true;
            }

            visitedSet.Add((next.r, next.c));
            visitedList.Add((next.r, next.c));

            if (FindChain(next, targetValue, nextLink, allBivalue, a, cells, visitedSet, visitedList))
                return true;

            visitedSet.Remove((next.r, next.c));
            visitedList.RemoveAt(visitedList.Count - 1);
        }

        return false;
    }

    private bool ApplyElimination(
        (int r, int c) start,
        (int r, int c, int v1, int v2) end,
        int targetCand,
        List<int>[,] a,
        SudokuCell[,] cells)
    {
        bool changed = false;

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (cells[r, c].IsFixed()) continue;
                if ((r == start.r && c == start.c) || (r == end.r && c == end.c)) continue;

                var cand = a[r, c];
                if (!cand.Contains(targetCand)) continue;

                if (CanSee(r, c, start.r, start.c) && CanSee(r, c, end.r, end.c))
                {
                    int before = cand.Count;
                    cand.Remove(targetCand);
                    if (cand.Count != before) changed = true;
                }
            }
        }

        return changed;
    }

    private bool CanSee(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2 && c1 == c2) return false;
        if (r1 == r2) return true;
        if (c1 == c2) return true;
        return (r1 / 3 == r2 / 3) && (c1 / 3 == c2 / 3);
    }
}

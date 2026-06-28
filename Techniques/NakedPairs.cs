using System.Collections.Generic;

public class NakedPairs
{
    List<int>[,] allCellCandidates;
    SudokuCell[,] cells;

    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        this.allCellCandidates = allCellCandidates;
        this.cells = cells;

        bool changed = false;

        // =========================
        // SATIRLAR
        // =========================
        for (int row = 0; row < 9; row++)
        {
            for (int c1 = 0; c1 < 9; c1++)
            {
                if (cells[row, c1].IsFixed()) continue;

                List<int> cand1 = allCellCandidates[row, c1];
                if (cand1.Count != 2) continue;

                for (int c2 = c1 + 1; c2 < 9; c2++)
                {
                    if (cells[row, c2].IsFixed()) continue;

                    List<int> cand2 = allCellCandidates[row, c2];

                    if (IsSamePair(cand1, cand2))
                    { 
                        // Naked Pair bulundu

                        for (int col = 0; col < 9; col++)
                        {
                            if (col == c1 || col == c2) continue;
                            if (cells[row, col].IsFixed()) continue;

                            if (RemoveCandidates(row, col, cand1)) changed = true;
                        }
                    }
                }
            }
        }

        // =========================
        // SÜTUNLAR
        // =========================
        for (int col = 0; col < 9; col++)
        {
            for (int r1 = 0; r1 < 9; r1++)
            {
                if (cells[r1, col].IsFixed()) continue;

                List<int> cand1 = allCellCandidates[r1, col];
                if (cand1.Count != 2) continue;

                for (int r2 = r1 + 1; r2 < 9; r2++)
                {
                    if (cells[r2, col].IsFixed()) continue;

                    List<int> cand2 = allCellCandidates[r2, col];

                    if (IsSamePair(cand1, cand2))
                    {
                        for (int row = 0; row < 9; row++)
                        {
                            if (row == r1 || row == r2) continue;
                            if (cells[row, col].IsFixed()) continue;

                            if (RemoveCandidates(row, col, cand1)) changed = true; 
                        }
                    }
                }
            }
        }

        // =========================
        // BLOKLAR
        // =========================
        for (int br = 0; br < 3; br++)
        {
            for (int bc = 0; bc < 3; bc++)
            {
                int startRow = br * 3;
                int startCol = bc * 3;

                var blockCells = new List<(int r, int c)>();
                for (int r = startRow; r < startRow + 3; r++)
                    for (int c = startCol; c < startCol + 3; c++)
                        if (!cells[r, c].IsFixed())
                            blockCells.Add((r, c));

                for (int i = 0; i < blockCells.Count; i++)
                {
                    var (r1, c1) = blockCells[i];
                    List<int> cand1 = allCellCandidates[r1, c1];
                    if (cand1.Count != 2) continue;

                    for (int j = i + 1; j < blockCells.Count; j++)
                    {
                        var (r2, c2) = blockCells[j];
                        List<int> cand2 = allCellCandidates[r2, c2];

                        if (IsSamePair(cand1, cand2))
                        {
                            foreach (var (r, c) in blockCells)
                            {
                                if ((r == r1 && c == c1) || (r == r2 && c == c2)) continue;
                                if (RemoveCandidates(r, c, cand1)) changed = true;
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }

    private bool IsSamePair(List<int> a, List<int> b)
    {
        return a.Count == 2 &&
               b.Count == 2 &&
               a.Contains(b[0]) &&
               a.Contains(b[1]);
    }

    private bool RemoveCandidates(int row, int col, List<int> pair)
    {
        var candidates = allCellCandidates[row, col];
        int before = candidates.Count;

        candidates.RemoveAll(x => pair.Contains(x));

        return candidates.Count != before;
    }
}
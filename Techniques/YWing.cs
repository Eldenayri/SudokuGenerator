using System.Collections.Generic;

public class YWing
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        for (int pivotRow = 0; pivotRow < 9; pivotRow++)
        {
            for (int pivotCol = 0; pivotCol < 9; pivotCol++)
            {
                if (cells[pivotRow, pivotCol].IsFixed())
                    continue;

                List<int> pivotCands = allCellCandidates[pivotRow, pivotCol];

                if (pivotCands == null || pivotCands.Count != 2)
                    continue;

                int A = pivotCands[0];
                int B = pivotCands[1];

                List<(int r, int c)> pivotPeers = GetPeers(pivotRow, pivotCol);

                // Pincer1 = [A,C]
                foreach (var (p1r, p1c) in pivotPeers)
                {
                    if (cells[p1r, p1c].IsFixed())
                        continue;

                    List<int> cands1 = allCellCandidates[p1r, p1c];

                    if (cands1 == null || cands1.Count != 2)
                        continue;

                    if (!cands1.Contains(A))
                        continue;

                    // [A,B] istemiyoruz
                    if (cands1.Contains(B))
                        continue;

                    int C = (cands1[0] == A) ? cands1[1] : cands1[0];

                    // Pincer2 = [B,C]
                    foreach (var (p2r, p2c) in pivotPeers)
                    {
                        if (p2r == p1r && p2c == p1c)
                            continue;

                        if (cells[p2r, p2c].IsFixed())
                            continue;

                        List<int> cands2 = allCellCandidates[p2r, p2c];

                        if (cands2 == null || cands2.Count != 2)
                            continue;

                        if (!cands2.Contains(B))
                            continue;

                        if (!cands2.Contains(C))
                            continue;

                        // [A,B] istemiyoruz
                        if (cands2.Contains(A))
                            continue;

                        // Ortak gören hücreleri bul
                        List<(int r, int c)> peers1 = GetPeers(p1r, p1c);
                        HashSet<(int r, int c)> peers2 =
                            new HashSet<(int r, int c)>(GetPeers(p2r, p2c));

                        foreach (var (r, c) in peers1)
                        {
                            if (!peers2.Contains((r, c)))
                                continue;

                            if ((r == pivotRow && c == pivotCol) ||
                                (r == p1r && c == p1c) ||
                                (r == p2r && c == p2c))
                                continue;

                            if (cells[r, c].IsFixed())
                                continue;

                            List<int> target = allCellCandidates[r, c];

                            if (target == null)
                                continue;

                            if (!target.Contains(C))
                                continue;

                            target.Remove(C);
                            changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }

    private List<(int r, int c)> GetPeers(int row, int col)
    {
        HashSet<(int r, int c)> peers = new HashSet<(int r, int c)>();

        for (int c = 0; c < 9; c++)
        {
            if (c != col)
                peers.Add((row, c));
        }

        for (int r = 0; r < 9; r++)
        {
            if (r != row)
                peers.Add((r, col));
        }

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;

        for (int r = startRow; r < startRow + 3; r++)
        {
            for (int c = startCol; c < startCol + 3; c++)
            {
                if (r != row || c != col)
                    peers.Add((r, c));
            }
        }

        return new List<(int r, int c)>(peers);
    }
}
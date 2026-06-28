using System.Collections.Generic;

public class XYZWing
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // XYZ-Wing mantığı:
        // Pivot hücre: tam 3 adayı var [X, Y, Z]
        // Pincer1: pivot ile peer, tam 2 adayı var [X, Z]
        // Pincer2: pivot ile peer, tam 2 adayı var [Y, Z]
        // Pivot, Pincer1 ve Pincer2'yi aynı anda gören hücrelerden Z silinir

        for (int pivotRow = 0; pivotRow < 9; pivotRow++)
        {
            for (int pivotCol = 0; pivotCol < 9; pivotCol++)
            {
                if (cells[pivotRow, pivotCol].IsFixed()) continue;

                List<int> pivotCands = allCellCandidates[pivotRow, pivotCol];
                if (pivotCands.Count != 3) continue;

                int X = pivotCands[0];
                int Y = pivotCands[1];
                int Z = pivotCands[2];

                List<(int r, int c)> pivotPeers = GetPeers(pivotRow, pivotCol);

                // Pincer1: [X, Z]
                foreach (var (p1r, p1c) in pivotPeers)
                {
                    if (cells[p1r, p1c].IsFixed()) continue;
                    List<int> cands1 = allCellCandidates[p1r, p1c];
                    if (cands1.Count != 2) continue;
                    if (!cands1.Contains(X) || !cands1.Contains(Z)) continue;

                    // Pincer2: [Y, Z]
                    foreach (var (p2r, p2c) in pivotPeers)
                    {
                        if (p2r == p1r && p2c == p1c) continue;
                        if (cells[p2r, p2c].IsFixed()) continue;
                        List<int> cands2 = allCellCandidates[p2r, p2c];
                        if (cands2.Count != 2) continue;
                        if (!cands2.Contains(Y) || !cands2.Contains(Z)) continue;

                        // XYZ-Wing bulundu
                        // Pivot, Pincer1 ve Pincer2'yi aynı anda gören hücrelerden Z'yi sil
                        List<(int r, int c)> peers1 = GetPeers(p1r, p1c);
                        List<(int r, int c)> peers2 = GetPeers(p2r, p2c);

                        foreach (var (r, c) in pivotPeers)
                        {
                            if (r == pivotRow && c == pivotCol) continue;
                            if (r == p1r && c == p1c) continue;
                            if (r == p2r && c == p2c) continue;
                            if (cells[r, c].IsFixed()) continue;
                            if (!peers1.Contains((r, c))) continue;
                            if (!peers2.Contains((r, c))) continue;

                            int before = allCellCandidates[r, c].Count;
                            allCellCandidates[r, c].Remove(Z);
                            if (allCellCandidates[r, c].Count != before) changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }



    // Bir hücrenin gördüğü tüm hücreleri döndürür (aynı satır, sütun, blok)
    private List<(int r, int c)> GetPeers(int row, int col)
    {
        HashSet<(int, int)> peers = new HashSet<(int, int)>();

        for (int c = 0; c < 9; c++)
            if (c != col) peers.Add((row, c));

        for (int r = 0; r < 9; r++)
            if (r != row) peers.Add((r, col));

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (r != row || c != col) peers.Add((r, c));

        return new List<(int, int)>(peers);
    }

}

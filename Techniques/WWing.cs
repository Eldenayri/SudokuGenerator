using System.Collections.Generic;

public class WWing
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        // Tüm bivalue hücreleri bul
        List<(int r, int c)> bivalueCells = new List<(int, int)>();
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Count == 2)
                    bivalueCells.Add((r, c));

        // A için tüm conjugate pair'leri bul (bir birimde A tam 2 hücrede)
        // Bridge: (b1, b2) çifti — aynı satır/sütun/blokta A tam 2 kez geçiyor
        List<(int b1r, int b1c, int b2r, int b2c, int bridgeDigit)> bridges = GetBridges(allCellCandidates, cells);

        // Her bivalue çifti dene (Pincer1 ve Pincer2)
        for (int i = 0; i < bivalueCells.Count - 1; i++)
        {
            for (int j = i + 1; j < bivalueCells.Count; j++)
            {
                var (p1r, p1c) = bivalueCells[i];
                var (p2r, p2c) = bivalueCells[j];

                List<int> cands1 = allCellCandidates[p1r, p1c];
                List<int> cands2 = allCellCandidates[p2r, p2c];

                if (cands1.Count != 2 || cands2.Count != 2) continue;

                // İkisi de aynı [A, B] adaylarına sahip olmalı
                if (!cands1.Contains(cands2[0]) || !cands1.Contains(cands2[1])) continue;

                int A = cands1[0];
                int B = cands1[1];

                // Pincer1 ve Pincer2 birbirini doğrudan görüyorsa W-Wing değil
                if (Sees(p1r, p1c, p2r, p2c)) continue;

                // A ve B için her iki yönde dene
                for (int pass = 0; pass < 2; pass++)
                {
                    int bridgeDigit = (pass == 0) ? A : B;
                    int elimDigit = (pass == 0) ? B : A;

                    // Bu A (veya B) için uygun bir köprü var mı?
                    foreach (var (b1r, b1c, b2r, b2c, bd) in bridges)
                    {
                        if (bd != bridgeDigit) continue;

                        // Köprü hücreleri pincer olmamalı
                        if (b1r == p1r && b1c == p1c) continue;
                        if (b1r == p2r && b1c == p2c) continue;
                        if (b2r == p1r && b2c == p1c) continue;
                        if (b2r == p2r && b2c == p2c) continue;

                        // Köprünün bir ucu Pincer1'i, diğer ucu Pincer2'yi görmeli
                        bool case1 = Sees(b1r, b1c, p1r, p1c) && Sees(b2r, b2c, p2r, p2c);
                        bool case2 = Sees(b1r, b1c, p2r, p2c) && Sees(b2r, b2c, p1r, p1c);

                        if (!case1 && !case2) continue;

                        // W-Wing bulundu — her iki pinceri de gören hücrelerden elimDigit'i sil
                        HashSet<(int, int)> peers1 = new HashSet<(int, int)>(GetPeers(p1r, p1c));
                        HashSet<(int, int)> peers2 = new HashSet<(int, int)>(GetPeers(p2r, p2c));

                        foreach (var (r, c) in peers1)
                        {
                            if (!peers2.Contains((r, c))) continue;
                            if (r == p1r && c == p1c) continue;
                            if (r == p2r && c == p2c) continue;
                            if (r == b1r && c == b1c) continue;
                            if (r == b2r && c == b2c) continue;
                            if (cells[r, c].IsFixed()) continue;
                            if (!allCellCandidates[r, c].Contains(elimDigit)) continue;

                            allCellCandidates[r, c].Remove(elimDigit);
                            changed = true;
                        }
                    }
                }
            }
        }

        return changed;
    }

    // Tüm birimlerde (satır/sütun/blok) her rakam için conjugate pair'leri döndür
    // Conjugate pair: bir birimde rakam tam 2 hücrede bulunuyorsa
    private List<(int b1r, int b1c, int b2r, int b2c, int digit)> GetBridges(
        List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        var bridges = new List<(int, int, int, int, int)>();

        for (int digit = 1; digit <= 9; digit++)
        {
            // Satır bazlı
            for (int row = 0; row < 9; row++)
            {
                var cols = new List<int>();
                for (int col = 0; col < 9; col++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                        cols.Add(col);
                if (cols.Count == 2)
                    bridges.Add((row, cols[0], row, cols[1], digit));
            }

            // Sütun bazlı
            for (int col = 0; col < 9; col++)
            {
                var rows = new List<int>();
                for (int row = 0; row < 9; row++)
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                        rows.Add(row);
                if (rows.Count == 2)
                    bridges.Add((rows[0], col, rows[1], col, digit));
            }

            // Blok bazlı
            for (int boxRow = 0; boxRow < 3; boxRow++)
            {
                for (int boxCol = 0; boxCol < 3; boxCol++)
                {
                    var positions = new List<(int, int)>();
                    for (int r = boxRow * 3; r < boxRow * 3 + 3; r++)
                        for (int c = boxCol * 3; c < boxCol * 3 + 3; c++)
                            if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                                positions.Add((r, c));
                    if (positions.Count == 2)
                        bridges.Add((positions[0].Item1, positions[0].Item2,
                                     positions[1].Item1, positions[1].Item2, digit));
                }
            }
        }

        return bridges;
    }

    private bool Sees(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2) return true;
        if (c1 == c2) return true;
        if ((r1 / 3) == (r2 / 3) && (c1 / 3) == (c2 / 3)) return true;
        return false;
    }

    private List<(int r, int c)> GetPeers(int row, int col)
    {
        HashSet<(int r, int c)> peers = new HashSet<(int r, int c)>();

        for (int c = 0; c < 9; c++)
            if (c != col) peers.Add((row, c));

        for (int r = 0; r < 9; r++)
            if (r != row) peers.Add((r, col));

        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                if (r != row || c != col) peers.Add((r, c));

        return new List<(int r, int c)>(peers);
    }
}
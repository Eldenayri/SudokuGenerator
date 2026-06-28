using System.Collections.Generic;
using System.Linq;

public class SimpleColouring
{
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        bool changed = false;

        for (int digit = 1; digit <= 9; digit++)
        {
            // Bu rakam için tüm conjugate pair'leri bul
            // Conjugate pair: bir birimde (satır/sütun/blok) rakam tam 2 hücrede bulunuyorsa
            // bu iki hücre birbirinin conjugate'idir.
            List<(int r1, int c1, int r2, int c2)> conjugatePairs = GetConjugatePairs(digit, allCellCandidates, cells);

            // Conjugate pair'lerden bağlantı grafiği oluştur
            // Her hücre için komşularını tut
            Dictionary<(int, int), List<(int, int)>> graph = new Dictionary<(int, int), List<(int, int)>>();

            foreach (var (r1, c1, r2, c2) in conjugatePairs)
            {
                if (!graph.ContainsKey((r1, c1))) graph[(r1, c1)] = new List<(int, int)>();
                if (!graph.ContainsKey((r2, c2))) graph[(r2, c2)] = new List<(int, int)>();
                graph[(r1, c1)].Add((r2, c2));
                graph[(r2, c2)].Add((r1, c1));
            }

            // Her bağlantılı bileşeni BFS ile boya (0 = renksiz, 1 = renk A, 2 = renk B)
            Dictionary<(int, int), int> colour = new Dictionary<(int, int), int>();
            foreach (var node in graph.Keys) colour[node] = 0;

            foreach (var startNode in graph.Keys)
            {
                if (colour[startNode] != 0) continue; // Zaten boyanmış

                // BFS ile bu bileşeni boya
                Queue<(int, int)> queue = new Queue<(int, int)>();
                queue.Enqueue(startNode);
                colour[startNode] = 1; // Renk A

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    int nextColour = (colour[current] == 1) ? 2 : 1;

                    foreach (var neighbour in graph[current])
                    {
                        if (colour[neighbour] == 0)
                        {
                            colour[neighbour] = nextColour;
                            queue.Enqueue(neighbour);
                        }
                    }
                }

                // Bu bileşendeki renk 1 ve renk 2 hücrelerini topla
                List<(int, int)> colourA = new List<(int, int)>();
                List<(int, int)> colourB = new List<(int, int)>();

                foreach (var node in colour.Keys)
                {
                    if (colour[node] == 1) colourA.Add(node);
                    else if (colour[node] == 2) colourB.Add(node);
                }

                // --- Kural 2: Aynı renkteki iki hücre aynı birimi görüyorsa ---
                // O renk yanlış → karşı renk tüm hücrelerinde kesinleşir (digit sabit)
                // ve bu bileşendeki o renk adaylardan silinir.

                bool colourAInvalid = SameColourSeesEachOther(colourA, digit, allCellCandidates, cells);
                bool colourBInvalid = SameColourSeesEachOther(colourB, digit, allCellCandidates, cells);

                if (colourAInvalid || colourBInvalid)
                {
                    // Yanlış rengi belirle
                    List<(int, int)> wrongColour = colourAInvalid ? colourA : colourB;

                    foreach (var (wr, wc) in wrongColour)
                    {
                        if (cells[wr, wc].IsFixed()) continue;
                        int before = allCellCandidates[wr, wc].Count;
                        allCellCandidates[wr, wc].Remove(digit);
                        if (allCellCandidates[wr, wc].Count != before) changed = true;
                    }

                    // Renk belirlendikten sonra bu bileşen için Kural 4'e gerek yok
                    continue;
                }

                // --- Kural 4: Renksiz bir hücre her iki rengi de görüyorsa ---
                // O hücreden digit silinir.

                for (int r = 0; r < 9; r++)
                {
                    for (int c = 0; c < 9; c++)
                    {
                        if (cells[r, c].IsFixed()) continue;
                        if (!allCellCandidates[r, c].Contains(digit)) continue;
                        if (colour.ContainsKey((r, c))) continue; // Zaten zincirde

                        bool seesA = SeesAnyInList(r, c, colourA);
                        bool seesB = SeesAnyInList(r, c, colourB);

                        if (seesA && seesB)
                        {
                            int before = allCellCandidates[r, c].Count;
                            allCellCandidates[r, c].Remove(digit);
                            if (allCellCandidates[r, c].Count != before) changed = true;
                        }
                    }
                }

                // Bir sonraki bileşen için renkleri sıfırla
                foreach (var node in colour.Keys.ToList()) colour[node] = 0;
            }
        }

        return changed;
    }

    // Bir birimde (satır/sütun/blok) rakam tam 2 hücredeyse conjugate pair döndür
    private List<(int r1, int c1, int r2, int c2)> GetConjugatePairs(
        int digit, List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        // HashSet ile tekrar eden çiftleri önle
        HashSet<(int, int, int, int)> seen = new HashSet<(int, int, int, int)>();
        List<(int, int, int, int)> pairs = new List<(int, int, int, int)>();

        // Satır bazlı conjugate pair
        for (int row = 0; row < 9; row++)
        {
            List<int> cols = new List<int>();
            for (int col = 0; col < 9; col++)
                if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                    cols.Add(col);

            if (cols.Count == 2)
                AddPair(pairs, seen, row, cols[0], row, cols[1]);
        }

        // Sütun bazlı conjugate pair
        for (int col = 0; col < 9; col++)
        {
            List<int> rows = new List<int>();
            for (int row = 0; row < 9; row++)
                if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(digit))
                    rows.Add(row);

            if (rows.Count == 2)
                AddPair(pairs, seen, rows[0], col, rows[1], col);
        }

        // Blok bazlı conjugate pair
        for (int boxRow = 0; boxRow < 3; boxRow++)
        {
            for (int boxCol = 0; boxCol < 3; boxCol++)
            {
                List<(int, int)> positions = new List<(int, int)>();
                for (int r = boxRow * 3; r < boxRow * 3 + 3; r++)
                    for (int c = boxCol * 3; c < boxCol * 3 + 3; c++)
                        if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(digit))
                            positions.Add((r, c));

                if (positions.Count == 2)
                    AddPair(pairs, seen, positions[0].Item1, positions[0].Item2,
                                        positions[1].Item1, positions[1].Item2);
            }
        }

        return pairs;
    }

    private void AddPair(List<(int, int, int, int)> pairs, HashSet<(int, int, int, int)> seen,
        int r1, int c1, int r2, int c2)
    {
        // Küçük olanı önce koy (duplicate önleme)
        var key = (r1 < r2 || (r1 == r2 && c1 < c2))
            ? (r1, c1, r2, c2)
            : (r2, c2, r1, c1);

        if (seen.Add(key)) pairs.Add(key);
    }

    // Aynı renkteki iki hücre aynı birimi görüyor mu?
    private bool SameColourSeesEachOther(List<(int, int)> colourGroup,
        int digit, List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        for (int i = 0; i < colourGroup.Count; i++)
        {
            for (int j = i + 1; j < colourGroup.Count; j++)
            {
                var (r1, c1) = colourGroup[i];
                var (r2, c2) = colourGroup[j];
                if (SeesEachOther(r1, c1, r2, c2)) return true;
            }
        }
        return false;
    }

    // Verilen hücre, listedeki herhangi bir hücreyi görüyor mu?
    private bool SeesAnyInList(int row, int col, List<(int, int)> group)
    {
        foreach (var (r, c) in group)
            if (SeesEachOther(row, col, r, c)) return true;
        return false;
    }

    // İki hücre birbirini görüyor mu? (aynı satır, sütun veya blok)
    private bool SeesEachOther(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2) return true;
        if (c1 == c2) return true;
        if ((r1 / 3 == r2 / 3) && (c1 / 3 == c2 / 3)) return true;
        return false;
    }
}
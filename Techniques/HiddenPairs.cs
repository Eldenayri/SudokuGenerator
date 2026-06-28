using System.Collections.Generic;

public class HiddenPairs
{
    
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {

        bool changed = false;

        // ======================
        // SATIRLAR
        // ======================
        for (int row = 0; row < 9; row++)
        {
            for (int n1 = 1; n1 <= 9; n1++)
            {
                // n1'in bu satırda geçtiği sütunları bul
                List<int> cols1 = new List<int>();
                for (int col = 0; col < 9; col++)
                {
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(n1))
                        cols1.Add(col);
                }

                if (cols1.Count != 2) continue;

                for (int n2 = n1 + 1; n2 <= 9; n2++)
                {
                    // n2'nin bu satırda geçtiği sütunları bul
                    List<int> cols2 = new List<int>();
                    for (int col = 0; col < 9; col++)
                    {
                        if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(n2))
                            cols2.Add(col);
                    }

                    // n1 ve n2 aynı 2 hücrede mi geçiyor?
                    if (cols2.Count == 2 && cols1[0] == cols2[0] && cols1[1] == cols2[1])
                    {
                        // Hidden Pair bulundu, bu 2 hücreden n1 ve n2 dışındaki adayları sil
                        int ca = cols1[0], cb = cols1[1];
                        List<int> pair = new List<int> { n1, n2 };

                        foreach (int col in new[] { ca, cb })
                        {
                            List<int> candidates = allCellCandidates[row, col];
                            int before = candidates.Count;
                            candidates.RemoveAll(x => !pair.Contains(x));
                            if (candidates.Count != before) changed = true;
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
            for (int n1 = 1; n1 <= 9; n1++)
            {
                List<int> rows1 = new List<int>();
                for (int row = 0; row < 9; row++)
                {
                    if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(n1))
                        rows1.Add(row);
                }

                if (rows1.Count != 2) continue;

                for (int n2 = n1 + 1; n2 <= 9; n2++)
                {
                    List<int> rows2 = new List<int>();
                    for (int row = 0; row < 9; row++)
                    {
                        if (!cells[row, col].IsFixed() && allCellCandidates[row, col].Contains(n2))
                            rows2.Add(row);
                    }

                    if (rows2.Count == 2 && rows1[0] == rows2[0] && rows1[1] == rows2[1])
                    {
                        int ra = rows1[0], rb = rows1[1];
                        List<int> pair = new List<int> { n1, n2 };

                        foreach (int row in new[] { ra, rb })
                        {
                            var candidates = allCellCandidates[row, col];
                            int before = candidates.Count;
                            candidates.RemoveAll(x => !pair.Contains(x));
                            if (candidates.Count != before) changed = true;
                        }
                    }
                }
            }
        }

        // ======================
        // BLOKLAR
        // ======================
        for (int br = 0; br < 3; br++)
        {
            for (int bc = 0; bc < 3; bc++)
            {
                int startRow = br * 3;
                int startCol = bc * 3;

                for (int n1 = 1; n1 <= 9; n1++)
                {
                    // n1'in bu blokta geçtiği hücreleri bul
                    List<(int r, int c)> cells1 = new List<(int, int)>();
                    for (int r = startRow; r < startRow + 3; r++)
                        for (int c = startCol; c < startCol + 3; c++)
                            if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(n1))
                                cells1.Add((r, c));

                    if (cells1.Count != 2) continue;

                    for (int n2 = n1 + 1; n2 <= 9; n2++)
                    {
                        List<(int r, int c)> cells2 = new List<(int, int)>();
                        for (int r = startRow; r < startRow + 3; r++)
                            for (int c = startCol; c < startCol + 3; c++)
                                if (!cells[r, c].IsFixed() && allCellCandidates[r, c].Contains(n2))
                                    cells2.Add((r, c));

                        if (cells2.Count == 2 &&
                            cells1[0] == cells2[0] &&
                            cells1[1] == cells2[1])
                        {
                            List<int> pair = new List<int> { n1, n2 };

                            foreach (var (r, c) in cells1)
                            {
                                var candidates = allCellCandidates[r, c];
                                int before = candidates.Count;
                                candidates.RemoveAll(x => !pair.Contains(x));
                                if (candidates.Count != before) changed = true;
                            }
                        }
                    }
                }
            }
        }

        return changed;
    }
}

using System.Collections.Generic;

public class BUG
{
    // Dönüş: true ise bir hücreye kesin değer atandı → döngüde "break" ile kullanılır
    public bool Fill(List<int>[,] allCellCandidates, SudokuCell[,] cells)
    {
        // BUG koşulu:
        // 1. Tüm çözülmemiş hücrelerin tam 2 adayı olmalı,
        //    YALNIZCA bir hücre 3 adaya sahip olabilir (BUG+1 hücresi).
        // 2. O hücredeki 3 adaydan hangisi kendi satır, sütun ve bloğunda
        //    tam 3 kez görünüyorsa (BUG+1 hücresi dahil), o rakam bu hücrenin çözümüdür.

        (int bugRow, int bugCol) = (-1, -1);
        int bugCellCount = 0;

        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (cells[r, c].IsFixed()) continue;

                int count = allCellCandidates[r, c].Count;

                if (count == 2)
                    continue; // Normal bivalue hücre, BUG koşuluyla uyumlu

                if (count == 3 && bugCellCount == 0)
                {
                    // İlk ve tek 3-adaylı hücre → BUG+1 adayı
                    bugRow = r;
                    bugCol = c;
                    bugCellCount++;
                }
                else
                {
                    // 2'den fazla veya 3-adaylı ikinci hücre → BUG koşulu sağlanmıyor
                    return false;
                }
            }
        }

        // BUG+1 hücresi bulunamadı (hepsi 2-adaylı veya hiç çözülmemiş hücre yok)
        if (bugCellCount != 1) return false;

        // BUG+1 hücresinin 3 adayından hangisi satır, sütun ve blokta 3 kez görünüyor?
        List<int> bugCands = allCellCandidates[bugRow, bugCol];

        foreach (int digit in bugCands)
        {
            int countInRow = CountDigitInUnit(digit, allCellCandidates, cells, GetRow(bugRow));
            int countInCol = CountDigitInUnit(digit, allCellCandidates, cells, GetCol(bugCol));
            int countInBlock = CountDigitInUnit(digit, allCellCandidates, cells, GetBlock(bugRow, bugCol));

            // BUG+1 hücresini de saydığımız için 3 kez görünmeli
            if (countInRow == 3 && countInCol == 3 && countInBlock == 3)
            {
                // Bu digit BUG+1 hücresinin çözümü
                cells[bugRow, bugCol].SetValue(digit);
                allCellCandidates[bugRow, bugCol].Clear();
                cells[bugRow, bugCol].SetFixed(true);
                return true;
            }
        }

        return false;
    }

    // Verilen birim (hücre listesi) içinde digit kaç hücrede aday olarak var?
    private int CountDigitInUnit(
        int digit,
        List<int>[,] allCellCandidates,
        SudokuCell[,] cells,
        List<(int r, int c)> unit)
    {
        int count = 0;
        foreach (var (r, c) in unit)
        {
            if (cells[r, c].IsFixed()) continue;
            if (allCellCandidates[r, c].Contains(digit)) count++;
        }
        return count;
    }

    private List<(int r, int c)> GetRow(int row)
    {
        var list = new List<(int, int)>();
        for (int c = 0; c < 9; c++) list.Add((row, c));
        return list;
    }

    private List<(int r, int c)> GetCol(int col)
    {
        var list = new List<(int, int)>();
        for (int r = 0; r < 9; r++) list.Add((r, col));
        return list;
    }

    private List<(int r, int c)> GetBlock(int row, int col)
    {
        var list = new List<(int, int)>();
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int r = startRow; r < startRow + 3; r++)
            for (int c = startCol; c < startCol + 3; c++)
                list.Add((r, c));
        return list;
    }
}
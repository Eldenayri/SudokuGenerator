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
                    if (cand != null && cand.Count == 2)
                    {
                        bivalueCells.Add((r, c, cand[0], cand[1]));
                    }
                }
            }
        }

        // 2. Her bir bivalue hücreden başlayarak olası zincirleri dene (DFS)
        foreach (var start in bivalueCells)
        {
            // Durum 1: Zincirin "Hedef (Target)" rakamı v1 olsun, yola (Link) v2'den bağlanarak çıkalım:
            // "Eğer bu hücre v1 değilse, kesinlikle v2'dir. O halde v2 şuraya gider..." mantığı.
            if (FindChain(start, targetValue: start.v1, linkValue: start.v2, bivalueCells, allCellCandidates, cells, new List<(int, int)>() { (start.r, start.c) }))
                return true; // Bir değişiklik yapıldıysa turu bitir, grid güncellendi.

            // Durum 2: Zincirin "Hedef (Target)" rakamı v2 olsun, yola (Link) v1'den çıkalım:
            if (FindChain(start, targetValue: start.v2, linkValue: start.v1, bivalueCells, allCellCandidates, cells, new List<(int, int)>() { (start.r, start.c) }))
                return true;
        }

        return false;
    }

    /// <summary>
    /// DFS (Depth-First Search) ile olası XY zincirlerini bulur.
    /// </summary>
    private bool FindChain(
        (int r, int c, int v1, int v2) current,
        int targetValue,
        int linkValue,
        List<(int r, int c, int v1, int v2)> allBivalue,
        List<int>[,] a,
        SudokuCell[,] cells,
        List<(int r, int c)> visited) // Path
    {
        foreach (var next in allBivalue)
        {
            // Sonsuz döngü engeli: Zincirde daha önce geçtiğimiz bir hücreye dönmeyelim
            if (visited.Contains((next.r, next.c)))
                continue;

            // 1. KURAL: Bir sonraki halka asıl hücreyi "Sudoku" mantığında görmelidir (Peer olmalı)
            if (!CanSee(current.r, current.c, next.r, next.c))
                continue;

            // 2. KURAL: Sonraki hücre, bizim aradığımız bağlantı sayısını barındırmalı
            if (next.v1 != linkValue && next.v2 != linkValue)
                continue;

            // Gelen bağlantıyı harcadık, artık bir sonraki adım için elimizde öteki sayı kalıyor
            int nextLink = (next.v1 == linkValue) ? next.v2 : next.v1;

            // ZİNCİR TAMAMLANDI MI?
            // "Eğer bu yeni ulaştığımız hücrenin sondaki rakamı (nextLink) ilk başladığımız 'targetValue' ile aynıysa"
            // VE "Zincir Pivot mantığına ulaştıysa (Kendisi hariç en az 2 hücre gereklidir; start -> pivot -> end)"
            if (nextLink == targetValue && visited.Count >= 2)
            {
                var startCell = visited[0];
                // Ortak peer'lardan silmeyi dene. Gerçekten silinecek aday(lar) varsa bu zincir geçerlidir.
                if (ApplyElimination(startCell, next, targetValue, a, cells))
                    return true;
            }

            // Zinciri daha da uzatmayı dene (Hedefi vurmuş ama hala bir şey silememiş olabiliriz, aramaya devam etmeliyiz)
            visited.Add((next.r, next.c)); // Yola ekle

            if (FindChain(next, targetValue, nextLink, allBivalue, a, cells, visited))
                return true; // Alt dallarda değişiklik bulduysak silsileyi bitirip true döndür

            visited.RemoveAt(visited.Count - 1); // Başarısız olduysa yoldan çıkart (Backtrack/Geri çekil)
        }

        return false;
    }

    /// <summary>
    /// Başlangıç hücresi ile bitiş hücresinin İKİSİNİ BİRDEN gören ortak (mutual peer) hücrelerden, hedef(target) adayı siler.
    /// </summary>
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

                // Başlangıç ve bitiş pivotlarının kendilerini asla silmemeliyiz
                if ((r == start.r && c == start.c) || (r == end.r && c == end.c)) continue;

                var cand = a[r, c];
                if (cand == null || !cand.Contains(targetCand)) continue;

                // Silinecek hücre SADECE hem başlangıcı (start) hem de sonu (end) ayı anda görebiliyorsa aday silinebilir.
                if (CanSee(r, c, start.r, start.c) && CanSee(r, c, end.r, end.c))
                {
                    int before = cand.Count;
                    cand.Remove(targetCand);
                    if (cand.Count != before) changed = true;
                }
            }
        }

        return changed; // Eğer listeden 1 tane bile targetCand sildiyse true döner
    }

    /// <summary>
    /// İki hücrenin birbirini satır, sütun veya aynı blok bağlamında görüp görmediğini kontrol eder.
    /// </summary>
    private bool CanSee(int r1, int c1, int r2, int c2)
    {
        if (r1 == r2 && c1 == c2) return false; // Aynı hücre kendini görebilir bağlamında değildir.
        if (r1 == r2) return true; // Aynı satır
        if (c1 == c2) return true; // Aynı sütun
        return (r1 / 3 == r2 / 3) && (c1 / 3 == c2 / 3); // Aynı 3x3 blok
    }
}
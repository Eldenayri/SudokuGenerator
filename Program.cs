using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace SudokuFilter

{
    class Program
    {
        private const string InputCsv = "input.csv";
        private const string OutputDir = "output";

        private const int PuzzleIndex = 0;
        private const int SolutionIndex = 1;
        private const int DifficultyIndex = 2;

        private const int TargetPerLevel = 3000;
        private const int LevelCount = 6;
        private const int MinEmptyCells = 25;
        private const int AllDigitsMask = 0x1FF;
        private const int ProgressInterval = 10000;

        // Durdurma sinyali
        private static volatile bool stopRequested = false;

        static void Main()
        {
            Directory.CreateDirectory(OutputDir);

            // 1. Durdurma tuşu için arka plan thread (Sizin kodunuz, aynen duruyor)
            Thread inputThread = new Thread(() =>
            {
                Console.WriteLine("Durdurmak için 'Q' tuşuna bas.");
                Console.WriteLine();
                while (!stopRequested)
                {
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Q)
                        {
                            stopRequested = true;
                            Console.WriteLine();
                            Console.WriteLine(">>> Durdurma isteği alındı, mevcut satır tamamlanıyor...");
                        }
                    }
                    Thread.Sleep(100);
                }
            });
            inputThread.IsBackground = true;
            inputThread.Start();

            // 2. Her seviye için writer ve sayaç (Aynen duruyor)
            var writers = new StreamWriter[LevelCount + 1];
            var counts = new int[LevelCount + 1];
            for (int i = 1; i <= LevelCount; i++)
            {
                string path = Path.Combine(OutputDir, $"level{i}.csv");
                writers[i] = new StreamWriter(path, false, new UTF8Encoding(false));
            }

            // *** YENİ: StreamReader YERİNE JENERATÖR KULLANIYORUZ ***
            var generator = new SudokuGenerator();

            long totalGenerated = 0;
            long written = 0;
            int progressLineCount = 0;

            // 3. Ana Döngü (CSV okumak yerine Jeneratörden puzzle istiyoruz)
            while (!stopRequested)
            {
                if (AllLevelsFull(counts)) break; // Hepsi 3000 olunca biter

                // Jeneratörden 1 tane geçerli (tek çözümlü) bulmaca üretip getirsin
                var (puzzle, solution, level) = generator.GenerateOne();
                totalGenerated++;

                // Her ProgressInterval adet üretimde bir ekranı temizle ve yeniden yaz
                if (totalGenerated % ProgressInterval == 0)
                {
                    ClearLines(progressLineCount);
                    progressLineCount = PrintProgress(totalGenerated, written, counts);
                }

                // Eğer bulmacanın leveli 1-6 arasındaysa ve o level klasörü henüz 3000 olmadıysa kaydet
                if (level >= 1 && level <= LevelCount && counts[level] < TargetPerLevel)
                {
                    string csvLine = $"{puzzle},{solution},{level}";
                    writers[level].WriteLine(csvLine);
                    writers[level].Flush();
                    counts[level]++;
                    written++;
                }
            }

            // Yazıcıları kapat
            for (int i = 1; i <= LevelCount; i++)
                writers[i]?.Close();

            // Son durumu temizle ve yazdır
            ClearLines(progressLineCount);
            Console.WriteLine();
            Console.WriteLine(stopRequested ? "=== Q TUŞU İLE DURDURULDU ===" : "=== TÜM SEVİYELER TAMAMLANDI ===");
            Console.WriteLine($"Toplam Üretilen ve Test Edilen : {totalGenerated}");
            Console.WriteLine($"Diske Kaydedilen               : {written}");
            Console.WriteLine();
            Console.WriteLine("Seviye bazında yazılanlar:");
            for (int i = 1; i <= LevelCount; i++)
                Console.WriteLine($"  Level {i}: {counts[i]}/{TargetPerLevel}");

            Console.WriteLine();
            Console.WriteLine("Çıkmak için bir tuşa bas.");
            Console.ReadKey();
        }

        // Konsolda yukarı çıkarak satırları temizle
        static void ClearLines(int lineCount)
        {
            for (int i = 0; i < lineCount; i++)
            {
                Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
                Console.Write(new string(' ', Console.WindowWidth));
                Console.CursorLeft = 0;
            }
        }

        // Progress bilgisini yaz, kaç satır yazdığını döndür
        static int PrintProgress(long total, long written, int[] counts)
        {
            int lines = 0;
            void W(string s) { Console.WriteLine(s); lines++; }

            W($"Üretilip Test Edilen : {total,10}   Dosyaya Yazılan : {written,6}");

            string levelLine = "Seviye :";
            for (int i = 1; i <= LevelCount; i++)
                levelLine += $"  L{i}={counts[i],4}/{TargetPerLevel}";
            W(levelLine);

            return lines;
        }

        static bool AllLevelsFull(int[] counts)
        {
            for (int i = 1; i <= LevelCount; i++)
                if (counts[i] < TargetPerLevel) return false;
            return true;
        }


    /*static void Main()
    {
        Directory.CreateDirectory(OutputDir);

        // Durdurma tuşu için arka plan thread
        Thread inputThread = new Thread(() =>
        {
            Console.WriteLine("Durdurmak için 'Q' tuşuna bas.");
            Console.WriteLine();
            while (!stopRequested)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        stopRequested = true;
                        Console.WriteLine();
                        Console.WriteLine(">>> Durdurma isteği alındı, mevcut satır tamamlanıyor...");
                    }
                }
                Thread.Sleep(100);
            }
        });
        inputThread.IsBackground = true;
        inputThread.Start();

        // Her seviye için writer ve sayaç
        var writers = new StreamWriter[LevelCount + 1];
        var counts = new int[LevelCount + 1];
        for (int i = 1; i <= LevelCount; i++)
        {
            string path = Path.Combine(OutputDir, $"level{i}.csv");
            writers[i] = new StreamWriter(path, false, new UTF8Encoding(false));
        }

        var solver = new SudokuSolver();

        long totalLines = 0;
        long skippedBadFormat = 0;
        long skippedFewClues = 0;
        long skippedInvalidSolution = 0;
        long skippedMultipleSolution = 0;
        long skippedUnsolvable = 0;
        long written = 0;

        // Son progress satırının konsolda kaç satır kapladığını takip et
        int progressLineCount = 0;

        using var reader = new StreamReader(InputCsv, Encoding.UTF8);

        string? line;
        bool firstLine = true;

        while (!stopRequested && (line = reader.ReadLine()) != null)
        {
            if (AllLevelsFull(counts)) break;

            totalLines++;

            if (string.IsNullOrWhiteSpace(line)) continue;

            // Header satırını atla
            if (firstLine)
            {
                firstLine = false;
                if (!char.IsDigit(line[0])) continue;
            }

            // Her ProgressInterval satırda bir ekranı temizle ve yeniden yaz
            if (totalLines % ProgressInterval == 0)
            {
                // Önceki progress satırlarını sil
                ClearLines(progressLineCount);
                progressLineCount = PrintProgress(totalLines, written, skippedBadFormat,
                    skippedFewClues, skippedInvalidSolution, skippedMultipleSolution,
                    skippedUnsolvable, counts);
            }

            var parts = line.Split(',');
            if (parts.Length < 2) { skippedBadFormat++; continue; }

            string puzzle = parts[PuzzleIndex].Trim();
            string solution = parts.Length > SolutionIndex ? parts[SolutionIndex].Trim() : "";

            // Format kontrolü
            if (!IsPuzzleStringValid(puzzle)) { skippedBadFormat++; continue; }
            if (solution.Length == 81 && !IsSolutionStringValid(solution)) { skippedBadFormat++; continue; }

            // Boş kare sayısı kontrolü
            int emptyCells = CountEmpty(puzzle);
            if (emptyCells < MinEmptyCells) { skippedFewClues++; continue; }

            // Çözüm varsa geçerliliğini kontrol et
            if (solution.Length == 81)
            {
                if (!IsGivenSolutionValidForPuzzle(puzzle, solution))
                { skippedInvalidSolution++; continue; }

                if (HasAlternativeSolution(puzzle, solution))
                { skippedMultipleSolution++; continue; }
            }

            // Solver ile zorluk seviyesini belirle
            // Çözülemeyen veya kısmen çözülen bulmacaları atla
            SolveResult result = solver.Solve(puzzle);

            if (!result.Solved) { skippedUnsolvable++; continue; }

            // Çözücünün bulduğu sonuç, CSV'deki orijinal sonuçtan farklıysa programı durdur!
            if (result.SolutionString != solution)
            {
                Console.WriteLine();
                Console.WriteLine(">>> KRİTİK HATA: Çözücünüz hatalı veya farklı bir çözüm üretti! <<<");
                Console.WriteLine($"Hatalı Satır No: {totalLines}");
                Console.WriteLine($"Puzzle        : {puzzle}");
                Console.WriteLine($"Beklenen Çözüm: {solution}");
                Console.WriteLine($"Bulunan Çözüm : {result.SolutionString}");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine(">>> BU ÇÖZÜMDE KULLANILAN TEKNİKLER (Hata muhtemelen bunlardan birinde):");

                if (result.NakedPairs > 0) Console.WriteLine($"- Naked Pairs: {result.NakedPairs} kez kullanıldı");
                if (result.NakedTriples > 0) Console.WriteLine($"- Naked Triples: {result.NakedTriples} kez kullanıldı");
                if (result.NakedQuads > 0) Console.WriteLine($"- Naked Quads: {result.NakedQuads} kez kullanıldı");

                if (result.HiddenPairs > 0) Console.WriteLine($"- Hidden Pairs: {result.HiddenPairs} kez kullanıldı");
                if (result.HiddenTriples > 0) Console.WriteLine($"- Hidden Triples: {result.HiddenTriples} kez kullanıldı");
                if (result.HiddenQuads > 0) Console.WriteLine($"- Hidden Quads: {result.HiddenQuads} kez kullanıldı");

                if (result.PointingPairs > 0) Console.WriteLine($"- Pointing Pairs / Intersections: {result.PointingPairs} kez kullanıldı");
                if (result.BoxLineReduction > 0) Console.WriteLine($"- Box Line Reduction: {result.BoxLineReduction} kez kullanıldı");

                if (result.XWing > 0) Console.WriteLine($"- X-Wing: {result.XWing} kez kullanıldı");
                if (result.Swordfish > 0) Console.WriteLine($"- Swordfish: {result.Swordfish} kez kullanıldı");
                if (result.Jellyfish > 0) Console.WriteLine($"- Jellyfish: {result.Jellyfish} kez kullanıldı");

                if (result.YWing > 0) Console.WriteLine($"- Y-Wing (XY-Wing): {result.YWing} kez kullanıldı");
                if (result.XYZWing > 0) Console.WriteLine($"- XYZ-Wing: {result.XYZWing} kez kullanıldı");
                if (result.WWing > 0) Console.WriteLine($"- W-Wing: {result.WWing} kez kullanıldı");

                if (result.UniqueRectangle > 0) Console.WriteLine($"- Unique Rectangle: {result.UniqueRectangle} kez kullanıldı");
                if (result.Skyscraper > 0) Console.WriteLine($"- Skyscraper: {result.Skyscraper} kez kullanıldı");
                if (result.XYChain > 0) Console.WriteLine($"- XY-Chain: {result.XYChain} kez kullanıldı");
                if (result.SimpleColouring > 0) Console.WriteLine($"- Simple Colouring: {result.SimpleColouring} kez kullanıldı");

                break; // Döngüyü kırıp programı durdurur
            }

            int level = result.DifficultyLevel;
            if (level < 1 || level > LevelCount) continue;
            if (counts[level] >= TargetPerLevel) continue;

            writers[level].WriteLine(puzzle);
            writers[level].Flush();
            counts[level]++;
            written++;
        }

        // Yazıcıları kapat
        for (int i = 1; i <= LevelCount; i++)
            writers[i]?.Close();

        // Son durumu temizle ve yazdır
        ClearLines(progressLineCount);
        Console.WriteLine();
        Console.WriteLine(stopRequested ? "=== DURDURULDU ===" : "=== TAMAMLANDI ===");
        Console.WriteLine($"Toplam okunan       : {totalLines}");
        Console.WriteLine($"Toplam yazılan      : {written}");
        Console.WriteLine($"Format hatası       : {skippedBadFormat}");
        Console.WriteLine($"Az boş kare         : {skippedFewClues}");
        Console.WriteLine($"Geçersiz çözüm      : {skippedInvalidSolution}");
        Console.WriteLine($"Çoklu çözüm         : {skippedMultipleSolution}");
        Console.WriteLine($"Çözülemeyen         : {skippedUnsolvable}");
        Console.WriteLine();
        Console.WriteLine("Seviye bazında yazılanlar:");
        for (int i = 1; i <= LevelCount; i++)
            Console.WriteLine($"  Level {i}: {counts[i]}/{TargetPerLevel}");

        Console.WriteLine();
        Console.WriteLine("Çıkmak için bir tuşa bas.");
        Console.ReadKey();
    }

    // Konsolda yukarı çıkarak satırları temizle
    static void ClearLines(int lineCount)
    {
        for (int i = 0; i < lineCount; i++)
        {
            Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.CursorLeft = 0;
        }
    }

    // Progress bilgisini yaz, kaç satır yazdığını döndür
    static int PrintProgress(long total, long written,
        long badFormat, long fewClues, long invalidSol, long multiSol, long unsolvable,
        int[] counts)
    {
        int lines = 0;

        void W(string s) { Console.WriteLine(s); lines++; }

        W($"Okunan : {total,10}   Yazılan : {written,6}");
        W($"Format : {badFormat,10}   AzBoş   : {fewClues,6}   Geçersiz : {invalidSol,6}   Çoklu : {multiSol,6}   Çözülemez : {unsolvable,6}");

        string levelLine = "Seviye :";
        for (int i = 1; i <= LevelCount; i++)
            levelLine += $"  L{i}={counts[i],4}/{3000}";
        W(levelLine);

        return lines;
    }

    // ─── Yardımcı metodlar ───────────────────────────────────────────────

    static bool AllLevelsFull(int[] counts)
    {
        for (int i = 1; i <= LevelCount; i++)
            if (counts[i] < TargetPerLevel) return false;
        return true;
    }*/

    static int CountEmpty(string puzzle)
        {
            int count = 0;
            for (int i = 0; i < 81; i++)
                if (puzzle[i] == '0') count++;
            return count;
        }

        static bool IsPuzzleStringValid(string s)
        {
            if (s.Length != 81) return false;
            for (int i = 0; i < 81; i++)
                if (s[i] < '0' || s[i] > '9') return false;
            return true;
        }

        static bool IsSolutionStringValid(string s)
        {
            if (s.Length != 81) return false;
            for (int i = 0; i < 81; i++)
                if (s[i] < '1' || s[i] > '9') return false;
            return true;
        }

        static bool IsGivenSolutionValidForPuzzle(string puzzle, string solution)
        {
            for (int i = 0; i < 81; i++)
            {
                int p = puzzle[i] - '0';
                int s = solution[i] - '0';
                if (p != 0 && p != s) return false;
            }
            return IsFullBoardValid(solution);
        }

        static bool IsFullBoardValid(string board)
        {
            for (int r = 0; r < 9; r++)
            {
                int mask = 0;
                for (int c = 0; c < 9; c++)
                {
                    int bit = 1 << (board[r * 9 + c] - '1');
                    if ((mask & bit) != 0) return false;
                    mask |= bit;
                }
            }
            for (int c = 0; c < 9; c++)
            {
                int mask = 0;
                for (int r = 0; r < 9; r++)
                {
                    int bit = 1 << (board[r * 9 + c] - '1');
                    if ((mask & bit) != 0) return false;
                    mask |= bit;
                }
            }
            for (int br = 0; br < 3; br++)
                for (int bc = 0; bc < 3; bc++)
                {
                    int mask = 0;
                    for (int r = 0; r < 3; r++)
                        for (int c = 0; c < 3; c++)
                        {
                            int bit = 1 << (board[(br * 3 + r) * 9 + (bc * 3 + c)] - '1');
                            if ((mask & bit) != 0) return false;
                            mask |= bit;
                        }
                }
            return true;
        }

        static bool HasAlternativeSolution(string puzzle, string givenSolution)
        {
            int[] board = new int[81];
            int[] rowMask = new int[9];
            int[] colMask = new int[9];
            int[] boxMask = new int[9];
            int[] expected = new int[81];

            for (int i = 0; i < 81; i++)
                expected[i] = givenSolution[i] - '0';

            for (int i = 0; i < 81; i++)
            {
                int val = puzzle[i] - '0';
                board[i] = val;
                if (val == 0) continue;

                int r = i / 9, c = i % 9, b = (r / 3) * 3 + (c / 3);
                int bit = 1 << (val - 1);

                if ((rowMask[r] & bit) != 0 || (colMask[c] & bit) != 0 || (boxMask[b] & bit) != 0)
                    return true;

                rowMask[r] |= bit;
                colMask[c] |= bit;
                boxMask[b] |= bit;
            }

            return SearchAlternative(board, rowMask, colMask, boxMask, expected);
        }

        static bool SearchAlternative(int[] board, int[] rowMask, int[] colMask, int[] boxMask, int[] expected)
        {
            int bestIndex = -1, bestMask = 0, bestCount = 10;

            for (int i = 0; i < 81; i++)
            {
                if (board[i] != 0) continue;

                int r = i / 9, c = i % 9, b = (r / 3) * 3 + (c / 3);
                int candidates = AllDigitsMask & ~(rowMask[r] | colMask[c] | boxMask[b]);
                int count = BitCount(candidates);

                if (count == 0) return false;
                if (count < bestCount)
                {
                    bestCount = count;
                    bestMask = candidates;
                    bestIndex = i;
                    if (count == 1) break;
                }
            }

            if (bestIndex == -1)
            {
                for (int i = 0; i < 81; i++)
                    if (board[i] != expected[i]) return true;
                return false;
            }

            int row = bestIndex / 9, col = bestIndex % 9, box = (row / 3) * 3 + (col / 3);
            int expectedBit = 1 << (expected[bestIndex] - 1);
            int otherMask = bestMask & ~expectedBit;

            while (otherMask != 0)
            {
                int bit = otherMask & -otherMask;
                int digit = BitToDigit(bit);
                Place(board, rowMask, colMask, boxMask, bestIndex, row, col, box, digit, bit);
                if (SearchAlternative(board, rowMask, colMask, boxMask, expected)) return true;
                Remove(board, rowMask, colMask, boxMask, bestIndex, row, col, box, bit);
                otherMask &= ~bit;
            }

            if ((bestMask & expectedBit) != 0)
            {
                Place(board, rowMask, colMask, boxMask, bestIndex, row, col, box, expected[bestIndex], expectedBit);
                if (SearchAlternative(board, rowMask, colMask, boxMask, expected)) return true;
                Remove(board, rowMask, colMask, boxMask, bestIndex, row, col, box, expectedBit);
            }

            return false;
        }

        static void Place(int[] board, int[] rowMask, int[] colMask, int[] boxMask,
            int index, int row, int col, int box, int digit, int bit)
        {
            board[index] = digit;
            rowMask[row] |= bit;
            colMask[col] |= bit;
            boxMask[box] |= bit;
        }

        static void Remove(int[] board, int[] rowMask, int[] colMask, int[] boxMask,
            int index, int row, int col, int box, int bit)
        {
            board[index] = 0;
            rowMask[row] &= ~bit;
            colMask[col] &= ~bit;
            boxMask[box] &= ~bit;
        }

        static int BitCount(int x) { int n = 0; while (x != 0) { x &= x - 1; n++; } return n; }
        static int BitToDigit(int bit) { int d = 1; while (bit > 1) { bit >>= 1; d++; } return d; }
    }
}

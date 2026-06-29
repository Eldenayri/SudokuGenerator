using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

    class Program
    {
    //private const string OutputDir = "output";
        private static string OutputDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "output" );
        private const int TargetPerLevel = 3000;
        private const int LevelCount = 6;
        private const int ProgressInterval = 10;

        private static volatile bool stopRequested = false;

        static void Main()
        {
            Directory.CreateDirectory(OutputDir);

            // 1. Durdurma tuşu için arka plan thread
            Thread inputThread = new Thread(() =>
            {
                Console.WriteLine("Durdurmak için 'Q' tuşuna bas.");
                Console.WriteLine();
                while (!stopRequested)
                {
                    try
                    {
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(intercept: true);
                            if (key.Key == ConsoleKey.Q)
                            {
                                stopRequested = true;
                                Console.WriteLine();
                                Console.WriteLine(">>> Durdurma isteği alındı, mevcut işlemler tamamlanıp çıkılacak...");
                            }
                        }
                    }
                    catch
                    {
                        // Konsol yönlendirilmişse (örn: > out.txt) KeyAvailable exception fırlatır, sessizce çık
                        break;
                    }
                    Thread.Sleep(100);
                }
            });
            inputThread.IsBackground = true;
            inputThread.Start();

            // 2. Her seviye için writer ve sayaç oluşturma (Dosyadan Okuma ve Ekleme Modu)
            var writers = new StreamWriter[LevelCount + 1];
            var counts = new int[LevelCount + 1];

            for (int i = 1; i <= LevelCount; i++)
            {
                string path = Path.Combine(OutputDir, $"level{i}.csv");

                if (File.Exists(path))
                {
                    // FIX 1: Boş satırları saymıyoruz (trailing newline sorunu)
                    counts[i] = File.ReadAllLines(path).Count(l => l.Trim().Length > 0);
                }
                else
                {
                    counts[i] = 0;
                }

                // Append modu: mevcut dosyaya ekleme yapar, yoksa oluşturur
                writers[i] = new StreamWriter(path, append: true, new UTF8Encoding(false));
            }

            // 3. Sudoku Jeneratörümüzü başlatıyoruz
            var generator = new SudokuGenerator();

            long totalGenerated = 0;
            long written = 0;
            int progressLineCount = 0;

            progressLineCount = PrintProgress(totalGenerated, written, counts);

            // 4. Ana Döngü
            while (!stopRequested)
            {
                if (AllLevelsFull(counts)) break;

                var (puzzle, solution, level) = generator.GenerateOne();
                totalGenerated++;

                if (level >= 1 && level <= LevelCount && counts[level] < TargetPerLevel)
                {
                    string csvLine = $"{puzzle},{solution},{level}";
                    writers[level].WriteLine(csvLine);
                    writers[level].Flush();
                    counts[level]++;
                    written++;

                    // FIX 5: Son puzzle yazıldıktan hemen sonra döngüyü bitir,
                    // gereksiz GenerateOne() çağrısı yapma
                    if (AllLevelsFull(counts)) break;
                }

                if (totalGenerated % ProgressInterval == 0)
                {
                    ClearLines(progressLineCount);
                    progressLineCount = PrintProgress(totalGenerated, written, counts);
                }
            }

            // FIX 4: Close() yerine Dispose() — daha güvenli kaynak serbest bırakma
            for (int i = 1; i <= LevelCount; i++)
                writers[i]?.Dispose();

            ClearLines(progressLineCount);
            Console.WriteLine();
            Console.WriteLine(stopRequested
                ? "=== Q TUŞU İLE DURDURULDU ==="
                : "=== TÜM SEVİYELER BAŞARIYLA TAMAMLANDI ===");
            Console.WriteLine($"Bu Oturumda Üretilip Test Edilen : {totalGenerated}");
            Console.WriteLine($"Bu Oturumda Diske Eklenen        : {written}");
            Console.WriteLine();
            Console.WriteLine("Dosyalardaki SON DURUM (Toplam):");
            for (int i = 1; i <= LevelCount; i++)
                Console.WriteLine($"  Level {i}: {counts[i]}/{TargetPerLevel}");

            Console.WriteLine();
            Console.WriteLine("Çıkmak için bir tuşa bas.");
            Console.ReadKey();
        }

        // --- YARDIMCI METOTLAR ---

        /*static void ClearLines(int lineCount)
        {
            // FIX 2: Konsol yönlendirilmişse exception fırlatabilir, try-catch ile koru
            try
            {
                for (int i = 0; i < lineCount; i++)
                {
                    Console.CursorTop = Math.Max(0, Console.CursorTop - 1);
                    Console.Write(new string(' ', Console.WindowWidth));
                    Console.CursorLeft = 0;
                }
            }
            catch {  Yönlendirme veya dar terminal durumunda sessizce geç  }
        }*/

    static void ClearLines(int lineCount)
    {
        try
        {
            for (int i = 0; i < lineCount; i++)
            {
                Console.Write("\x1B[1A"); // 1 satır yukarı çık
                Console.Write("\x1B[2K"); // satırı tamamen sil
            }
        }
    catch { }
    }
    
    static int PrintProgress(long total, long written, int[] counts)
        {
            int lines = 0;
            void W(string s) { Console.WriteLine(s); lines++; }

            W($"Şu an Üretilen : {total,10}   Bu Oturumda Eklenen : {written,6}");

            string levelLine = "Veritabanı Durumu :";
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
    }

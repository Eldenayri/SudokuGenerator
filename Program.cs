using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static string OutputDir = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "output");

    private const int TargetPerBucket = 1000;
    private const int ProgressIntervalMs = 2000;

    private static volatile bool stopRequested = false;

    // Kova tanımları: Level1 = NakedSingle + HiddenSingle (ileri teknik gerektirmez) +
    // TechniqueCatalog'daki her teknik kendi tier'ında bir kova. Tek doğruluk kaynağı TechniqueCatalog'dur.
    private static readonly List<(int Level, string Technique)> BucketKeys = BuildBucketKeys();

    private static readonly Dictionary<(int Level, string Technique), int[]> counters =
        BucketKeys.ToDictionary(k => k, _ => new int[1]);
    // BUG hedefi 0: ölçümle doğrulandı ki bu teknik hiyerarşisinde BUG, aynı tier'deki
    // WWing/XYChain tarafından yapısal olarak gölgede bırakılıyor (WWing/XYChain tier-6
    // durumların çoğunda BUG'dan önce zaten çözüyor) — emsalleri açıkken tek başına
    // zorunlu olduğu bir puzzle bulmak neredeyse imkansız. Hedefi 0 yaparak program
    // sonsuza kadar bu kovayı beklemeden diğer 19 kova dolunca doğal olarak bitebiliyor.
    private static readonly Dictionary<(int Level, string Technique), int> targets =
        BucketKeys.ToDictionary(k => k, k => k.Technique == "BUG" ? 0 : TargetPerBucket);

    private static readonly StreamWriter[] writers = new StreamWriter[7]; // index 1..6
    private static readonly object[] levelLocks =
        { new object(), new object(), new object(), new object(), new object(), new object(), new object() };

    private static long totalBoards = 0;
    private static long totalDigSteps = 0;
    private static long totalWritten = 0;

    static List<(int, string)> BuildBucketKeys()
    {
        var list = new List<(int, string)> { (1, "NakedSingle"), (1, "HiddenSingle") };
        foreach (var (name, tier) in TechniqueCatalog.All)
            list.Add((tier, name));
        return list;
    }

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
                    break;
                }
                Thread.Sleep(100);
            }
        });
        inputThread.IsBackground = true;
        inputThread.Start();

        // 2. Var olan levelN.csv'ler varsa: içindeki satırları (4. kolon = teknik adı)
        // sayıp ilgili kovanın sayacını oradan başlatır, sonra dosyaya EKLEME (append)
        // modunda devam eder. Böylece program kapatılıp yeniden açılınca kaldığı
        // yerden sürer, daha önce bulunan satırlar kaybolmaz/silinmez.
        // 3. kolon artık level numarası DEĞİL, oyunun "çözüldü mü" bayrağı (0/1) - level
        // zaten dosya adından (levelN.csv) belli olduğu için parse edilmiyor, direkt
        // döngünün kendi level değişkeni kullanılıyor.
        for (int level = 1; level <= 6; level++)
        {
            string path = Path.Combine(OutputDir, $"level{level}.csv");

            if (File.Exists(path))
            {
                foreach (string line in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    string[] parts = line.Split(',');
                    if (parts.Length < 4) continue;

                    var key = (level, parts[3].Trim());
                    if (counters.TryGetValue(key, out int[] counter))
                        counter[0]++;
                }
            }

            writers[level] = new StreamWriter(path, append: true, new UTF8Encoding(false));
        }

        var sw = Stopwatch.StartNew();
        int progressLineCount = PrintProgress(sw.ElapsedMilliseconds);
        var lastProgress = Stopwatch.StartNew();

        // 3. Çoklu thread worker'lar
        int workerCount = Math.Max(1, Environment.ProcessorCount);
        var workers = new Task[workerCount];
        for (int w = 0; w < workerCount; w++)
            workers[w] = Task.Run(WorkerLoop);

        // 4. Ana thread sadece ilerlemeyi basar ve bitiş koşulunu bekler
        while (!stopRequested && !AllBucketsFull())
        {
            if (lastProgress.ElapsedMilliseconds > ProgressIntervalMs)
            {
                ClearLines(progressLineCount);
                progressLineCount = PrintProgress(sw.ElapsedMilliseconds);
                lastProgress.Restart();
            }
            Thread.Sleep(200);
        }
        stopRequested = true;

        Task.WaitAll(workers);

        for (int level = 1; level <= 6; level++)
            writers[level]?.Dispose();

        ClearLines(progressLineCount);
        Console.WriteLine();
        Console.WriteLine(AllBucketsFull()
            ? "=== TÜM KOVALAR DOLDU ==="
            : "=== Q TUŞU İLE DURDURULDU ===");
        Console.WriteLine($"Toplam üretilen tam tahta   : {totalBoards}");
        Console.WriteLine($"Toplam denenen (delik) adım : {totalDigSteps}");
        Console.WriteLine($"Diske yazılan bulmaca       : {totalWritten}");
        Console.WriteLine();
        PrintProgress(sw.ElapsedMilliseconds, permanent: true);

        Console.WriteLine();
        Console.WriteLine("Çıkmak için bir tuşa bas.");
        Console.ReadKey();
    }

    // --- WORKER ---

    static void WorkerLoop()
    {
        // Her worker kendi solver/generator/random örneğini kullanır (thread-safety).
        var solver = new SudokuSolver();
        var classifier = new PuzzleClassifier(solver);
        var generator = new SudokuGenerator();

        while (!stopRequested && !AllBucketsFull())
        {
            Interlocked.Increment(ref totalBoards);

            // Bir tier'da art arda birden fazla adım kalınabilir (delik açtıkça hâlâ
            // aynı teknik yetiyor); ilk yakalanan anı değil, o tier'de kalınan EN DERİN
            // (en çok boş hücreli) anı kaydetmek istiyoruz. Bu yüzden bir adım geriden
            // takip edip ("pending"), tier değiştiği ya da çözülemez hale geldiği anda
            // bir önceki (en derin) durumu değerlendiriyoruz.
            SolveResult pendingNatural = null;
            string pendingPuzzle = null, pendingSolution = null;
            HashSet<(int Level, string Technique)> pendingOpen = null;

            void Flush()
            {
                if (pendingNatural == null) return;
                TryAssignAndWrite(classifier, pendingPuzzle, pendingSolution, pendingNatural, pendingOpen);
                pendingNatural = null;
            }

            foreach (var (puzzle, solution) in generator.DigProgressively())
            {
                Interlocked.Increment(ref totalDigSteps);

                if (stopRequested) break;

                var open = OpenBucketsSnapshot();
                if (open.Count == 0) break;

                var natural = classifier.SolveNatural(puzzle);

                bool ceilingChanged = pendingNatural != null &&
                    (!natural.Solved || natural.DifficultyLevel != pendingNatural.DifficultyLevel);
                if (ceilingChanged) Flush();

                if (!natural.Solved) break; // çözülemez oldu, bu daldan daha derine inmenin anlamı yok

                pendingNatural = natural;
                pendingPuzzle = puzzle;
                pendingSolution = solution;
                pendingOpen = open;
            }

            Flush(); // dal doğal olarak bitti (delik tükendi) ya da yukarıda zaten boşaltıldı (no-op)
        }
    }

    static void TryAssignAndWrite(PuzzleClassifier classifier, string puzzle, string solution,
        SolveResult natural, ISet<(int Level, string Technique)> openBuckets)
    {
        var match = classifier.Assign(puzzle, natural, openBuckets);
        if (match == null) return;

        var key = match.Value;
        if (!TryClaim(key)) return;

        WriteResult(key.Level, puzzle, solution, key.Technique);
        Interlocked.Increment(ref totalWritten);
    }

    static HashSet<(int Level, string Technique)> OpenBucketsSnapshot()
    {
        var set = new HashSet<(int Level, string Technique)>();
        foreach (var key in BucketKeys)
            if (Volatile.Read(ref counters[key][0]) < targets[key])
                set.Add(key);
        return set;
    }

    static bool TryClaim((int Level, string Technique) key)
    {
        var counter = counters[key];
        int target = targets[key];
        while (true)
        {
            int current = Volatile.Read(ref counter[0]);
            if (current >= target) return false;
            if (Interlocked.CompareExchange(ref counter[0], current + 1, current) == current) return true;
        }
    }

    static bool AllBucketsFull()
    {
        foreach (var key in BucketKeys)
            if (Volatile.Read(ref counters[key][0]) < targets[key]) return false;
        return true;
    }

    static void WriteResult(int level, string puzzle, string solution, string technique)
    {
        // 3. kolon oyundaki "çözüldü mü" bayrağı (0/1); yeni üretilen her satır 0 ile başlar.
        string line = $"{puzzle},{solution},0,{technique}";
        lock (levelLocks[level])
        {
            writers[level].WriteLine(line);
            writers[level].Flush();
        }
    }

    // --- İLERLEME EKRANI ---

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

    static int PrintProgress(long elapsedMs, bool permanent = false)
    {
        int lines = 0;
        void W(string s) { Console.WriteLine(s); lines++; }

        long sec = elapsedMs / 1000;
        W($"Süre: {sec,6}sn   Tahta: {Volatile.Read(ref totalBoards),10}   Denenen adım: {Volatile.Read(ref totalDigSteps),10}   Yazılan: {Volatile.Read(ref totalWritten),6}");

        for (int level = 1; level <= 6; level++)
        {
            var levelBuckets = BucketKeys.Where(k => k.Level == level);
            string row = $"L{level}: " + string.Join("  ", levelBuckets.Select(k =>
                $"{k.Technique}={Volatile.Read(ref counters[k][0])}/{targets[k]}"));
            W(row);
        }

        return lines;
    }
}

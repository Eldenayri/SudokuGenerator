using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal static class Program
{
    private static readonly string OutputDir = Path.GetFullPath(Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "..", "..", "..", "output"));

    private const int TargetPerLevel = 2000;
    private const int LevelCount = 6;
    private const int ProgressIntervalMs = 2000;

    private static readonly int[] Counts = new int[LevelCount + 1];
    private static readonly StreamWriter[] Writers = new StreamWriter[LevelCount + 1];
    private static readonly object[] LevelLocks =
        Enumerable.Range(0, LevelCount + 1).Select(_ => new object()).ToArray();
    private static readonly ConcurrentDictionary<string, byte> KnownPuzzles = new();

    private static volatile bool stopRequested;
    private static long totalBoards;
    private static long totalDigSteps;
    private static long totalWritten;

    private static void Main()
    {
        Directory.CreateDirectory(OutputDir);
        LoadExistingCsvFiles();

        var inputThread = new Thread(WatchForStopKey) { IsBackground = true };
        inputThread.Start();

        var stopwatch = Stopwatch.StartNew();
        Task[] workers = Enumerable
            .Range(0, Math.Max(1, Environment.ProcessorCount))
            .Select(_ => Task.Run(WorkerLoop))
            .ToArray();

        while (!stopRequested && !AllLevelsFull())
        {
            PrintProgress(stopwatch.Elapsed);
            Thread.Sleep(ProgressIntervalMs);
        }

        stopRequested = true;
        Task.WaitAll(workers);

        for (int level = 1; level <= LevelCount; level++)
            Writers[level].Dispose();

        PrintProgress(stopwatch.Elapsed);
        Console.WriteLine(AllLevelsFull()
            ? "Tüm seviyeler hedefe ulaştı."
            : "Üretim durduruldu.");
    }

    private static void LoadExistingCsvFiles()
    {
        for (int level = 1; level <= LevelCount; level++)
        {
            string path = GetLevelPath(level);
            int count = 0;

            if (File.Exists(path))
            {
                foreach (string rawLine in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(rawLine))
                        continue;

                    count++;
                    string[] parts = rawLine.Split(',');
                    if (parts.Length > 0 && parts[0].Length == 81)
                        KnownPuzzles.TryAdd(parts[0], 0);
                }
            }

            Counts[level] = count;

            // append:true eski CSV'leri korur; her yeni bulmaca dosyanın sonuna yazılır.
            Writers[level] = new StreamWriter(path, append: true, new UTF8Encoding(false));
        }
    }

    private static void WatchForStopKey()
    {
        while (!stopRequested)
        {
            try
            {
                if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
                    stopRequested = true;
            }
            catch
            {
                return;
            }

            Thread.Sleep(100);
        }
    }

    private static void WorkerLoop()
    {
        var classifier = new PuzzleClassifier(new SudokuSolver());
        var generator = new SudokuGenerator();

        while (!stopRequested && !AllLevelsFull())
        {
            Interlocked.Increment(ref totalBoards);

            foreach ((string puzzle, string solution) in generator.DigProgressively())
            {
                Interlocked.Increment(ref totalDigSteps);

                if (stopRequested)
                    return;

                HashSet<int> openLevels = GetOpenLevels();
                if (openLevels.Count == 0)
                    return;

                int? level = classifier.Assign(puzzle, openLevels);
                if (!level.HasValue || !KnownPuzzles.TryAdd(puzzle, 0))
                    continue;

                if (!TryClaim(level.Value))
                {
                    KnownPuzzles.TryRemove(puzzle, out _);
                    continue;
                }

                WriteResult(level.Value, puzzle, solution);
                Interlocked.Increment(ref totalWritten);
            }
        }
    }

    private static HashSet<int> GetOpenLevels()
    {
        return new HashSet<int>(Enumerable
            .Range(1, LevelCount)
            .Where(level => Volatile.Read(ref Counts[level]) < TargetPerLevel));
    }

    private static bool TryClaim(int level)
    {
        while (true)
        {
            int current = Volatile.Read(ref Counts[level]);
            if (current >= TargetPerLevel)
                return false;

            if (Interlocked.CompareExchange(ref Counts[level], current + 1, current) == current)
                return true;
        }
    }

    private static bool AllLevelsFull()
    {
        return Enumerable
            .Range(1, LevelCount)
            .All(level => Volatile.Read(ref Counts[level]) >= TargetPerLevel);
    }

    private static void WriteResult(int level, string puzzle, string solution)
    {
        string technique = TechniqueCatalog.TechniqueForLevel(level);
        lock (LevelLocks[level])
        {
            Writers[level].WriteLine($"{puzzle},{solution},0,{technique}");
            Writers[level].Flush();
        }
    }

    private static string GetLevelPath(int level)
    {
        return Path.Combine(OutputDir, $"level{level}.csv");
    }

    private static void PrintProgress(TimeSpan elapsed)
    {
        Console.WriteLine(
            $"{elapsed:hh\\:mm\\:ss}  Tahta:{Volatile.Read(ref totalBoards)}  " +
            $"Deneme:{Volatile.Read(ref totalDigSteps)}  Yeni:{Volatile.Read(ref totalWritten)}");
        Console.WriteLine(string.Join("  ", Enumerable.Range(1, LevelCount)
            .Select(level => $"L{level}:{Volatile.Read(ref Counts[level])}/{TargetPerLevel}")));
    }
}

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

    private const int TargetPerLevel = 3000;
    private const int LevelCount = 6;
    private const int MinimumEmptyCells = 40;
    private const int ProgressIntervalMs = 2000;

    private static readonly int[] Counts = new int[LevelCount + 1];
    private static readonly Dictionary<string, int>[] QuotaCounts =
        Enumerable.Range(0, LevelCount + 1)
            .Select(_ => new Dictionary<string, int>())
            .ToArray();
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
        var classifier = new PuzzleClassifier(new SudokuSolver());
        var uniquenessChecker = new SudokuGenerator();

        for (int level = 1; level <= LevelCount; level++)
        {
            string path = GetLevelPath(level);
            int removedCount = 0;
            var retainedLines = new List<string>();

            if (File.Exists(path))
            {
                foreach (string rawLine in File.ReadLines(path))
                {
                    if (string.IsNullOrWhiteSpace(rawLine))
                        continue;

                    string[] parts = rawLine.Split(',');
                    if (parts.Length < 2
                        || parts[0].Length != 81
                        || !uniquenessChecker.HasUniqueSolution(parts[0], parts[1])
                        || CountEmptyCells(parts[0]) < MinimumEmptyCells
                        || !classifier.MatchesLevel(parts[0], level))
                    {
                        removedCount++;
                        continue;
                    }

                    string technique = classifier.TechniqueForAcceptedPuzzle(parts[0], level);
                    string quotaKey = GetQuotaKey(level, parts[0], technique);
                    if (!KnownPuzzles.TryAdd(parts[0], 0))
                    {
                        removedCount++;
                        continue;
                    }

                    if (!TryClaim(level, quotaKey))
                    {
                        KnownPuzzles.TryRemove(parts[0], out _);
                        removedCount++;
                        continue;
                    }

                    retainedLines.Add(rawLine);
                }

                if (removedCount > 0)
                {
                    File.WriteAllLines(path, retainedLines, new UTF8Encoding(false));
                    Console.WriteLine(
                        $"L{level}: güncel kayıt şartlarını karşılamayan " +
                        $"{removedCount} eski kayıt kaldırıldı.");
                }
            }

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

                if (CountEmptyCells(puzzle) < MinimumEmptyCells)
                    continue;

                HashSet<int> openLevels = GetOpenLevels();
                if (openLevels.Count == 0)
                    return;

                int? level = classifier.Assign(puzzle, openLevels);
                if (!level.HasValue || !KnownPuzzles.TryAdd(puzzle, 0))
                    continue;

                string technique = classifier.TechniqueForAcceptedPuzzle(puzzle, level.Value);
                string quotaKey = GetQuotaKey(level.Value, puzzle, technique);
                if (!TryClaim(level.Value, quotaKey))
                {
                    KnownPuzzles.TryRemove(puzzle, out _);
                    continue;
                }

                WriteResult(level.Value, puzzle, solution, technique);
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

    private static bool TryClaim(int level, string quotaKey)
    {
        lock (LevelLocks[level])
        {
            int quotaTarget = GetQuotaTarget(level);
            QuotaCounts[level].TryGetValue(quotaKey, out int quotaCount);
            if (quotaCount >= quotaTarget || Counts[level] >= TargetPerLevel)
                return false;

            QuotaCounts[level][quotaKey] = quotaCount + 1;
            Counts[level]++;
            return true;
        }
    }

    private static int GetQuotaTarget(int level)
    {
        return level switch
        {
            <= 4 => 1500,
            5 => 250,
            6 => 100,
            _ => throw new ArgumentOutOfRangeException(nameof(level)),
        };
    }

    private static string GetQuotaKey(int level, string puzzle, string technique)
    {
        int emptyCells = CountEmptyCells(puzzle);
        return level <= 4
            ? emptyCells.ToString()
            : $"{emptyCells}:{technique}";
    }

    private static bool AllLevelsFull()
    {
        return Enumerable
            .Range(1, LevelCount)
            .All(level => Volatile.Read(ref Counts[level]) >= TargetPerLevel);
    }

    private static void WriteResult(
        int level,
        string puzzle,
        string solution,
        string technique)
    {
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

    private static int CountEmptyCells(string puzzle)
    {
        int count = 0;
        foreach (char value in puzzle)
            if (value == '0')
                count++;

        return count;
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

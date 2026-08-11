using System;
using System.Collections.Generic;
using System.Linq;

public static class TechniqueCatalog
{
    // Kolaydan zora tek çözüm sırası. PointingPairs uygulaması blok içinde
    // aynı satır/sütuna sıkışan hem iki hem de üç adayı kapsar.
    public static readonly (int Level, string Name)[] LevelTechniques =
    {
        (1, "NakedSingle"),
        (2, "HiddenSingle"),
        (3, "NakedPairs"),
        (4, "HiddenPairs"),
        (5, "PointingPairs"),
        (6, "BoxLineReduction"),
    };

    public static IReadOnlyList<string> RequiredForLevel(int level)
    {
        if (level < 1 || level > LevelTechniques.Length)
            throw new ArgumentOutOfRangeException(nameof(level));

        return LevelTechniques
            .Where(item => item.Level <= level)
            .Select(item => item.Name)
            .ToArray();
    }

    public static string TechniqueForLevel(int level)
    {
        if (level < 1 || level > LevelTechniques.Length)
            throw new ArgumentOutOfRangeException(nameof(level));

        return LevelTechniques[level - 1].Name;
    }
}

public static class TechniqueCatalog
{
    // Artan zorluk (tier) sırasında, tek doğruluk kaynağı.
    // SudokuSolver bu sırayla dener, Program.cs kova listesini buradan türetir.
    public static readonly (string Name, int Tier)[] All = new[]
    {
        ("NakedPairs", 2), ("HiddenPairs", 2), ("PointingPairs", 2), ("BoxLineReduction", 2),
        ("XWing", 3), ("NakedTriples", 3), ("HiddenTriples", 3),
        ("YWing", 4), ("Swordfish", 4), ("NakedQuads", 4), ("HiddenQuads", 4),
        ("Skyscraper", 5), ("XYZWing", 5), ("UniqueRectangle", 5),
        ("Jellyfish", 6), ("WWing", 6), ("XYChain", 6), ("SimpleColouring", 6), ("BUG", 6),
    };
}

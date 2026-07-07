using System.Collections.Generic;


    public class PuzzleClassifier
    {
        private readonly SudokuSolver solver;

        public PuzzleClassifier(SudokuSolver solver)
        {
            this.solver = solver;
        }

        // Tam cephanelikle "doğal tavan" (kaç boş kalırsa hangi tier gerekiyor) hesabı.
        // Çağıran, aynı tier'de kalınan ardışık adımlar arasında en derin (en çok boş
        // hücreli) olanı seçip sadece onu Assign()'a vermek için bunu her adımda çağırır.
        public SolveResult SolveNatural(string puzzle) => solver.Solve(puzzle);

        // Önceden hesaplanmış bir doğal sonucu, en fazla tek bir (level, technique)
        // kovasına atar; None dönerse (tavan tekniklerinin hepsi zaten dolu ya da
        // gereklilik doğrulanamadı) çağıran bu bulmacayı atlar.
        public (int Level, string Technique)? Assign(string puzzle, SolveResult natural, ISet<(int Level, string Technique)> openBuckets)
        {
            if (!natural.Solved) return null;

            int ceiling = natural.DifficultyLevel; // 1-6

            if (ceiling == 1)
            {
                var singlesKey = (1, "Singles");
                return openBuckets.Contains(singlesKey) ? singlesKey : ((int, string)?)null;
            }

            foreach (var (name, tier) in TechniqueCatalog.All)
            {
                if (tier != ceiling) continue;
                if (natural.Get(name) == 0) continue; // doğal izde bu teknik hiç ateşlenmedi

                var key = (ceiling, name);
                if (!openBuckets.Contains(key)) continue;

                if (solver.IsTechniqueRequired(puzzle, name, ceiling))
                    return key;
            }

            return null;
        }
    }

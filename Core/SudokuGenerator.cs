using System;
using System.Collections.Generic;
using System.Linq;


    public class SudokuGenerator
    {
        private readonly Random random;
        private const int AllDigitsMask = 0x1FF;

        // random verilmezse thread'e özgü, birbirinden bağımsız bir tohum kullanılır
        // (çoklu thread'de paylaşılan/varsayılan Random güvenli değildir).
        public SudokuGenerator(Random random = null)
        {
            this.random = random ?? new Random(Guid.NewGuid().GetHashCode());
        }

        // Tam dolu bir tahtadan başlayıp hücreleri (tekillik bozulmadıkça) teker
        // teker boşaltır. Her kabul edilen adımda bir öncekinden bir tık daha zor
        // olan (puzzle, solution) çiftini döndürür. Tek bir tam tahtadan onlarca
        // giderek zorlaşan durum elde edilir; bu da nadir tekniklere rastlama
        // olasılığını kör rastgele delik açmaya göre büyük ölçüde artırır.
        public IEnumerable<(string Puzzle, string Solution)> DigProgressively()
        {
            string fullSolution = GenerateFullBoard();
            string current = fullSolution;

            var positions = Enumerable.Range(0, 81).OrderBy(_ => random.Next()).ToList();

            foreach (int pos in positions)
            {
                if (current[pos] == '0') continue;

                string trial = current.Remove(pos, 1).Insert(pos, "0");
                if (HasAlternativeSolution(trial, fullSolution)) continue;

                current = trial;
                yield return (current, fullSolution);
            }
        }

        private string GenerateFullBoard()
        {
            int[] board = new int[81];
            FillBoard(board, 0);
            return string.Join("", board);
        }

        private bool FillBoard(int[] board, int index)
        {
            if (index == 81) return true;

            var digits = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 }.OrderBy(x => random.Next()).ToList();
            foreach (int digit in digits)
            {
                if (IsValidPlacement(board, index, digit))
                {
                    board[index] = digit;
                    if (FillBoard(board, index + 1)) return true;
                    board[index] = 0; // Geri al
                }
            }
            return false;
        }

        private bool IsValidPlacement(int[] board, int index, int digit)
        {
            int row = index / 9, col = index % 9, boxRow = (row / 3) * 3, boxCol = (col / 3) * 3;
            for (int i = 0; i < 9; i++)
            {
                if (board[row * 9 + i] == digit) return false;
                if (board[i * 9 + col] == digit) return false;
            }
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[(boxRow + r) * 9 + (boxCol + c)] == digit) return false;
            return true;
        }

        // --- TEK ÇÖZÜM KONTROLÜ (Bitwise Mantığınız) ---
        private bool HasAlternativeSolution(string puzzle, string givenSolution)
        {
            int[] board = new int[81], rowMask = new int[9], colMask = new int[9], boxMask = new int[9], expected = new int[81];
            for (int i = 0; i < 81; i++) expected[i] = givenSolution[i] - '0';
            for (int i = 0; i < 81; i++)
            {
                int val = puzzle[i] - '0';
                board[i] = val;
                if (val == 0) continue;
                int r = i / 9, c = i % 9, b = (r / 3) * 3 + (c / 3);
                int bit = 1 << (val - 1);
                rowMask[r] |= bit; colMask[c] |= bit; boxMask[b] |= bit;
            }
            return SearchAlternative(board, rowMask, colMask, boxMask, expected);
        }

        private bool SearchAlternative(int[] board, int[] rowMask, int[] colMask, int[] boxMask, int[] expected)
        {
            int bestIndex = -1, bestMask = 0, bestCount = 10;
            for (int i = 0; i < 81; i++)
            {
                if (board[i] != 0) continue;
                int r = i / 9, c = i % 9, b = (r / 3) * 3 + (c / 3);
                int candidates = AllDigitsMask & ~(rowMask[r] | colMask[c] | boxMask[b]);
                int count = BitCount(candidates);
                if (count == 0) return false;
                if (count < bestCount) { bestCount = count; bestMask = candidates; bestIndex = i; if (count == 1) break; }
            }
            if (bestIndex == -1)
            {
                for (int i = 0; i < 81; i++) if (board[i] != expected[i]) return true;
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

        private void Place(int[] board, int[] rowMask, int[] colMask, int[] boxMask, int index, int row, int col, int box, int digit, int bit)
        { board[index] = digit; rowMask[row] |= bit; colMask[col] |= bit; boxMask[box] |= bit; }
        private void Remove(int[] board, int[] rowMask, int[] colMask, int[] boxMask, int index, int row, int col, int box, int bit)
        { board[index] = 0; rowMask[row] &= ~bit; colMask[col] &= ~bit; boxMask[box] &= ~bit; }
        private int BitCount(int x) { int n = 0; while (x != 0) { x &= x - 1; n++; } return n; }
        private int BitToDigit(int bit) { int d = 1; while (bit > 1) { bit >>= 1; d++; } return d; }
    }

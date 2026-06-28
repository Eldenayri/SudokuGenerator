

 
    public class SudokuCell
    {
        private int value = 0;
        private bool isFixed = false;


        public void Setup(int row, int col, int value, bool isFixed)
        {
            this.value = value;
            this.isFixed = isFixed;
        }

        public int GetValue() => value;
        public void SetValue(int val) { value = val; }

        public bool IsFixed() => isFixed;
        public void SetFixed(bool val) { isFixed = val; }

    }

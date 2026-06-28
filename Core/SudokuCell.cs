// Unity bağımlılıklarından arındırılmış sade SudokuCell
public class SudokuCell
{
    private int value = 0;
    private bool isFixed = false;
    private bool isFix = false;

    public void Setup(int row, int col, int value, bool isFixed)
    {
        this.value = value;
        this.isFixed = isFixed;
        this.isFix = isFixed;
    }

    public int GetValue() => value;
    public void SetValue(int val) { value = val; }

    public bool IsFixed() => isFixed;
    public void SetFixed(bool val) { isFixed = val; }

    public bool IsFix() => isFix;
    public void SetFix(bool val) { isFix = val; }
}
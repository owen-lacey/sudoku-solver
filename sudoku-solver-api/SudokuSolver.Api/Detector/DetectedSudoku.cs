namespace SudokuSolver.Api.Detector;

public record DetectedSudoku()
{
    public float Score { get; set; }
    public float[] Box { get; set; }
}
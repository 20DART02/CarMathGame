namespace CarMathGame.Models
{
    public class MathProblem
    {
        public string Question { get; set; } = string.Empty;
        public int CorrectAnswer { get; set; }
        public int[] Options { get; set; } = Array.Empty<int>();
        public ProblemType Type { get; set; }
        public int Difficulty { get; set; }
    }

    public enum ProblemType
    {
        Addition,
        Subtraction,
        Multiplication,
        Division
    }
}
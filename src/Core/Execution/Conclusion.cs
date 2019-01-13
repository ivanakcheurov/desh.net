namespace Desh.Core.Execution
{
    public class Conclusion : EvaluationResult
    {
        public Conclusion(string[] decisions) => Decisions = decisions;

        public string[] Decisions { get; }
    }
}
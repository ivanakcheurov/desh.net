using Desh.Parsing;

namespace Desh.OutOfTheBox
{
    public class Context : IContext
    {
        public Context(string sourceDesh, IParseLogger logger, INameRecognizer variableRecognizer, INameRecognizer operatorRecognizer)
        {
            SourceDesh = sourceDesh;
            Logger = logger;
            VariableRecognizer = variableRecognizer;
            OperatorRecognizer = operatorRecognizer;
        }

        public string SourceDesh { get; }
        public IParseLogger Logger { get; }
        public INameRecognizer VariableRecognizer { get; }
        public INameRecognizer OperatorRecognizer { get; }
    }
}

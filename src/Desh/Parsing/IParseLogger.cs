namespace Desh.Parsing
{
    public interface IParseLogger
    {
        void Log(string node, string data, int line, int column);
        void LogStart(string node, string data, int line, int column);
        void LogEnd(string node, string data, int line, int column);
    }
}

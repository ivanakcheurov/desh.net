using System.Threading.Tasks;

namespace Desh.Execution
{
    public interface IVariableEvaluator
    {
        Task<string> Evaluate(string variable);
    }
}
using System.Threading;
using System.Threading.Tasks;

namespace MvvmTools.Controls
{
    public interface ISuggestionsProvider
    {
        Task<object[]> GetSuggestions(string filter, CancellationToken ct);
    }
}
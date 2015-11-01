using System.Threading;
using System.Threading.Tasks;

namespace MvvmTools.Core.Controls
{
    public interface ISuggestionsProvider
    {
        Task<object[]> GetSuggestions(string filter, CancellationToken ct);
    }
}
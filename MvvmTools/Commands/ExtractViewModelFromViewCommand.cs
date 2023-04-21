//------------------------------------------------------------------------------
// ReSharper disable HeapView.BoxingAllocation

using System.Threading.Tasks;
using Unity;

namespace MvvmTools.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ExtractViewModelFromViewCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractViewModelFromViewCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        public ExtractViewModelFromViewCommand(IUnityContainer container)
            : base(Constants.ExtractViewModelFromViewCommandId, container)
        {
        }
        
        protected override Task OnExecuteAsync()
        {
            return base.OnExecuteAsync();
        }
    }
}

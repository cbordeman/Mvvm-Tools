﻿//------------------------------------------------------------------------------

using System.Threading.Tasks;
using MvvmTools.Services;
using MvvmTools.ViewModels;
using Unity;

namespace MvvmTools.Commands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class ScaffoldViewAndViewModelCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractViewModelFromViewCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        public ScaffoldViewAndViewModelCommand(IUnityContainer container)
            : base(Constants.ScaffoldViewAndViewModelCommandId, container)
        {
            SolutionService = container.Resolve<ISolutionService>();
            DialogService = container.Resolve<DialogService>();
        }

        public ISolutionService SolutionService { get; set; }
        public DialogService DialogService { get; set; }

        protected override async Task OnExecuteAsync()
        {
            await base.OnExecuteAsync().ConfigureAwait(false);

            var vm = Container.Resolve<ScaffoldDialogViewModel>();
            await vm.Init();
            DialogService.ShowDialog(vm);
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="ExtractViewModelFromViewCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using MvvmTools.Core.Services;
using MvvmTools.Core.ViewModels;
using MvvmTools.Core.Views;
using Ninject;

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
        public ScaffoldViewAndViewModelCommand()
            : base(Constants.ScaffoldViewAndViewModelCommandId)
        {
        }

        [Inject]
        public ISolutionService SolutionService { get; set; }

        [Inject]
        public DialogService DialogService { get; set; }

        protected async override void OnExecute()
        {
            base.OnExecute();

            var vm = Kernel.Get<ScaffoldDialogViewModel>();
            await vm.Init();
            DialogService.ShowDialog(vm);
        }
    }
}

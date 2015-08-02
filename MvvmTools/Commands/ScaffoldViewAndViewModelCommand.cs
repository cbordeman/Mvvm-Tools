//------------------------------------------------------------------------------
// <copyright file="ExtractViewModelFromViewCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

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

        protected override void OnExecute()
        {
            base.OnExecute();
            
            //var projectModel = SolutionService.GetFullProjectModel(SolutionService.GetProject(viewModelLocationOptions.ProjectIdentifier));

        }
    }
}

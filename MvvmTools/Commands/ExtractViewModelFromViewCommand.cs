//------------------------------------------------------------------------------
// <copyright file="ExtractViewModelFromViewCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// ReSharper disable HeapView.BoxingAllocation

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
        public ExtractViewModelFromViewCommand()
            : base(Constants.ExtractViewModelFromViewCommandId)
        {
        }
        
        protected override void OnExecute()
        {
            base.OnExecute();

            
        }
    }
}

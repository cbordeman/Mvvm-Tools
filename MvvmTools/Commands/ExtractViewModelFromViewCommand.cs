//------------------------------------------------------------------------------
// <copyright file="ExtractViewModelFromViewCommand.cs" company="Chris Bordeman">
//     Copyright (c) 2015 Chris Bordeman.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

// ReSharper disable HeapView.BoxingAllocation

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
        
        protected override void OnExecute()
        {
            base.OnExecute();

            
        }
    }
}

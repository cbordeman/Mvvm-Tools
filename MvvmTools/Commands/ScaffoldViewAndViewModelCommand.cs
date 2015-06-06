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
    internal sealed class ScaffoldViewAndViewModelCommand : BaseCommand
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractViewModelFromViewCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        internal ScaffoldViewAndViewModelCommand(MvvmToolsPackage package)
            : base(package, Constants.ScaffoldViewAndViewModelCommandId)
        {
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ExtractViewModelFromViewCommand Instance { get; private set; }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(MvvmToolsPackage package)
        {
            Instance = new ExtractViewModelFromViewCommand(package);
        }

        protected override void OnExecute()
        {
            base.OnExecute();

        }
    }
}

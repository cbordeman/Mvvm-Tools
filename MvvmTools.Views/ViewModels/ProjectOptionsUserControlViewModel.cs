using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MvvmTools.Core.Models;
using MvvmTools.Core.Services;
using MvvmTools.Core.Utilities;
using Ninject;

// ReSharper disable ConvertIfStatementToReturnStatement

namespace MvvmTools.Core.ViewModels
{
    public class ProjectOptionsUserControlViewModel : BaseViewModel, IDataErrorInfo
    {
        #region Data

        private static readonly Regex SuffixRegex = new Regex(@"^[_a-zA-Z0-9]*$");
        
        #endregion Data

        #region Ctor and Init

        #endregion Ctor and Init

        #region Properties

        [Inject]
        public IDialogService DialogService { get; set; }

        public ProjectModel ProjectModel { get; private set; }
        public bool IsProject { get; set; }

        #region ResetButtonText
        private string _resetButtonText;
        public string ResetButtonText
        {
            get { return _resetButtonText; }
            set { SetProperty(ref _resetButtonText, value); }
        }
        #endregion ResetButtonText

        #region IsInherited
        public bool IsInherited
        {
            get
            {
                if (InheritedProjectOptionsViewModel == null)
                    return false;

                if (ViewModelSuffix != InheritedProjectOptionsViewModel.ViewModelSuffix)
                    return false;

                if (!LocationDescriptorForViewModel.IsInherited)
                    return false;
                if (!LocationDescriptorForView.IsInherited)
                    return false;

                return true;
            }
        }
        #endregion IsInherited

        #region ViewModelSuffix

        private string _viewModelSuffix;

        public string ViewModelSuffix
        {
            get { return _viewModelSuffix; }
            set
            {
                if (SetProperty(ref _viewModelSuffix, value))
                {
                    ResetToInheritedCommand.RaiseCanExecuteChanged();
                    ResetViewModelSuffixCommand.RaiseCanExecuteChanged();
                }
            }
        }

        #endregion ViewModelSuffix

        public LocationDescriptorUserControlViewModel LocationDescriptorForViewModel { get; private set; }
        public LocationDescriptorUserControlViewModel LocationDescriptorForView { get; private set; }
        public ProjectOptionsUserControlViewModel InheritedProjectOptionsViewModel { get; private set; }

        #endregion Properties

        #region Commands
        
        #region ResetToInheritedCommand
        DelegateCommand _resetToInheritedCommand;
        public DelegateCommand ResetToInheritedCommand => _resetToInheritedCommand ?? (_resetToInheritedCommand = new DelegateCommand(ExecuteResetToInheritedCommand, CanResetToInheritedCommand));
        public bool CanResetToInheritedCommand() => !IsInherited;
        public void ExecuteResetToInheritedCommand()
        {
            // Reset to inherited values.
            ResetToInherited();
        }

        #endregion

        #region ResetViewModelSuffixCommand
        private DelegateCommand _resetViewModelSuffixCommand;
        public DelegateCommand ResetViewModelSuffixCommand => _resetViewModelSuffixCommand ?? (_resetViewModelSuffixCommand = new DelegateCommand(ExecuteResetViewModelSuffixCommand, CanResetViewModelSuffixCommand));
        public bool CanResetViewModelSuffixCommand() => ViewModelSuffix != InheritedProjectOptionsViewModel?.ViewModelSuffix;
        public void ExecuteResetViewModelSuffixCommand()
        {
            ViewModelSuffix = InheritedProjectOptionsViewModel.ViewModelSuffix;
            ResetToInheritedCommand.RaiseCanExecuteChanged();
        }
        #endregion
        
        #endregion Commands

        #region Private Methods


        #endregion Private Methods

        #region IDataErrorInfo

        public string this[string columnName]
        {
            get
            {
                if (columnName == "ViewModelSuffix")
                {
                    if (string.IsNullOrWhiteSpace(ViewModelSuffix))
                        return "Empty.";

                    if (!SuffixRegex.IsMatch(ViewModelSuffix))
                        return "Invalid.";

                    return null;
                }
                return null;
            }
        }

        public string Error => this["ViewModelSuffix"];

        #endregion IDataErrorInfo

        #region Public Methods

        public async Task Initialize(ProjectOptions projectOptions,
            bool isProject,
            ProjectOptionsUserControlViewModel inherited,
            string viewModelSuffix,
            LocationDescriptorUserControlViewModel locationDescriptorForViewModel,
            LocationDescriptorUserControlViewModel locationDescriptorForView)
        {
            IsProject = isProject;
            ResetButtonText = isProject ? "Reset Project to Solution Defaults" : "Reset Solution to Defaults";

            ProjectModel = projectOptions.ProjectModel;
            InheritedProjectOptionsViewModel = inherited;

            LocationDescriptorForViewModel = locationDescriptorForViewModel;
            LocationDescriptorForViewModel.Inherited = inherited?.LocationDescriptorForViewModel;
            await LocationDescriptorForViewModel.InitializeFromSolution();
            locationDescriptorForViewModel.PropertyChanged += LocationDescriptorOnPropertyChanged;

            LocationDescriptorForView = locationDescriptorForView;
            LocationDescriptorForView.Inherited = inherited?.LocationDescriptorForView;
            await LocationDescriptorForView.InitializeFromSolution();
            locationDescriptorForView.PropertyChanged += LocationDescriptorOnPropertyChanged;

            // Now set initial property values.
            ViewModelSuffix = viewModelSuffix;
            LocationDescriptorForViewModel.SetFromDescriptor(projectOptions.ViewModelLocation);
            LocationDescriptorForView.SetFromDescriptor(projectOptions.ViewLocation);

            ResetToInheritedCommand.RaiseCanExecuteChanged();
        }

        public void LocationDescriptorOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            ResetToInheritedCommand.RaiseCanExecuteChanged();
        }
        
        public void ResetToInherited()
        {
            ViewModelSuffix = InheritedProjectOptionsViewModel.ViewModelSuffix;
            LocationDescriptorForViewModel.ResetToInherited();
            LocationDescriptorForView.ResetToInherited();
        }

        #endregion Public Methods
    }
}

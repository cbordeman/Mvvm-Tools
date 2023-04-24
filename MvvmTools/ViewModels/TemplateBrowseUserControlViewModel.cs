using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MvvmTools.Models;
using MvvmTools.Services;
using MvvmTools.Utilities;
using Unity;

namespace MvvmTools.ViewModels
{

    public class TemplateBrowseUserControlViewModel : BaseViewModel
    {
        #region Data

        //private readonly ISettingsService _settingsService;
        private readonly ITemplateService _templateService;

        private ObservableCollection<TemplateDialogViewModel> _templatesSource;

        private string _localTemplateFolder;

        #endregion Data

        #region Ctor and Init

        public TemplateBrowseUserControlViewModel(ITemplateService templateService, 
            IUnityContainer container,
            IDialogService dialogService,
            ISettingsService settingsSvc) : base(container)
        {
            DialogService = dialogService;
            SettingsSvc = settingsSvc;
            _templateService = templateService;
        }

        public void Init(string localTemplateFolder)
        {
            _localTemplateFolder = localTemplateFolder;

            // Set up platforms filter combo.
            Platforms = new List<ValueDescriptor<Platform?>>();
            Platforms.Add(new ValueDescriptor<Platform?>(null, "All Platforms"));
            foreach (var p in Enum.GetValues(typeof(Platform)).Cast<Platform>().OrderBy(p => p.ToString().ToLower()))
                Platforms.Add(new ValueDescriptor<Platform?>(p, p.ToString()));

            // Set up form factors filter combo.
            FormFactors = new List<ValueDescriptor<FormFactor?>>();
            FormFactors.Add(new ValueDescriptor<FormFactor?>(null, "All Form Factors"));
            foreach (var ff in Enum.GetValues(typeof(FormFactor)).Cast<FormFactor>().OrderBy(ff => ff.ToString().ToLower()))
                FormFactors.Add(new ValueDescriptor<FormFactor?>(ff, ff.ToString()));

            // The framework filter is setup in RefreshTemplates() because it's not based on an enum.

            RefreshTemplates();
        }


        #endregion Ctor and Init

        #region Properties

        public IDialogService DialogService { get; set; }
        public ISettingsService SettingsSvc { get; set; }

        #region SearchText
        private string _searchText;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    Templates?.Refresh();
                }
            }
        }
        #endregion SearchText

        #region Templates
        private ListCollectionView _templates;
        public ListCollectionView Templates
        {
            get { return _templates; }
            set { SetProperty(ref _templates, value); }
        }
        #endregion Templates

        #region FormFactors
        private List<ValueDescriptor<FormFactor?>> _formFactors;
        public List<ValueDescriptor<FormFactor?>> FormFactors
        {
            get { return _formFactors; }
            set { SetProperty(ref _formFactors, value); }
        }
        #endregion FormFactors

        #region SelectedFormFactor
        private FormFactor? _selectedFormFactor;
        public FormFactor? SelectedFormFactor
        {
            get { return _selectedFormFactor; }
            set
            {
                if (SetProperty(ref _selectedFormFactor, value))
                    Templates?.Refresh();
            }
        }
        #endregion SelectedFormFactor

        #region Platforms
        private List<ValueDescriptor<Platform?>> _platforms;
        public List<ValueDescriptor<Platform?>> Platforms
        {
            get { return _platforms; }
            set { SetProperty(ref _platforms, value); }
        }
        #endregion Platforms

        #region SelectedPlatform
        private Platform? _selectedPlatform;
        public Platform? SelectedPlatform
        {
            get { return _selectedPlatform; }
            set
            {
                if (SetProperty(ref _selectedPlatform, value))
                    Templates?.Refresh();
            }
        }
        #endregion SelectedPlatform

        #region Frameworks
        private List<ValueDescriptor<string>> _frameworks;
        public List<ValueDescriptor<string>> Frameworks
        {
            get { return _frameworks; }
            set { SetProperty(ref _frameworks, value); }
        }
        #endregion Frameworks

        #region SelectedFramework
        private string _selectedFramework;
        public string SelectedFramework
        {
            get { return _selectedFramework; }
            set
            {
                if (SetProperty(ref _selectedFramework, value))
                    Templates?.Refresh();
            }
        }
        #endregion SelectedFramework

        #region TemplateEditButtonContent
        private string _templateEditButtonContent = "Edit";
        public string TemplateEditButtonContent
        {
            get { return _templateEditButtonContent; }
            set { SetProperty(ref _templateEditButtonContent, value); }
        }
        #endregion TemplateEditButtonContent

        #endregion Properties

        #region Commands
        
        #region SearchCommand
        DelegateCommand _searchCommand;
        public DelegateCommand SearchCommand => _searchCommand ?? (_searchCommand = new DelegateCommand(ExecuteSearchCommand, CanSearchCommand));
        public bool CanSearchCommand() => !string.IsNullOrWhiteSpace(SearchText);
        public void ExecuteSearchCommand()
        {
            Templates?.Refresh();
        }
        #endregion

        #region AddTemplateCommand
        DelegateCommand _addTemplateCommand;
        public DelegateCommand AddTemplateCommand => _addTemplateCommand ?? (_addTemplateCommand = new DelegateCommand(ExecuteAddTemplateCommand, CanAddTemplateCommand));
        public bool CanAddTemplateCommand() => true;
        public void ExecuteAddTemplateCommand()
        {
            try
            {
                var vm = TemplateDialogViewModel.CreateFrom(Container);
                vm.Add(_templatesSource.Select(t => t.Name));

                if (DialogService.ShowDialog(vm))
                {
                    _templatesSource.Add(vm);
                    Templates.MoveCurrentTo(vm);

                    _templateService.SaveTemplates(_localTemplateFolder, _templatesSource.Select(t => new Template(t)));
                    RefreshFrameworksFilter();
                }
            }
            catch
            {
                // ignored
            }
        }
        #endregion

        #region EditTemplateCommand
        DelegateCommand _editTemplateCommand;
        public DelegateCommand EditTemplateCommand => _editTemplateCommand ?? (_editTemplateCommand = new DelegateCommand(ExecuteEditTemplateCommand, CanEditTemplateCommand));
        public bool CanEditTemplateCommand() => Templates?.CurrentItem != null;
        public void ExecuteEditTemplateCommand()
        {
            try
            {
                var vm = (TemplateDialogViewModel)Templates.CurrentItem;

                // Edit a copy so we don't have live updates in our list.
                var copyVm = TemplateDialogViewModel.CreateFrom(Container, vm);

                copyVm.Edit(_templatesSource.Select(t => t.Name));
                if (DialogService.ShowDialog(copyVm))
                {
                    // Success, copy fields back into our instance, save, and refresh frameworks (filter combobox).
                    vm.CopyFrom(copyVm);
                    _templateService.SaveTemplates(_localTemplateFolder, _templatesSource.Select(t => new Template(t)));
                    RefreshFrameworksFilter();
                }
            }
            catch
            {
                // ignored
            }
        }
        #endregion

        #region DeleteTemplateCommand
        DelegateCommand _deleteTemplateCommand;
        public DelegateCommand DeleteTemplateCommand => _deleteTemplateCommand ?? (_deleteTemplateCommand = new DelegateCommand(ExecuteDeleteTemplateCommand, CanDeleteTemplateCommand));
        public bool CanDeleteTemplateCommand() => Templates?.CurrentItem != null && !((TemplateDialogViewModel)Templates.CurrentItem).IsInternal;
        public async void ExecuteDeleteTemplateCommand()
        {
            try
            {
                var t = (TemplateDialogViewModel)Templates.CurrentItem;
                if ((await DialogService.Ask("Delete Template?", $"Delete template \"{t.Name}?\"", AskButton.OKCancel)) ==
                    AskResult.OK)
                    if ((await DialogService.Ask("Are you sure?", $"Are you sure you want to DELETE template \"{t.Name}?\"",
                                AskButton.OKCancel)) == AskResult.OK)
                    {
                        Templates.Remove(Templates.CurrentItem);

                        _templateService.SaveTemplates(_localTemplateFolder, _templatesSource.Select(t2 => new Template(t2)));

                        RefreshFrameworksFilter();
                    }
            }
            catch (Exception)
            {
                // ignored
            }
        }
        #endregion

        #region CopyTemplateCommand
        DelegateCommand _copyTemplateCommand;
        public DelegateCommand CopyTemplateCommand => _copyTemplateCommand ?? (_copyTemplateCommand = new DelegateCommand(ExecuteCopyTemplateCommand, CanCopyTemplateCommand));
        public bool CanCopyTemplateCommand() => Templates?.CurrentItem != null;
        public async void ExecuteCopyTemplateCommand()
        {
            try
            {
                var t = (TemplateDialogViewModel)Templates.CurrentItem;
                if ((await DialogService.Ask("Copy Template?", $"Copy template \"{t.Name}?\"", AskButton.OKCancel)) ==
                    AskResult.OK)
                {
                    var vm = Container.Resolve<TemplateDialogViewModel>();
                    vm.CopyFrom(t);
                    vm.IsInternal = false;
                    vm.Add(_templatesSource.Select(t2 => t2.Name));
                    if (DialogService.ShowDialog(vm))
                    {
                        _templatesSource.Add(vm);
                        Templates.MoveCurrentTo(vm);

                        _templateService.SaveTemplates(_localTemplateFolder, _templatesSource.Select(t3 => new Template(t3)));

                        RefreshFrameworksFilter();
                    }
                }
            }
            catch
            {
                // ignored
            }
        }
        #endregion
        
        #region RefreshTemplatesCommand
        DelegateCommand _refreshTemplatesCommand;
        public DelegateCommand RefreshTemplatesCommand => _refreshTemplatesCommand ?? (_refreshTemplatesCommand = new DelegateCommand(ExecuteRefreshTemplatesCommand, CanRefreshTemplatesCommand));
        public bool CanRefreshTemplatesCommand() => true;
        public void ExecuteRefreshTemplatesCommand()
        {
            RefreshTemplates();
        }
        #endregion
        
        #endregion Commands

        #region Virtuals



        #endregion Virtuals

        #region Public Methods

        public void ChangeLocalTemplatesFolder(string localTemplateFolder)
        {
            _localTemplateFolder = localTemplateFolder;
            RefreshTemplates();
        }

        #endregion Public Methods

        #region Private Helpers

        private void RefreshTemplates()
        {
            try
            {
                var templates = _templateService.LoadTemplates(_localTemplateFolder);

                bool restore = Templates != null;
                int savedPos = 0;
                if (restore)
                {
                    Templates.CurrentChanged -= TemplatesOnCurrentChanged;
                    Templates.Filter = null;

                    // Save state of ListCollectionView to restore later.
                    savedPos = Templates.CurrentPosition;
                }

                _templatesSource = new ObservableCollection<TemplateDialogViewModel>(templates.Select(t => TemplateDialogViewModel.CreateFrom(Container, t)));
                Templates = new ListCollectionView(_templatesSource) { Filter = TemplateFilter };
                Templates.CurrentChanged += TemplatesOnCurrentChanged;

                if (restore)
                {
                    if (Templates.Count > savedPos)
                        Templates.MoveCurrentToPosition(savedPos);
                }
                else if (Templates.Count > 0)
                    Templates.MoveCurrentToPosition(0);

                RefreshFrameworksFilter();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void RefreshFrameworksFilter()
        {
            // Set up form frameworks combo.
            var tmp = new List<ValueDescriptor<string>>();
            tmp.Add(new ValueDescriptor<string>(null, "All Frameworks"));
            var distinctFrameworks = _templatesSource.Distinct(new TemplateDialogViewModelFrameworkComparer()).OrderBy(t => t.Framework.ToLower()).ToList();
            foreach (var t in distinctFrameworks)
                tmp.Add(new ValueDescriptor<string>(t.Framework, string.IsNullOrWhiteSpace(t.Framework) ? "(no framework)" : t.Framework));
            var savedSelectedFramework = SelectedFramework;
            Frameworks = tmp;
            SelectedFramework = savedSelectedFramework;
            NotifyPropertyChanged(nameof(SelectedFramework));
            Templates.Refresh();

            if (Templates != null && _templatesSource != null && SelectedFramework != null)
            {
                if (!distinctFrameworks.Select(t => t.Framework).Contains(SelectedFramework))
                    SelectedFramework = null;
            }
        }

        private void TemplatesOnCurrentChanged(object sender, EventArgs eventArgs)
        {
            EditTemplateCommand.RaiseCanExecuteChanged();
            DeleteTemplateCommand.RaiseCanExecuteChanged();
            CopyTemplateCommand.RaiseCanExecuteChanged();

            if (Templates.CurrentItem != null && ((TemplateDialogViewModel)Templates.CurrentItem).IsInternal)
                TemplateEditButtonContent = "View";
            else
                TemplateEditButtonContent = "Edit";
        }

        private bool TemplateFilter(object template)
        {
            // If nothing is checked on Platforms or Form Factors, that's assumed 
            // to mean all, so that filter is not applied.

            var t = (TemplateDialogViewModel)template;
            if (SelectedPlatform != null && t.Platforms.CheckedItems.Count != 0)
                if (!t.Platforms.CheckedItems.Contains(SelectedPlatform.Value))
                    return false;
            if (SelectedFormFactor != null && t.FormFactors.CheckedItems.Count != 0)
                if (!t.FormFactors.CheckedItems.Contains(SelectedFormFactor.Value))
                    return false;
            if (SelectedFramework != null)
                if (!string.Equals(t.Framework, SelectedFramework, StringComparison.OrdinalIgnoreCase))
                    return false;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                // Easy - contains whole search term.
                if (t.Name.Contains(SearchText.Trim()))
                    return true;
                if (t.Description.Contains(SearchText.Trim()))
                    return true;
                if (t.Tags.Contains(SearchText.Trim()))
                    return true;

                // Harder - split search term into words and search for each
                // one individually.  If two or more are found, include in result.
                int found = 0;
                var searchTerms = SearchText.Split(' ');
                foreach (var w in searchTerms)
                {
                    if (string.IsNullOrWhiteSpace(w))
                        continue;

                    if (t.Name.Contains(w))
                    {
                        found++;
                        if (found == 2)
                            return true;
                    }
                    if (t.Description.Contains(w))
                    {
                        found++;
                        if (found == 2)
                            return true;
                    }
                    if (t.Tags.Contains(w))
                    {
                        found++;
                        if (found == 2)
                            return true;
                    }
                }

                return false;
            }

            return true;
        }

        #endregion Private Helpers
    }
}
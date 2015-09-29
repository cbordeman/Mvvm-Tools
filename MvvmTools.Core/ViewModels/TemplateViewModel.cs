using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MvvmTools.Core.Models;

namespace MvvmTools.Core.ViewModels
{
    public class TemplateViewModel : BaseViewModel
    {
        public TemplateViewModel(Template template)
        {
            IsInternal = template.IsInternal;
            Platforms = new ListCollectionView(new ObservableCollection<Platform>(template.Platforms.OrderBy(p => p.ToString().ToLower())));
            FormFactors = new ListCollectionView(new ObservableCollection<FormFactor>(template.FormFactors.OrderBy(ff => ff.ToString().ToLower())));
            Framework = template.Framework;
            Name = template.Name;
            Description = template.Description;

            // Deep copy fields.
            Fields = new ListCollectionView(new ObservableCollection<FieldViewModel>(template.Fields.Select(f => new FieldViewModel(f))));

            View = template.View;

            ViewModelCSharp = template.ViewModelCSharp;
            CodeBehindCSharp = template.CodeBehindCSharp;

            ViewModelVisualBasic = template.ViewModelVisualBasic;
            CodeBehindVisualBasic = template.CodeBehindVisualBasic;
        }

        public TemplateViewModel(TemplateViewModel template)
        {
            IsInternal = template.IsInternal;
            Platforms = new ListCollectionView(new ObservableCollection<Platform>((ObservableCollection<Platform>)template.Platforms.SourceCollection));
            FormFactors = new ListCollectionView(new ObservableCollection<FormFactor>((ObservableCollection<FormFactor>)template.FormFactors.SourceCollection));
            Framework = template.Framework;
            Name = template.Name;
            Description = template.Description;

            // Deep copy fields.
            Fields = new ListCollectionView(new ObservableCollection<FieldViewModel>((ObservableCollection<FieldViewModel>)template.Fields.SourceCollection));

            View = template.View;

            ViewModelCSharp = template.ViewModelCSharp;
            CodeBehindCSharp = template.CodeBehindCSharp;

            ViewModelVisualBasic = template.ViewModelVisualBasic;
            CodeBehindVisualBasic = template.CodeBehindVisualBasic;
        }

        public bool IsInternal { get; set; }

        #region Platforms
        private ListCollectionView _platforms;
        public ListCollectionView Platforms
        {
            get { return _platforms; }
            set { SetProperty(ref _platforms, value); }
        }
        #endregion Platforms

        #region FormFactors
        private ListCollectionView _formFactors;
        public ListCollectionView FormFactors
        {
            get { return _formFactors; }
            set { SetProperty(ref _formFactors, value); }
        }
        #endregion FormFactors

        #region Framework
        private string _framework;
        public string Framework
        {
            get { return _framework; }
            set { SetProperty(ref _framework, value); }
        }
        #endregion Framework
        
        #region Name
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Tags
        private string _tags;
        public string Tags
        {
            get { return _tags; }
            set { SetProperty(ref _tags, value); }
        }
        #endregion Tags

        #region Description
        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        #endregion Description

        #region Fields
        private ListCollectionView _fields;
        public ListCollectionView Fields
        {
            get { return _fields; }
            set { SetProperty(ref _fields, value); }
        }
        #endregion Fields

        #region View
        private string _view;
        public string View
        {
            get { return _view; }
            set { SetProperty(ref _view, value); }
        }
        #endregion View

        #region ViewModelCSharp
        private string _viewModelCSharp;
        public string ViewModelCSharp
        {
            get { return _viewModelCSharp; }
            set { SetProperty(ref _viewModelCSharp, value); }
        }
        #endregion ViewModelCSharp

        #region ViewModelVisualBasic
        private string _viewModelVisualBasic;
        public string ViewModelVisualBasic
        {
            get { return _viewModelVisualBasic; }
            set { SetProperty(ref _viewModelVisualBasic, value); }
        }
        #endregion ViewModelVisualBasic
        
        #region CodeBehindCSharp
        private string _codeBehindCSharp;
        public string CodeBehindCSharp
        {
            get { return _codeBehindCSharp; }
            set { SetProperty(ref _codeBehindCSharp, value); }
        }
        #endregion CodeBehindCSharp

        #region CodeBehindVisualBasic
        private string _codeBehindVisualBasic;
        public string CodeBehindVisualBasic
        {
            get { return _codeBehindVisualBasic; }
            set { SetProperty(ref _codeBehindVisualBasic, value); }
        }
        #endregion CodeBehindVisualBasic
    }

    public class FieldViewModel : BaseViewModel
    {
        #region Data

        private ObservableCollection<StringViewModel> _choicesSource;

        #endregion Data

        #region Ctor and Init

        public FieldViewModel(Field field)
        {
            this._name = field.Name;
            this._default = field.Default;
            this._prompt = field.Prompt;
            this._description = field.Description;

            this._choicesSource = new ObservableCollection<StringViewModel>(field.Choices.Select(s => new StringViewModel(s)));
            this._choices = new ListCollectionView(_choicesSource);

            this._fieldType = field.FieldType;
        }

        public FieldViewModel(FieldViewModel field)
        {
            this._name = field.Name;
            this._default = field.Default;
            this._prompt = field.Prompt;
            this._description = field.Description;

            this._choicesSource = new ObservableCollection<StringViewModel>(field._choicesSource);
            this._choices = new ListCollectionView(_choicesSource);

            this._fieldType = field.FieldType;
        }

        #endregion Ctor and Init

        #region Name
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Default
        private string _default;
        public string Default
        {
            get { return _default; }
            set { SetProperty(ref _default, value); }
        }
        #endregion Default

        #region Prompt
        private string _prompt;
        public string Prompt
        {
            get { return _prompt; }
            set { SetProperty(ref _prompt, value); }
        }
        #endregion Prompt

        #region Description
        private string _description;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }
        #endregion Description

        #region Choices
        private ListCollectionView _choices;
        public ListCollectionView Choices
        {
            get { return _choices; }
            set { SetProperty(ref _choices, value); }
        }
        #endregion Choices
        
        #region FieldType
        private FieldType _fieldType;
        public FieldType FieldType
        {
            get { return _fieldType; }
            set { SetProperty(ref _fieldType, value); }
        }
        #endregion FieldType
    }
}

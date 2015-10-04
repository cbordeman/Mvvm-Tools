using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MvvmTools.Core.Models;

namespace MvvmTools.Core.ViewModels
{
    public class FieldDialogViewModel : BaseDialogViewModel
    {
        #region Data

        private ObservableCollection<StringViewModel> _choicesSource;

        #endregion Data

        #region Ctor and Init

        public FieldDialogViewModel() { }

        public FieldDialogViewModel(Field field)
        {
            this._name = field.Name;
            this._default = field.Default;
            this._prompt = field.Prompt;
            this._description = field.Description;

            this._choicesSource = new ObservableCollection<StringViewModel>(field.Choices.Select(s => new StringViewModel(s)));
            this._choices = new ListCollectionView(_choicesSource);

            this._fieldType = field.FieldType;
        }

        public FieldDialogViewModel(FieldDialogViewModel field)
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
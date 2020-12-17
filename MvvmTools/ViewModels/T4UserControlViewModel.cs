using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Threading;
using MvvmTools.Extensions;
using MvvmTools.Services;
using Unity;

namespace MvvmTools.ViewModels
{
    public class T4UserControlViewModel : BaseViewModel
    {
        #region Data

        private string _initialBuffer;
        private readonly DispatcherTimer _transformTimer;
        
        #endregion Data

        #region Ctor and Init

        public T4UserControlViewModel(IUnityContainer container,
            ITemplateService templateService) : base(container)
        {
            TemplateService = templateService;
            // Transforms only happen at most every interval.
            _transformTimer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(600)};
            _transformTimer.Tick += TransformTimerOnTick;
        }

        private void TransformTimerOnTick(object sender, EventArgs eventArgs)
        {
            _transformTimer.Stop();
            Transform();
        }

        #endregion Ctor and Init

        #region Properties

        public string HeaderFirstPart
        {
            get
            {
                var sb = new StringBuilder();

                sb.AppendLine("<#@ template debug=\"false\" hostspecific=\"false\" language=\"C#\" #>");
                
                sb.AppendLine("<#@ assembly name=\"System.Core\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Data\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Data.DataSetExtensions\" #>");
                sb.AppendLine("<#@ assembly name=\"System.IO\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Net.Http\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Runtime\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Text.Encoding\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Threading.Tasks\" #>");
                sb.AppendLine("<#@ assembly name=\"Microsoft.CSharp\" #>");
                sb.AppendLine("<#@ assembly name=\"Microsoft.CodeAnalysis.dll\" #>");
                sb.AppendLine("<#@ assembly name=\"Microsoft.CodeAnalysis.CSharp.dll\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Collections.Immutable.dll\" #>");
                sb.AppendLine("<#@ assembly name=\"PresentationCore\" #>");
                sb.AppendLine("<#@ assembly name=\"PresentationFramework\" #>");
                sb.AppendLine("<#@ assembly name=\"WindowsBase\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Xaml\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Xml\" #>");
                sb.AppendLine("<#@ assembly name=\"System.Xml.Linq\" #>");
                sb.AppendLine("<#@ assembly name=\"MvvmTools.Core.dll\" #>");

                sb.AppendLine("<#@ import namespace=\"System.Linq\" #>");
                sb.AppendLine("<#@ import namespace=\"System.Text\" #>");
                sb.AppendLine("<#@ import namespace=\"System.Collections.Generic\" #>");
                sb.AppendLine("<#@ import namespace=\"System.IO\" #>");
                sb.AppendLine("<#@ import namespace=\"Microsoft.CodeAnalysis\" #>");
                sb.AppendLine("<#@ import namespace=\"Microsoft.CodeAnalysis.CSharp\" #>");
                sb.AppendLine("<#@ import namespace=\"Microsoft.CodeAnalysis.CSharp.Syntax\" #>");
                sb.AppendLine("<#@ import namespace=\"MvvmTools.Core\" #>");
                sb.AppendLine("<#@ import namespace=\"MvvmTools.Core.Extensions\" #>");

                return sb.ToString().TrimEnd();
            }
        }

        public string Header
        {
            get
            {
                var sb = new StringBuilder(HeaderFirstPart, HeaderFirstPart.Length + 1000);
                
                foreach (var f in PredefinedFields)
                    sb.AppendLine($"<#@ parameter name=\"{f.Name}\" type=\"{f.Type}\" #>");
                foreach (var f in CustomFields)
                    sb.AppendLine($"<#@ parameter name=\"{f.Name}\" type=\"{f.Type}\" #>");

                return sb.ToString().TrimEnd();
            }
        }
        
        public ITemplateService TemplateService { get; set; }
        
        #region ShowErrors
        private bool _showErrors;
        public bool ShowErrors
        {
            get { return _showErrors; }
            set { SetProperty(ref _showErrors, value); }
        }
        #endregion ShowErrors

        #region Name
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }
        #endregion Name

        #region Buffer
        private string _buffer;
        public string Buffer
        {
            get { return _buffer; }
            set
            {
                if (SetProperty(ref _buffer, value))
                {
                    NotifyPropertyChanged(nameof(IsModified));

                    _transformTimer.Stop();
                    _transformTimer.Start();
                }
            }
        }

        #endregion Buffer

        #region Preview
        private string _preview;
        public string Preview
        {
            get { return _preview; }
            set { SetProperty(ref _preview, value); }
        }
        #endregion Preview

        #region Errors
        private List<T4Error> _errors;
        public List<T4Error> Errors
        {
            get { return _errors; }
            set
            {
                if (SetProperty(ref _errors, value))
                    ShowErrors = Errors?.Count > 0;
            }
        }
        #endregion Errors

        #region IsModified
        public bool IsModified => _initialBuffer != Buffer;
        #endregion IsModified

        #region PredefinedFields
        private List<InsertFieldViewModel> _predefinedFields;
        public List<InsertFieldViewModel> PredefinedFields
        {
            get { return _predefinedFields; }
            set { SetProperty(ref _predefinedFields, value); }
        }
        #endregion PredefinedFields

        #region CustomFields
        private List<InsertFieldViewModel> _customFields;
        public List<InsertFieldViewModel> CustomFields
        {
            get { return _customFields; }
            set { SetProperty(ref _customFields, value); }
        }
        #endregion CustomFields

        #endregion Properties

        #region Commands
        
        #endregion Commands

        #region Public Methods

        public static T4UserControlViewModel Create(IUnityContainer container, string buffer, List<InsertFieldViewModel> predefinedFields, List<InsertFieldViewModel> customFields)
        {
            var rval = container.Resolve<T4UserControlViewModel>();
            rval.Init(buffer);
            return rval;
        }

        public void Init(string buffer)
        {
            _initialBuffer = buffer;
            Buffer = buffer;
        }

        public void ResetFieldValues(List<InsertFieldViewModel> predefinedFields, List<InsertFieldViewModel> customFields)
        {
            PredefinedFields = predefinedFields;
            CustomFields = customFields;
            Transform();
        }

        #endregion Public Methods
        
        #region Virtuals

        #endregion Virtuals

        #region Private Helpers

        private void Transform()
        {
            if (PredefinedFields == null || CustomFields == null)
                return;

            try
            {
                string preview;
                Errors = TemplateService.Transform(Header + Buffer, PredefinedFields, CustomFields, out preview);
                var lc = Header.LineCount();
                foreach (var r in Errors)
                {
                    r.Line -= lc;
                    if (r.Line < 1)
                        r.Line = 1;
                }
                Preview = preview;
            }
            catch (Exception ex)
            {
                Errors = new List<T4Error> { new T4Error(ex.ToString(), 0, 0) };
                Preview = null;
            }
        }

        #endregion Private Helpers
    }
}

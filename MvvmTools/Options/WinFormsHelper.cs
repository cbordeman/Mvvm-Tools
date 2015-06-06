using System;
using System.Windows.Forms;

namespace MvvmTools.Options
{
    /// <summary>
    /// Provides Windows.Forms GUI helper functions.
    /// It provides a boolean property to suppress message boxes.
    /// </summary>
    public static class WinFormsHelper
    {
        #region Data

        private static bool _messageBoxAllowed = true;
        private static DialogResult _fakeResult = DialogResult.None;

        #endregion Data

        #region Properties

        /// <summary>
        /// Gets or sets the value that indicates whether to display MessageBox.
        /// </summary>
        /// <remarks>Used in MessageBox simulation purposes. By default is true.</remarks>
        public static bool AllowMessageBox
        {
            get { return _messageBoxAllowed; }
            set { _messageBoxAllowed = value; }
        }

        /// <summary>
        /// Gets or sets fake DialogResult value.
        /// </summary>
        /// <remarks>Used in MessageBox simulation purposes. By default - DialogResult.None.</remarks>
        public static DialogResult FakeDialogResult
        {
            get { return _fakeResult; }
            set { _fakeResult = value; }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Shows Windows.Forms message box based on passed parameters if AllowMessageBox property is true.
        /// </summary>
        /// <returns>If MessageBox was not shown - method returns FakeDialogResult.</returns>
        public static DialogResult ShowMessageBox(string text, string caption, MessageBoxButtons buttons,
            MessageBoxIcon icon)
        {
            if (!String.IsNullOrEmpty(text) && _messageBoxAllowed)
            {
                return MessageBox.Show(text, caption, buttons, icon, MessageBoxDefaultButton.Button1);
            }
            return _fakeResult;
        }

        /// <summary>
        /// Shows Windows.Forms message box (with specified text message and button set) if AllowMessageBox property is true.
        /// </summary>
        /// <returns>If MessageBox was not shown - method returns FakeDialogResult.</returns>
        public static DialogResult ShowMessageBox(string text, MessageBoxButtons buttons)
        {
            return ShowMessageBox(text, Resources.MessageCaption, buttons, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Shows Windows.Forms message box (with specified text message and OKCancel button set) if AllowMessageBox property is true.
        /// </summary>
        /// <returns>If MessageBox was not shown - method returns FakeDialogResult.</returns>
        public static DialogResult ShowMessageBox(string text)
        {
            return ShowMessageBox(text, Resources.MessageCaption, MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
        }

        #endregion Methods
    }
}

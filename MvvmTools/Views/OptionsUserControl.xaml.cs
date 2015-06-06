using System.Windows.Controls;
using MvvmTools.Options;

namespace MvvmTools.Views
{
    /// <summary>
    /// Interaction logic for OptionsUserControl.xaml
    /// </summary>
    public partial class OptionsUserControl : UserControl
    {
        public OptionsUserControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets or Sets the reference to the underlying OptionsPage object.
        /// </summary>
        public OptionsPageGeneral OptionsPage { get; set; }
    }
}

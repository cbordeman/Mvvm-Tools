using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace MvvmTools.Core.Services
{
    public interface IOutputService
    {
        void WriteLine(string line);
    }

    public class OutputService : IOutputService
    {
        #region Data

        private readonly IVsOutputWindowPane _pane;

        #endregion Data

        #region Ctor and Init

        public OutputService(IVsOutputWindow outputWindow)
        {
            var paneGuid = VSConstants.GUID_OutWindowDebugPane;
            outputWindow.GetPane(ref paneGuid, out _pane);
        }

        #endregion Ctor and Init

        #region Public Methods

        public void WriteLine(string line)
        {
            _pane.OutputString(line);
#if DEBUG
            _pane.Activate();
#endif
        }

        #endregion Public Methods
    }
}

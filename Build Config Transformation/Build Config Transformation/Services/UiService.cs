using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;

namespace BuildConfigTransformation.Services
{
    public static class UiService
    {
        public const int IDOK = 1;

        public static int ShowMessageBox(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
        {
            int pnResult;
            IVsUIShell vsUiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
           
            ErrorHandler.ThrowOnFailure(vsUiShell.ShowMessageBox(0, Guid.Empty, title, message, string.Empty, 0, buttons, 0, icon, 0, out pnResult));
            return pnResult;
        }

        public static void ShowTransformDifference(string sourceFile, string transformFile)
        {
            string transformTmpFilePath = TransformService.CreateTransformResultTempFile(sourceFile, transformFile);

            IVsWindowFrame2 windowFrame = ShowDifferenceFrame(sourceFile, transformTmpFilePath, Path.GetFileName(sourceFile), Path.GetFileName(transformFile)) as IVsWindowFrame2;

            if (windowFrame != null)
            {
                uint pdwCookie;
                WindowFrameSink windowFrameSink = new WindowFrameSink();
                windowFrameSink.OnShow_WinClosedEvent += (sender, evnt) => {
                    FileService.RemoveTempFile(transformTmpFilePath);
                };

                windowFrame.Advise(windowFrameSink, out pdwCookie);
            }
            else
            {
                FileService.RemoveTempFile(transformTmpFilePath);
            }
        }

        private static IVsWindowFrame ShowDifferenceFrame(string leftFile, string rightFile, string leftLabel, string rightLabel)
        {
            IVsDifferenceService differenceService = Package.GetGlobalService(typeof(SVsDifferenceService)) as IVsDifferenceService;
            if (differenceService == null)
                throw new NotSupportedException("IVsDifferenceService");

            string caption = string.Format("{0} & {1}", leftLabel, rightLabel);
            string tooltip = string.Format("Difference: {0}", rightLabel);

            return differenceService.OpenComparisonWindow2(leftFile, rightFile, caption, tooltip, leftLabel, rightLabel, "{0}&{1}", null, (uint)__VSDIFFSERVICEOPTIONS.VSDIFFOPT_RightFileIsTemporary);
        }
    }
}

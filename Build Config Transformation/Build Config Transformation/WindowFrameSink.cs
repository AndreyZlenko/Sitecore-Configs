using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using System;

namespace BuildConfigTransformation
{
    class WindowFrameSink : IVsWindowFrameNotify
    {

        public event EventHandler OnDockableChangeEvent;
        public event EventHandler OnMoveEvent;
        public event EventHandler OnShow_AutoHideSlideBeginEvent;
        public event EventHandler OnShow_WinClosedEvent;
        public event EventHandler OnShow_WinShownEvent;
        public event EventHandler OnShow_WinHiddenEvent;
        public event EventHandler OnSizeEvent;


        public int OnDockableChange(int fDockable)
        {
            OnDockableChangeEvent?.Invoke(this, new EventArgs());
            return VSConstants.S_OK;
        }

        public int OnMove()
        {
            OnMoveEvent?.Invoke(this, new EventArgs());
            return VSConstants.S_OK;
        }

        public int OnShow(int fShow)
        {
            switch (fShow)
            {
                case (int)__FRAMESHOW.FRAMESHOW_AutoHideSlideBegin:
                    OnShow_AutoHideSlideBeginEvent?.Invoke(this, new EventArgs());
                    break;

                case (int)__FRAMESHOW.FRAMESHOW_WinClosed:
                    OnShow_WinClosedEvent?.Invoke(this, new EventArgs());
                    break;

                case (int)__FRAMESHOW.FRAMESHOW_WinShown:
                    OnShow_WinShownEvent?.Invoke(this, new EventArgs());
                    break;

                case (int)__FRAMESHOW.FRAMESHOW_WinHidden:
                    OnShow_WinHiddenEvent?.Invoke(this, new EventArgs());
                    break;

                default:
                    break;

            }

            return VSConstants.S_OK;
        }

        public int OnSize()
        {
            OnSizeEvent?.Invoke(this, new EventArgs());
            return VSConstants.S_OK;
        }
    }
}

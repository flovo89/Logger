using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using Logger.Common.Base.ObjectModel;




namespace Logger.Core.Interfaces
{
    public interface IShellManager : ISynchronizable
    {
        void Disable (string shellName);

        void DisableAll ();

        void Enable (string shellName);

        void EnableAll ();

        Window GetMainWindow ();

        IShell GetShell (string shellName);

        IShell[] GetShells (string shellName);

        void Hide (string shellName);

        void HideAll ();

        void HideDialog (string childShellName);

        void Maximize (string shellName);

        void MaximizeAll ();

        void Minimize (string shellName);

        void MinimizeAll ();

        void MoveAllToBackground ();

        void MoveAllToForeground ();

        void MoveAllToPrimaryScreen ();

        void MoveAllToScreen (int screenIndex);

        void MoveAllToScreen (Screen screen);

        void MoveToBackground (string shellName);

        void MoveToForeground (string shellName);

        void MoveToPrimaryScreen (string shellName);

        void MoveToScreen (string shellName, int screenIndex);

        void MoveToScreen (string shellName, Screen screen);

        void SetAllCaption (string caption);

        void SetAllHideMouseCursor (bool hideMouseCursor);

        void SetAllIcon (Icon icon);

        void SetAllIcon (ImageSource icon);

        void SetAllRectangle (Rect rectangle);

        void SetAllState (WindowState state);

        void SetCaption (string shellName, string caption);

        void SetHideMouseCursor (string shellName, bool hideMouseCursor);

        void SetIcon (string shellName, Icon icon);

        void SetIcon (string shellName, ImageSource icon);

        void SetParent (string shellName, string parentShellName);

        void SetRectangle (string shellName, Rect rectangle);

        void SetState (string shellName, WindowState state);

        void Show (string shellName);

        void ShowAll ();

        void ShowDialog (string parentShellName, string childShellName, bool modal);
    }
}

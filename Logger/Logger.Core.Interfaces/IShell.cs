using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;




namespace Logger.Core.Interfaces
{
    public interface IShell
    {
        Window AssociatedWindow { get; }

        bool IsVisible { get; }

        string Name { get; }

        IViewModel ViewModel { get; set; }

        void Disable ();

        void Enable ();

        Rect GetRectangle ();

        Screen GetScreen ();

        int GetScreenIndex ();

        WindowState GetState ();

        void Hide ();

        void Initialize ();

        void Maximize ();

        void Minimize ();

        void MoveToBackground ();

        void MoveToForeground ();

        void MoveToPrimaryScreen ();

        void MoveToScreen (int screenIndex);

        void MoveToScreen (Screen screen);

        void OnActivate ();

        void OnDeactivate ();

        void SetCaption (string caption);

        void SetHideMouseCursor (bool hideMosueCursor);

        void SetIcon (Icon icon);

        void SetIcon (ImageSource icon);

        void SetParent (IShell parentShell);

        void SetRectangle (Rect rectangle);

        void SetState (WindowState state);

        void Show ();
    }
}

using System.Windows.Forms;




namespace Logger.Core.Interfaces
{
    public interface ISplashScreen
    {
        bool IsVisible { get; }

        string Name { get; }

        IViewModel ViewModel { get; set; }

        void Hide ();

        void Initialize ();

        void MoveToBackground ();

        void MoveToForeground ();

        void MoveToPrimaryScreen ();

        void MoveToScreen (int screenIndex);

        void MoveToScreen (Screen screen);

        void OnActivate ();

        void OnDeactivate ();

        void SetFooter (string footerText);

        void SetHideMouseCursor (bool hideMosueCursor);

        void SetLicenseInformation (string licenseInformation);

        void SetStatus (string statusText);

        void Show ();
    }
}

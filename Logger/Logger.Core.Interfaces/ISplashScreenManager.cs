using System.Windows.Forms;

using Logger.Common.ObjectModel;




namespace Logger.Core.Interfaces
{
    public interface ISplashScreenManager : ISynchronizable
    {
        ISplashScreen GetSplashScreen (string splashScreenName);

        ISplashScreen[] GetSplashScreens (string splashScreenName);

        void Hide (string splashScreenName);

        void HideAll ();

        void MoveAllToBackground ();

        void MoveAllToForeground ();

        void MoveAllToPrimaryScreen ();

        void MoveAllToScreen (int screenIndex);

        void MoveAllToScreen (Screen screen);

        void MoveToBackground (string splashScreenName);

        void MoveToForeground (string splashScreenName);

        void MoveToPrimaryScreen (string splashScreenName);

        void MoveToScreen (string splashScreenName, int screenIndex);

        void MoveToScreen (string splashScreenName, Screen screen);

        void SetAllFooter (string footerText);

        void SetAllHideMouseCursor (bool hideMouseCursor);

        void SetAllLicenseInformation (string licenseInformation);

        void SetAllStatus (string statusText);

        void SetFooter (string splashScreenName, string footerText);

        void SetHideMouseCursor (string splashScreenName, bool hideMouseCursor);

        void SetLicenseInformation (string splashScreenName, string licenseInformation);

        void SetStatus (string splashScreenName, string statusText);

        void Show (string splashScreenName);

        void ShowAll ();
    }
}

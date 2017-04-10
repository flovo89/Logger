using System.Globalization;




namespace Logger.Core.Interfaces.Session
{
    public interface ISessionCultureAware
    {
        void OnFormattingCultureChanged (CultureInfo formattingCulture);

        void OnUiCultureChanged (CultureInfo uiCulture);
    }
}

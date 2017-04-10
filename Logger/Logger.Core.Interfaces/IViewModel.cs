using System.ComponentModel;

using Microsoft.Practices.Prism.Regions;




namespace Logger.Core.Interfaces
{
    public interface IViewModel : INotifyPropertyChanged,
            INavigationAware,
            IConfirmNavigationRequest
    {
        string Name { get; }

        void Initialize ();
    }
}

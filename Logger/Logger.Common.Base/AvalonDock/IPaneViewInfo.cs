using System.ComponentModel;
using System.Windows;
using System.Windows.Media;




namespace Logger.Common.AvalonDock
{
    public interface IPaneViewInfo : INotifyPropertyChanged
    {
        ImageSource Icon { get; }

        bool IsCloseable { get; }

        string Title { get; }

        event RequestBringIntoViewEventHandler RequestBringIntoView;

        void UpdateIsActive (bool isActive);
    }
}

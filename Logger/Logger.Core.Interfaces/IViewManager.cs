using Logger.Common.ObjectModel;

using Microsoft.Practices.Prism.Mvvm;
using Microsoft.Practices.Prism.Regions;




namespace Logger.Core.Interfaces
{
    public interface IViewManager : ISynchronizable
    {
        void AddView (string regionName, string viewName);

        void AddView (string regionName, object view);

        void ClearView (string regionName);

        IRegion GetRegion (string regionName);

        IView GetView (string viewName);

        IViewModel GetViewModel (string viewModelName);

        IView[] GetViews (string viewName);

        void Navigate (string regionName, string viewName);

        void RemoveView (string regionName, string viewName);

        void RemoveView (string regionName, object view);

        void SetView (string regionName, string viewName);

        void SetView (string regionName, object view);
    }
}

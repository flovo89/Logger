using System.Collections.Generic;
using System.Globalization;

using Logger.Common.ObjectModel;




namespace Logger.Core.Interfaces.Resources
{
    public interface IResourceManager : ISynchronizable
    {
        IResourceSet[] GetAvailableResourceSets ();

        object GetResource (string key);

        string GetText (string key);

        IResourceSet[] GetUsedResourceSets ();

        void InitializeResource (string key, object value);

        void InitializeText (string key, string value);

        void Load ();

        void Save ();

        void SetResource (string key, object value);

        void SetText (string key, string value);

        void SetUsedResourceSets (IEnumerable<IResourceSet> resourceSets, bool updateResources);

        void SetUsedResourceSetsFromCulture (CultureInfo culture, bool updateResources);

        void UpdateResources ();
    }
}

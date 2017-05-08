using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

using Logger.Common.Collections.Generic;
using Logger.Common.DataTypes;
using Logger.Common.Globalization;
using Logger.Common.IO.Documents.Ini;
using Logger.Common.IO.Files;
using Logger.Common.ObjectModel.Exceptions;
using Logger.Common.Windows;
using Logger.Core.Interfaces;
using Logger.Core.Interfaces.Logging;




namespace Logger.Core.Resources
{
    [Export (typeof(IResourceManager))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class ResourceManager : IResourceManager
    {
        #region Constants

        private const string DefaultResourceSetKeySettingsKey = "Load";

        private const string DefaultsDirectoryName = "Defaults";

        private const string SettingsBackupStreamName = "Resources.ini";

        private const string SettingsFileName = "Resources.ini";

        private static readonly Guid SettingsBackupId = new Guid("22F439E5-AF16-49FF-B8D5-72FE85022DB2");

        private static readonly Encoding SettingsFileEncoding = Encoding.UTF8;

        private static readonly string SettingsSectionName = string.Empty;

        #endregion




        #region Instance Constructor/Destructor

        [ImportingConstructor]
        public ResourceManager (ISessionManager sessionManager)
        {
            if (sessionManager == null)
            {
                throw new ArgumentNullException(nameof(sessionManager));
            }

            this.SyncRoot = new object();

            this.ResourceDictionary = new ResourceDictionary();
            sessionManager.Application.Resources.MergedDictionaries.Add(this.ResourceDictionary);

            this.SettingsFile = sessionManager.DataFolder.Append(new FilePath(ResourceManager.SettingsFileName));
            this.SettingsFile.Create();

            this.DefaultFile = sessionManager.ExecutableFolder.Append(new DirectoryPath(ResourceManager.DefaultsDirectoryName).Append(new FilePath(ResourceManager.SettingsFileName)));

            this.SettingsEncoding = ResourceManager.SettingsFileEncoding;

            this.SectionName = ResourceManager.SettingsSectionName;

            this.DefaultResourceSetKeyKey = ResourceManager.DefaultResourceSetKeySettingsKey;

            this.BackupId = ResourceManager.SettingsBackupId;
            this.BackupStreamName = ResourceManager.SettingsBackupStreamName;

            this.InitializedUsedResourceSets = new HashSet<IResourceSet>();

            this.SuppressResourceChangeLogMessages = false;
        }

        #endregion




        #region Instance Properties/Indexer

        public Guid BackupId { get; private set; }

        public string BackupStreamName { get; private set; }

        public FilePath DefaultFile { get; }

        public string DefaultResourceSetKeyKey { get; }

        public ResourceDictionary ResourceDictionary { get; }

        public string SectionName { get; }

        public Encoding SettingsEncoding { get; }

        public FilePath SettingsFile { get; }

        [ImportMany (typeof(IResourceConverter), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<IResourceConverter>> Converters { get; private set; }

        [Import (typeof(ILogManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ILogManager> LogManager { get; private set; }

        [ImportMany (typeof(IResourceAware), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<IResourceAware>> ResourceAwares { get; private set; }

        [ImportMany (typeof(IResourceSource), AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Any)]
        internal IEnumerable<Lazy<IResourceSource>> ResourceSources { get; private set; }

        [Import (typeof(ISessionManager), AllowDefault = true, AllowRecomposition = true, RequiredCreationPolicy = CreationPolicy.Shared)]
        internal Lazy<ISessionManager> SessionManager { get; private set; }

        private HashSet<IResourceSet> InitializedUsedResourceSets { get; }

        private bool SuppressResourceChangeLogMessages { get; set; }

        #endregion




        #region Instance Methods

        public void BeginBackup ()
        {
        }

        public void BeginRestore ()
        {
        }

        public void EndBackup ()
        {
        }

        public void EndRestore ()
        {
        }

        private object ConvertResourceToObject (Stream stream, Encoding encoding, string targetType)
        {
            Lazy<IResourceConverter>[] converters = this.Converters.ToArray();
            foreach (Lazy<IResourceConverter> converter in converters)
            {
                object value = null;
                if (converter.Value.ConvertToObject(stream, encoding, targetType, out value))
                {
                    return value;
                }
            }

            return null;
        }

        private void LoadFile (FilePath file, Encoding encoding, IDictionary<string, IList<string>> values)
        {
            if (!file.Exists)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Warning, "Resource set keys file does not exist: {0}", file);
                return;
            }

            this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Loading resource set keys file: {0}", file);

            string fileContent = file.ReadTextTransactive(encoding);
            IniDocument iniDocument = new IniDocument();
            iniDocument.Load(fileContent);
            iniDocument.RemoveTextElements();
            iniDocument.RemoveEmptySections(false);

            Dictionary<string, IList<string>> tempValues = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);

            if (iniDocument.Sections.ContainsSection(this.SectionName))
            {
                IniSection iniSection = iniDocument.Sections[this.SectionName];
                foreach (IniValueElement element in iniSection.Elements)
                {
                    if (!tempValues.ContainsKey(element.Key))
                    {
                        tempValues.Add(element.Key, new List<string>());
                    }

                    tempValues[element.Key].Add(element.Value);
                }
            }

            if (tempValues.Count == 0)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Warning, "Resource set keys file contains no settings: {0}", file);
            }

            foreach (KeyValuePair<string, IList<string>> tempValue in tempValues)
            {
                if (!values.ContainsKey(tempValue.Key))
                {
                    values.Add(tempValue.Key, new List<string>());
                }

                values[tempValue.Key].AddRange(tempValue.Value);
            }
        }

        private void NotifyResourceChange (string key, object value)
        {
            if (!this.SuppressResourceChangeLogMessages)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Notifying resource change: {0} = {1}", key ?? "[null]", value ?? "[null]");
            }

            Lazy<IResourceAware>[] resourceAwares = this.ResourceAwares.ToArray();
            foreach (Lazy<IResourceAware> resourceAware in resourceAwares)
            {
                resourceAware.Value.OnResourceChanged(key, value);
            }
        }

        #endregion




        #region Interface: IResourceManager

        public bool IsSynchronized
        {
            get
            {
                return true;
            }
        }

        public object SyncRoot { get; }

        public IResourceSet[] GetAvailableResourceSets ()
        {
            lock (this.SyncRoot)
            {
                List<IResourceSet> resourceSets = new List<IResourceSet>();
                Lazy<IResourceSource>[] resourceSources = this.ResourceSources.ToArray();
                foreach (Lazy<IResourceSource> resourceSource in resourceSources)
                {
                    resourceSets.AddRange(resourceSource.Value.GetAvailableResourceSets());
                }

                resourceSets.RemoveWhere(x =>
                {
                    if (x.SessionModes == null)
                    {
                        return false;
                    }

                    foreach (string sessionMode in x.SessionModes)
                    {
                        if (this.SessionManager.Value.SessionMode.Contains(sessionMode, StringComparer.InvariantCultureIgnoreCase))
                        {
                            return false;
                        }
                    }

                    return true;
                });

                return resourceSets.ToArray();
            }
        }

        public object GetResource (string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                if (this.ResourceDictionary.Contains(key))
                {
                    return this.ResourceDictionary[key];
                }

                return null;
            }
        }

        public string GetText (string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                if (this.ResourceDictionary.Contains(key))
                {
                    return this.ResourceDictionary[key] as string;
                }

                return null;
            }
        }

        public IResourceSet[] GetUsedResourceSets ()
        {
            lock (this.SyncRoot)
            {
                return this.InitializedUsedResourceSets.ToArray();
            }
        }

        public void InitializeResource (string key, object defaultValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                object existingValue = this.GetResource(key);
                if (existingValue == null)
                {
                    this.SetResource(key, defaultValue);
                }
            }
        }

        public void InitializeText (string key, string defaultValue)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                string existingValue = this.GetText(key);
                if (existingValue == null)
                {
                    this.SetText(key, defaultValue);
                }
            }
        }

        public void Load ()
        {
            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Loading resources");

                IDictionary<string, IList<string>> values = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);
                this.LoadFile(this.SettingsFile, this.SettingsEncoding, values);
                if (values.Count == 0)
                {
                    this.LoadFile(this.DefaultFile, this.SettingsEncoding, values);
                }

                HashSet<IResourceSet> loadedResourceSets = new HashSet<IResourceSet>();

                if (values.ContainsKey(this.DefaultResourceSetKeyKey))
                {
                    IResourceSet[] availableResourceSets = this.GetAvailableResourceSets();

                    foreach (string value in values[this.DefaultResourceSetKeyKey])
                    {
                        foreach (IResourceSet availableResourceSet in availableResourceSets)
                        {
                            if (string.Equals(availableResourceSet.Key, value, StringComparison.InvariantCultureIgnoreCase))
                            {
                                loadedResourceSets.Add(availableResourceSet);
                            }
                        }
                    }
                }

                this.SetUsedResourceSets(loadedResourceSets, true);
            }
        }

        public void Save ()
        {
            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Saving resources");

                IniDocument iniDocument = new IniDocument();
                IniSection iniSection = new IniSection(this.SectionName);
                iniDocument.Sections.Add(iniSection);

                IResourceSet[] usedResourceSets = this.GetUsedResourceSets();
                foreach (IResourceSet usedResourceSet in usedResourceSets)
                {
                    iniSection.Elements.Add(new IniValueElement(this.DefaultResourceSetKeyKey, usedResourceSet.Key));
                }

                iniDocument.Normalize(IniDocumentNormalizeOptions.SortSections | IniDocumentNormalizeOptions.SortElements | IniDocumentNormalizeOptions.RemoveEmptySections | IniDocumentNormalizeOptions.RemoveTextElements | IniDocumentNormalizeOptions.MergeSections);

                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Saving resource set keys file: {0}", this.SettingsFile);
                string fileContent = iniDocument.ToString();
                this.SettingsFile.WriteTextTransactive(fileContent, this.SettingsEncoding);
            }
        }

        public void SetResource (string key, object value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                if (!this.SuppressResourceChangeLogMessages)
                {
                    this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting resource: {0} = {1}", key, value ?? "[null]");
                }

                if (value == null)
                {
                    this.ResourceDictionary.Remove(key);
                }
                else
                {
                    if (this.ResourceDictionary.Contains(key))
                    {
                        this.ResourceDictionary[key] = value;
                    }
                    else
                    {
                        this.ResourceDictionary.Add(key, value);
                    }
                }

                this.NotifyResourceChange(key, value);
            }
        }

        public void SetText (string key, string value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (key.IsEmpty())
            {
                throw new EmptyStringArgumentException(nameof(key));
            }

            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting resource: {0} = {1}", key, value ?? "[null]");

                if (value == null)
                {
                    this.ResourceDictionary.Remove(key);
                }
                else
                {
                    if (this.ResourceDictionary.Contains(key))
                    {
                        this.ResourceDictionary[key] = value;
                    }
                    else
                    {
                        this.ResourceDictionary.Add(key, value);
                    }
                }

                this.NotifyResourceChange(key, value);
            }
        }

        public void SetUsedResourceSets (IEnumerable<IResourceSet> resourceSets, bool updateResources)
        {
            if (resourceSets == null)
            {
                throw new ArgumentNullException(nameof(resourceSets));
            }

            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting used resource sets (from selection): {0}", ( from x in resourceSets select x.Key ).Join(", "));

                this.InitializedUsedResourceSets.Clear();
                this.InitializedUsedResourceSets.AddRange(resourceSets);

                if (updateResources)
                {
                    this.UpdateResources();
                }
            }
        }

        public void SetUsedResourceSetsFromCulture (CultureInfo culture, bool updateResources)
        {
            lock (this.SyncRoot)
            {
                this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Setting used resource sets (from culture): {0}", culture == null ? "[default]" : culture.ToString());

                IResourceSet[] availableResourceSets = this.GetAvailableResourceSets();

                if (culture == null)
                {
                    foreach (IResourceSet availableResourceSet in availableResourceSets)
                    {
                        if (availableResourceSet.IsDefaultSet)
                        {
                            if (availableResourceSet.UiCulture != null)
                            {
                                culture = availableResourceSet.UiCulture;
                                break;
                            }
                        }
                    }
                }

                CultureInfo specificCulture = culture.IsNeutralCulture ? culture.GetDefaultSpecificCulture(true) : culture;
                CultureInfo neutralCulture = culture.IsNeutralCulture ? culture : culture.GetNeutralCulture(true);

                HashSet<IResourceSet> resourceSetsToSelect = new HashSet<IResourceSet>();

                foreach (IResourceSet availableResourceSet in availableResourceSets)
                {
                    if (availableResourceSet.IsDefaultSet)
                    {
                        resourceSetsToSelect.Add(availableResourceSet);
                        continue;
                    }

                    if (availableResourceSet.UiCulture != null)
                    {
                        CultureInfo currentSpecificCulture = availableResourceSet.UiCulture.IsNeutralCulture ? availableResourceSet.UiCulture.GetDefaultSpecificCulture(true) : availableResourceSet.UiCulture;
                        CultureInfo currentNeutralCulture = availableResourceSet.UiCulture.IsNeutralCulture ? availableResourceSet.UiCulture : availableResourceSet.UiCulture.GetNeutralCulture(true);

                        if (currentSpecificCulture.Equals(specificCulture) || currentNeutralCulture.Equals(neutralCulture))
                        {
                            resourceSetsToSelect.Add(availableResourceSet);
                        }
                    }
                }

                this.SetUsedResourceSets(resourceSetsToSelect, updateResources);
            }
        }

        public void UpdateResources ()
        {
            lock (this.SyncRoot)
            {
                try
                {
                    this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Updating resources");

                    using (TemporaryCursor.Hourglass())
                    {
                        this.SuppressResourceChangeLogMessages = true;

                        List<IResourceSet> resourceSetsToUpdate = new List<IResourceSet>();
                        IResourceSet[] availableResourceSets = this.GetAvailableResourceSets();

                        foreach (IResourceSet availableResourceSet in availableResourceSets)
                        {
                            if (availableResourceSet.IsDefaultSet)
                            {
                                resourceSetsToUpdate.AddUnique(availableResourceSet, true);
                            }
                        }

                        resourceSetsToUpdate.AddRangeUnique(this.GetUsedResourceSets(), true);

                        Func<Stream, Encoding, string, object> converter = this.ConvertResourceToObject;
                        Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
                        List<CultureInfo> uiCultures = new List<CultureInfo>();
                        List<CultureInfo> formattingCultures = new List<CultureInfo>();
                        foreach (IResourceSet resourceSet in resourceSetsToUpdate)
                        {
                            this.LogManager.Value.Log(this.GetType().Name, LogLevel.Debug, "Updating resource set: {0} ({1}, {2}).", resourceSet.DisplayName, resourceSet.UiCulture == null ? "[no UI culture]" : resourceSet.UiCulture.ToString(), resourceSet.FormattingCulture == null ? "[no formatting culture]" : resourceSet.FormattingCulture.ToString());
                            resourceSet.Load(values, converter);
                            if (resourceSet.UiCulture != null)
                            {
                                uiCultures.Add(resourceSet.UiCulture);
                            }
                            if (resourceSet.FormattingCulture != null)
                            {
                                formattingCultures.Add(resourceSet.FormattingCulture);
                            }
                        }
                        foreach (KeyValuePair<string, object> value in values)
                        {
                            this.SetResource(value.Key, value.Value);
                        }

                        if (uiCultures.Count > 0)
                        {
                            this.SessionManager.Value.SetUiCulture(uiCultures[uiCultures.Count - 1]);
                        }
                        else
                        {
                            this.SessionManager.Value.SetUiCulture(this.SessionManager.Value.StartupUiCulture);
                        }

                        if (formattingCultures.Count > 0)
                        {
                            this.SessionManager.Value.SetFormattingCulture(formattingCultures[formattingCultures.Count - 1]);
                        }
                        else
                        {
                            this.SessionManager.Value.SetFormattingCulture(this.SessionManager.Value.StartupFormattingCulture);
                        }
                    }
                }
                finally
                {
                    this.SuppressResourceChangeLogMessages = false;
                }
            }
        }

        #endregion
    }
}

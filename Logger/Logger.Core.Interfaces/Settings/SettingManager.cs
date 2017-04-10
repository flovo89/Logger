using System;

using Logger.Common.Base.ObjectModel;




namespace Logger.Core.Interfaces.Settings
{
    public interface ISettingManager : ISynchronizable
    {
        void DeleteSetting (string key);

        void DeleteSetting (Predicate<string> predicate);

        string[] GetKeys ();

        string GetSetting (string key);

        T GetSetting <T> (string key);

        void InitializeSetting (string key, string defaultValue);

        void InitializeSetting <T> (string key, T defaultValue);

        void Load ();

        void Save ();

        void SetSetting (string key, string stringValue);

        void SetSetting <T> (string key, T value);
    }
}

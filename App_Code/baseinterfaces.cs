namespace ApiBase.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using ApiBase.Models;

    public interface ISettings
    {
        bool DeleteSetting(string _key);
        List<Setting> GetEditSetting(string _key);
        bool UpdateSetting(List<Setting> setting);
        bool CreateSetting(List<Setting> setting);
        bool SaveSettingValue(string _key,string _value);
        string GetSettingValue(string _key);
        DataTable GetTableFromJson(string _json);
        string GetJsonFromTable(DataTable _table);
        List<Setting> GetSettings(int _section);
        Setting GetSetting(string _key);
        bool SaveSettings(List<Setting> _settings);
    }

    public interface IJobs
    {
        List<Job> GetJobs();
    }

    public interface ILogs
    {
        string ClearLog();
        void NewLog(string _description);
        void NewLog(Exception e, string _description);
    }

    public interface ISchedulers
    {
        void Start();
        void Shutdown();
        bool isStarted();
        bool TriggerJob(string jobname);
    }

    public interface IGlobal
    {
        string hpass {get;set;}
        string sqlitedb {get;set;}
        string binpath {get;set;}
        string urllink {get;set;}
        string mailhost {get;set;}
        string templatesource {get;set;}
        string SendEmail(string to, string subject, string body);
        Int64 GetTime();
        DataTable ConvertObjectToTable(object _class);
    }

    public interface IRazorService
    {
        string RunCompile(string templatename, object model = null);
    }

    public interface ICachingProvider
    {
        bool AddCache(string key, object value);
        bool RemoveCache(string key);
        object GetCacheValue(string key);
        bool SetCacheValue(string key, object value);
        bool CacheKeyExist(string key);
    }
}
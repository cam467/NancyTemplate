namespace App.Jobs
{
    using System;
    using Quartz;
    using ApiBase.Interfaces;

    public class ImportJob : IJob
    {
        private readonly ISettings _settings;
        private readonly ILogs _log;
        private readonly ISchedulers _scheduler;
        private readonly IImporter _importer;
        private readonly ICachingProvider _cache;
        public ImportJob(ISettings settings,ILogs log,ISchedulers scheduler,IImporter importer,ICachingProvider cache)
        {
            this._settings = settings;
            this._log = log;
            this._scheduler = scheduler;
            this._importer = importer;
            this._cache = cache;
        }

        public void Execute(IJobExecutionContext context)
        {
            bool pause = _settings.GetSettingValue("schactive") == "Yes" ? true : false;
            if (pause)
            {
                _scheduler.Shutdown();
                return;
            }
            if (_cache.CacheKeyExist("_jobrunning_"))
            {
                _log.NewLog("KnowBe4 import in progress...");
                return;
            }
            _log.NewLog("KnowBe4 import started...");
            _cache.AddCache("_jobrunning_","running");
            _importer.RunImport();
            _cache.RemoveCache("_jobrunning_");
            _log.NewLog("KnowBe4 import completed");
            return;
        }
    }

    public class ExportJob : IJob
    {
        private readonly ISettings _settings;
        private readonly ILogs _log;
        private readonly ISchedulers _scheduler;
        private readonly IExporter _exporter;
        private readonly ICachingProvider _cache;
        public ExportJob(ISettings settings,ILogs log,ISchedulers scheduler,IExporter exporter,ICachingProvider cache)
        {
            this._settings = settings;
            this._log = log;
            this._scheduler = scheduler;
            this._exporter = exporter;
            this._cache = cache;
        }

        public void Execute(IJobExecutionContext context)
        {
            bool pause = _settings.GetSettingValue("schactive") == "Yes" ? true : false;
            if (pause)
            {
                _scheduler.Shutdown();
                return;
            }
            if (_cache.CacheKeyExist("_jobrunning_"))
            {
                _log.NewLog("KnowBe4 export in progress...");
                return;
            }
            _log.NewLog("KnowBe4 export started...");
            _cache.AddCache("_jobrunning_","running");
            _exporter.RunExport();
            _cache.RemoveCache("_jobrunning_");
            _log.NewLog("KnowBe4 export completed");
            return;
        }
    }
}
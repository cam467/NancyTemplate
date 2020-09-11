using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Nancy.TinyIoc;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;
using ApiBase.Interfaces;
using ApiBase.Models;

namespace ApiBase.Schedulers
{
    public class Jobs : IJobs
    {
        private ISettings _settings;
        private ILogs _log;
        public Jobs(ISettings settings,ILogs log)
        {
            _settings = settings;
            _log = log;
        }

        public List<Job> GetJobs()
        {
            DataTable jobset = _settings.GetTableFromJson(_settings.GetSettingValue("schschedules"));
            List<Job> jobs = new List<Job>();
            if (jobset == null) return jobs;
            var columns = jobset.Columns.Cast<DataColumn>().Select((x,i) => new {name=x.ColumnName,index=i});
            int colname = columns.Any(x => x.name.ToLower().Contains("name")) ? columns.First(x => x.name.ToLower().Contains("name")).index : -1,
                colcron = columns.Any(x => x.name.ToLower().Contains("cron")) ? columns.First(x => x.name.ToLower().Contains("cron")).index : -1,
                colactive = columns.Any(x => x.name.ToLower().Contains("active")) ? columns.First(x => x.name.ToLower().Contains("active")).index : -1,
                colparam = columns.Any(x => x.name.ToLower().Contains("param")) ? columns.First(x => x.name.ToLower().Contains("param")).index : -1,
                colclass = columns.Any(x => x.name.ToLower().Contains("class")) ? columns.First(x => x.name.ToLower().Contains("class")).index : -1;
            if (colname == -1) return jobs;
            foreach(DataRow r in jobset.Rows)
            {
                try
                {
                    jobs.Add(new Job {jobname = r[colname].ToString(), jobcron = r[colcron].ToString(), jobclass = r[colclass].ToString(), jobparam = r[colparam].ToString(), jobactive = bool.Parse(r[colactive].ToString())});
                }
                catch (Exception ex)
                {
                    _log.NewLog("GetJobs error: " + ex.Message);
                    continue;
                }
            }
            return jobs;
        }
    }

    public class TinyIoCJobFactory : SimpleJobFactory
    {
        private readonly TinyIoCContainer _container;
        public TinyIoCJobFactory(TinyIoCContainer container)
        {
            this._container = container;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            return (IJob) this._container.Resolve(bundle.JobDetail.JobType);
        }
    }

    public class Scheduler : ISchedulers
    {
        private readonly ISettings _settings;
        private readonly ILogs _log;
        private readonly IJobs _jobs;
        private readonly TinyIoCContainer _container;
        private readonly ICachingProvider _cache;

    	public Scheduler(ISettings settings, ILogs log, IJobs jobs, TinyIoCContainer container, ICachingProvider cache)
        {
            this._log = log;
            this._settings = settings;
            this._jobs = jobs;
            this._container = container;
            this._cache = cache;
            _log.NewLog("Trying to Start Scheduler");
            Start();
        }

        public void Start()
        {
        	if (isStarted()) return;
            IScheduler qtz = StdSchedulerFactory.GetDefaultScheduler();
            qtz.JobFactory = new TinyIoCJobFactory(_container);
            qtz.Start();
            _cache.AddCache("_qscheduler_",qtz);
            _log.NewLog("Scheduler Started");
            
            List<Job> jobs = _jobs.GetJobs();
            foreach (Job j in jobs)
            {
                try
                {
                    if (!j.jobactive) continue;
                    IJobDetail job = JobBuilder.Create(Type.GetType(j.jobclass,true)).Build();
                    job.JobDataMap["param"] = j.jobparam;
                    _cache.AddCache("_"+j.jobname+"_",job.Key);
                    ITrigger trig = TriggerBuilder.Create().WithCronSchedule(j.jobcron).Build();
                    qtz.ScheduleJob(job,trig);
                    _log.NewLog("Job "+j.jobname+" Scheduled");
                }
                catch (Exception ex)
                {
                    _log.NewLog("Schedule Jobs error: " + ex.Message);
                    continue;
                }
            }
        }

        public void Shutdown()
        {
            IScheduler qtz = (IScheduler)_cache.GetCacheValue("_qscheduler_");
            try
            {
                qtz.Shutdown();
                _cache.RemoveCache("_qscheduler_");
                _log.NewLog("Scheduler Shutdown");
            }
            catch (Exception ex)
            {
                
            }
        }

        public bool isStarted()
        {
            return _cache.CacheKeyExist("_qscheduler_");
        }

        public bool TriggerJob(string jobname)
        {
            JobKey key = (JobKey) _cache.GetCacheValue("_" + jobname + "_");
            IScheduler qtz = (IScheduler) _cache.GetCacheValue("_qscheduler_");
        	try
        	{
        		qtz.TriggerJob(key);
        		return true;
        	}
        	catch
        	{
        		return false;
        	}
        }
    }
}
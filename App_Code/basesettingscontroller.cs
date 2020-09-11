using System;
using System.Collections.Generic;
using System.Data;
using Nancy;
using Nancy.ViewEngines.Razor;
using Nancy.Extensions;
using Nancy.Responses;
using Nancy.ModelBinding;
using ApiBase.Interfaces;
using ApiBase.Models;

namespace ApiBase.Controller
{
    public class ApiBaseController : NancyModule
    {
		private readonly ISettings _settings;
		private readonly ILogs _log;
		private readonly ISchedulers _scheduler;
        public ApiBaseController(ISettings settings, ILogs log, ISchedulers scheduler) : base("/api/config")
        {
			this._settings = settings;
			this._log = log;
			this._scheduler = scheduler;
            Nancy.Json.JsonSettings.MaxJsonLength = Int32.MaxValue;
			this.Before += ctx => { 
				this.ViewBag.headertitle = settings.GetSettingValue("cfgheadertitle");
				this.ViewBag.headericon = settings.GetSettingValue("cfgheadericon");
				this.ViewBag.headercolor = settings.GetSettingValue("cfgheadercolor");
				return null;
			};
            Get["/"] = _ =>
			{
				string menucolor = _settings.GetSettingValue("cfgmenucolor");
				string sections = _settings.GetSettingValue("cfgmenusections");
				var menusections = _settings.GetTableFromJson(sections);
				var sets = _settings.GetSettings(1);
				return View["views/config",new {pagetitle="",sectionidselected="1", menucolor=menucolor, menus=menusections, settings=sets, configview=1 }];
			};
            Get["/{id}"] = p =>
			{
				string menucolor = _settings.GetSettingValue("cfgmenucolor");
				string sections = _settings.GetSettingValue("cfgmenusections");
				var menusections = _settings.GetTableFromJson(sections);
				var sets = _settings.GetSettings(p.id);
				return View["views/config",new {pagetitle="",sectionidselected=p.id.ToString(),menucolor=menucolor, menus=menusections, settings=sets, configview=p.id }];
			};
			Post["/settings/{id}"] = p =>
			{
				var sets = _settings.GetSettings(p.id);
				string title = Request.Form["title"];
				return View["views/configsettings",new {pagetitle=title,settings=sets, configview=p.id}];
			};
			Get["/settings/addsetting"] = _ =>
			{
				var sets = _settings.GetSettings(-1);
				return View["views/addsetting",new {pagetitle="",settings=sets, configview=-1}];
			};
			Post["/settings/addsetting"] = _ =>
			{
				var sets = this.Bind<List<Setting>>();
				return _settings.CreateSetting(sets);
			};
			Get["/settings/editsetting/{key}"] = p =>
			{
				var sets = _settings.GetEditSetting(p.key);
				return View["views/addsetting",new {pagetitle="",settings=sets, configview=-1}];
			};
			Put["/settings/editsetting"] = _ =>
			{
				var sets = this.Bind<List<Setting>>();
				return _settings.UpdateSetting(sets);
			};
	        Post["/settings"] = _ =>
	        {
	        	var sets = this.Bind<List<Setting>>();
	        	return Response.AsJson(_settings.SaveSettings(sets));
	        };
			Delete["/settings/{key}"] = p =>
			{
				return _settings.DeleteSetting(p.key);
			};
            Get["/scheduler/start"] = _ =>
	        {
                _scheduler.Start();
                return "true";
	        };
	        Get["/scheduler/stop"] = _ =>
	        {
                _scheduler.Shutdown();
                return "true";
	        };
	        Get["/scheduler/restart"] = _ =>
	        {
                _scheduler.Shutdown();
				_scheduler.Start();
                return "true";
	        };
	        Get["/scheduler/trigger/{jobname}"] = p =>
	        {
	        	return _scheduler.TriggerJob(p.jobname).ToString();
	        };
	        Get["/scheduler/isrunning"] = _ =>
	        {
	        	return _scheduler.isStarted().ToString();
	        };
	        Get["/log/clear"] = _ =>
	        {
	        	_log.ClearLog();
	        	return new {success=true};
	        };
        }
    }
}
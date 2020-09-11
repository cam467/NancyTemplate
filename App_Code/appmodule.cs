using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using ApiBase.Interfaces;
using App.Models;
using CsvHelper;

namespace App.Modules
{
	public class Exporter : IExporter
	{
		private readonly IADExtensions _ad;
		private readonly IRepository _repo;
		private readonly IKronosApi _kronos;
		private readonly IApi _api;
		private readonly ILogs _log;
		private readonly ISettings _settings;
		public Exporter(IADExtensions ad, IRepository repo, IKronosApi kronos, IApi api, ILogs log, ISettings settings)
		{
			this._ad = ad;
			this._repo = repo;
			this._kronos = kronos;
			this._api = api;
			this._settings = settings;
			this._log = log;
		}
		public bool RunExport()
		{
			List<string> groups = _settings.GetSettingValue("admonitoradgroup").Split('|').ToList();
			string ouroot = _settings.GetSettingValue("adrootsearcher");
			List<User> users = _ad.GetAllADUsersForGroups(groups,ouroot);
			List<User> dbusers = _repo.GetUsers();
			var dbemails = dbusers.Select(x => x.email.ToLower()).ToList();
			var ademails = users.Select(x => x.email.ToLower()).ToList();
			//Do comparison and filter out users already in db with same email address
			var newusers = users.Where(x => !dbemails.Contains(x.email.ToLower())).ToList();
			var deleteusers = dbusers.Where(x => !ademails.Contains(x.email.ToLower())).ToList();
			dbemails = newusers.Select(x => x.email.ToLower()).ToList();
			ademails = deleteusers.Select(x => x.email.ToLower()).ToList();
			var updateusers = dbusers.Where(x => !dbemails.Contains(x.email.ToLower()) && !ademails.Contains(x.email.ToLower()));
			var uploadusers = new List<User>();
			//Get Users employee ids from kronos
			foreach (User user in newusers)
			{
				var kronosuser = _kronos.GetUser(user.first_name,user.last_name,user.email);
				if (!String.IsNullOrWhiteSpace(kronosuser.employee_number))
				{
					user.employee_number = kronosuser.employee_number.Trim();
					user.manager_name = kronosuser.manager_name.Trim();
					user.manager_email = kronosuser.manager_email.Trim();
					uploadusers.Add(user);
				}
				else
				{
					_log.NewLog("User " + user.first_name + " " + user.last_name + " is not being added");
				}
			}
			_log.NewLog("uploadusers:" + Newtonsoft.Json.JsonConvert.SerializeObject(uploadusers));
			// _api.AddUsers(uploadusers);
			_api.ArchiveUsers(deleteusers);
			//After upload, no need to add users to the db as they will import on next import automatically
			return true;
		}
	}

	public class Importer : IImporter
	{
		private readonly IApi _api;
		private readonly IRepository _repo;
		public Importer(IApi api, IRepository repo)
		{
			this._api = api;
			this._repo = repo;
		}

		public bool RunImport()
		{
			AddGroups();
			AddUsers();
			AddStorePurchases();
			AddEnrollments();
			AddCampaigns();
			AddAccount();
			return true;
		}
        public string TestConnection()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(_api.GetUsers());
		}

		public bool AddUsers()
		{
			var users = _api.GetUsers();
			return _repo.AddUsers(users);
		}
		public bool AddGroups()
		{
			var groups = _api.GetGroups();
			return _repo.AddGroups(groups);
		}
		public bool AddStorePurchases()
		{
			var purchases = _api.GetStorePurchases();
			return _repo.AddStorePurchases(purchases);
		}
		public bool AddEnrollments()
		{
			var enrollments = _api.GetEnrollments();
			return _repo.AddEnrollments(enrollments);
		}
		public bool AddCampaigns()
		{
			var campaigns = _api.GetCampaigns();
			return _repo.AddCampaigns(campaigns);
		}
		public bool AddAccount()
		{
			var account = _api.GetAccount();
			return _repo.AddAccount(account);
		}
	}
}
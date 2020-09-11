namespace ApiBase.Globals
{
	using System;
	using System.Net.Mail;
	using System.IO;
	using System.Collections.Generic;
	using System.Reflection;
	using System.Data;
	using System.Configuration;
	using RazorEngine;
	using RazorEngine.Templating;
	using RazorEngine.Configuration;
	using ApiBase.Interfaces;

    public class RazorService : IRazorService
	{
		private readonly IRazorEngineService _razor;
		private readonly IGlobal _global;
		public RazorService(IGlobal global)
		{
			this._global = global;
			var config = new TemplateServiceConfiguration();
			config.TemplateManager = new ResolvePathTemplateManager(new List<string>() {_global.templatesource});
			this._razor = RazorEngineService.Create(config);
		}

		public string RunCompile(string templatename, object model = null)
		{
			return _razor.RunCompile(templatename, null, model,null);
		}
	}

	public class Global : IGlobal
	{
		public string hpass 
		{
			get {
				return "ak3#r9391!D";
			}
			set {}
		}
		public string sqlitedb 
		{
			get {
				return "Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\app_data\\appdata.s3db";
			}
			set {}
		}
		public string binpath 
		{
			get {
				return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,"bin");
			}
			set {}
		}
		public string urllink 
		{
			get {
				return ConfigurationManager.AppSettings["urllink"];
			}
			set {}
		}
		public string mailhost 
		{
			get {
				return ConfigurationManager.AppSettings["smtphost"];
			}
			set {}
		}
		public string templatesource 
		{
			get {
				return Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase,"Views");
			}
			set {}
		}

		private readonly ILogs _log;

		public Global(ILogs log)
		{
			_log = log;	
		}

		public string SendEmail(string to, string subject, string body)
		{
		    try
		    {
		        using (MailMessage mail = new MailMessage())
		       	{
			        mail.Body = body;
			        mail.IsBodyHtml = true;
			        foreach (var t in to.Split(new [] {";"}, StringSplitOptions.RemoveEmptyEntries))
					{
					    mail.To.Add(t);
					}
			        mail.From = new MailAddress("noreply@buc-ees.com", "VenaExport (NoReply)", System.Text.Encoding.UTF8);
			        mail.Subject = subject;
			        mail.SubjectEncoding = System.Text.Encoding.UTF8;
			        mail.Priority = MailPriority.Normal;
			        using (SmtpClient smtp = new SmtpClient())
			        {
				        smtp.Host = mailhost;
				        smtp.Send(mail);
				    }
			    }
			    return "true";
		    }
		    catch (Exception ex)
		    {
		        _log.NewLog("Mail send error: " + ex.Source + "-" + ex.Message);
		        return ex.Source + "-" + ex.Message;
		    }
		}

		public Int64 GetTime()
		{
			Int64 retval=0;
			DateTime st=  new DateTime(1970,1,1);
			TimeSpan t= (DateTime.Now.ToUniversalTime()-st);
			retval= (Int64)(t.TotalMilliseconds+0.5);
			return retval;
		}

		public DataTable ConvertObjectToTable(object _class)
		{
			DataTable dt = new DataTable();
			PropertyInfo[] props = null;
			var coll = _class as IEnumerable<object>;
			foreach (var o in coll) {
				props = o.GetType().GetProperties();
				foreach (var prop in props) {
					dt.Columns.Add(prop.Name);
				}
				break;
			}
			foreach (var o in coll) {
				var r = dt.NewRow();
				foreach (var prop in props) {
					r[prop.Name] = prop.GetValue(o);
				}
				dt.Rows.Add(r);
			}
			return dt;
		}

		public static string GetUrlLink()
		{
			return ConfigurationManager.AppSettings["urllink"];
		}

		public static string GetHpass()
		{
			return "ak3#r9391!D";
		}

		public static string GetSqlLiteDb()
		{
			return "Data Source=" + AppDomain.CurrentDomain.BaseDirectory + "\\app_data\\appdata.s3db";
		}
	}
}
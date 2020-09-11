namespace ApiBase.Globals
{
    using System;
    using ApiBase.SettingParams;
    using ApiBase.Interfaces;
    using NLog;

    public class Log : ILogs
	{
		private Logger logger = LogManager.GetCurrentClassLogger();

		public string ClearLog()
		{
			try
			{
				SQLiteDatabase cn = new SQLiteDatabase();
				string s = "delete from nlog;";
				cn.ExecuteNonQuery(s);
				return "{\"success\":\"success\"}";
			}
			catch (Exception e)
			{
				logger.Error(e,"Logging Error");
				return "{\"error\":\"" + e.Message + "\"}";
			}
		}

		public void NewLog(string _description)
		{
			logger.Info(_description);
		}

		public void NewLog(Exception e, string description)
		{
			logger.Info(e,description);
		}
	}
}
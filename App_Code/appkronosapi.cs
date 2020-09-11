namespace App.API
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using Dapper;
    using App.Models;
    using ApiBase.Globals;
    using ApiBase.Interfaces;

    public class KronosApi : IKronosApi
    {
        private readonly ISettings _settings;
        private readonly ILogs _log;

        public KronosApi(ISettings settings, ILogs log)
        {
            _settings = settings;
            _log = log;
            SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);
        }

        private SqlConnection ConnectDatabase()
        {
            string a = _settings.GetSettingValue("dbkronosserver"),
                b = _settings.GetSettingValue("dbkronosdefaultdatabase"),
                c = _settings.GetSettingValue("dbkronostrusted"),
                d = _settings.GetSettingValue("dbkronosusername"),
                h = _settings.GetSettingValue("dbkronospassword"),
                f = d.IndexOf('\\') != -1 ? d.Split('\\')[0] : "",
                g = d.IndexOf('\\') != -1 ? d.Split('\\')[1] : d,
                e = "server=" + a + ";database=" + b + ";" + (c=="0" && !String.IsNullOrWhiteSpace(g) ? "User ID=" + g + ";Password=" + h + ";" : c=="1" ? "Trusted_Connection=True;" : "");

            SqlConnection db;

            if (c=="1" && !String.IsNullOrWhiteSpace(g))
            {
                try
                {
                    ImpersonateUser iu = new ImpersonateUser();
                    if (iu.impersonateValidUser(g, f, h))
                    {
                        db = new SqlConnection(e);
                        return db;
                        iu.undoImpersonation();
                    }
                }
                catch(Exception ex)
                {
                    _log.NewLog(ex.Message);
                }
            }
            
            db = new SqlConnection(e);
            return db;
        }

        public User GetUser(string firstname, string lastname, string email)
        {
            string sql1 = "select employeeid employee_number, firstname first_name, lastname last_name, personemail email, supervisorname manager_name, supervisoremail manager_email from bucdata.dbo.lf_employees_active_all_with_supers where lastname like @lastname and firstname like @firstname;",
                sql2 = "select employeeid employee_number, firstname first_name, lastname last_name, personemail email, supervisorname manager_name, supervisoremail manager_email from bucdata.dbo.lf_employees_active_all_with_supers where lastname like @lastname and personemail = @email;";

            using (SqlConnection cn = ConnectDatabase())
            {
                User user;
                try
                {
                    user = cn.QueryFirst<User>(sql1, new {lastname = "%" + lastname + "%",firstname = "%" + firstname + "%"});
                }
                catch
                {
                    try
                    {
                        user = cn.QueryFirst<User>(sql2, new {lastname = "%" + lastname + "%",email = email});
                    }
                    catch
                    {
                        user = new User();
                    }
                }
                return user;
            }
        }
    }
}
namespace App.Repositories
{
    using System;
    using Dapper;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Data.SqlClient;
    using ApiBase.Interfaces;
    using ApiBase.Globals;
    using App.Models;
    using System.Linq;
    
    public class DapperRepository : IRepository
    {
        private readonly ISettings _settings;
        private readonly ILogs _log;
        public DapperRepository(ISettings settings,ILogs log)
        {
            this._settings = settings;
            this._log = log;
            SqlMapper.AddTypeMap(typeof(DateTime), System.Data.DbType.DateTime2);
        }

        public SqlConnection ConnectDatabase()
        {
            string a = _settings.GetSettingValue("dbserver"),
                b = _settings.GetSettingValue("dbdefaultdatabase"),
                c = _settings.GetSettingValue("dbtrusted"),
                d = _settings.GetSettingValue("dbusername"),
                h = _settings.GetSettingValue("dbpassword"),
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

        public bool AddUsers (List<User> users)
        {
            string sqlins = "insert into users (id,employee_number,first_name,last_name,job_title,email,phish_prone_percentage,phone_number,location,division,manager_name,manager_email,adi_manageable,adi_guid,joined_on,last_sign_in,status,organization,department,employee_start_date,current_risk_score) values (@id,@employee_number,@first_name,@last_name,@job_title,@email,@phish_prone_percentage,@phone_number,@location,@division,@manager_name,@manager_email,@adi_manageable,@adi_guid,@joined_on,@last_sign_in,@status,@organization,@department,@employee_start_date,@current_risk_score);",
                sqlup = "update users set employee_number = @employee_number,first_name = @first_name,last_name = @last_name,job_title = @job_title,email = @email,phish_prone_percentage = @phish_prone_percentage,phone_number = @phone_number,location = @location,division = @division,manager_name = @manager_name,manager_email = @manager_email,adi_manageable = @adi_manageable,adi_guid = @adi_guid,joined_on = @joined_on,last_sign_in = @last_sign_in,status = @status,organization = @organization,department = @department,employee_start_date = @employee_start_date,current_risk_score = @current_risk_score where id = @id;",
                sqlgroups = "insert into user_groups (user_id,group_id) values (@user_id,@group_id);";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (User u in users)
                {
                    try
                    {
                        cn.Execute(sqlins,u);

                        //insert groups
                        foreach (int g in u.groups)
                        {
                            cn.Execute(sqlgroups, new {user_id = u.id, group_id = g});
                        }

                        //insert risk scores
                        AddRiskScores(u.risk_score_history,"u",u.id.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,u);
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddUsers) User " + u.id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddUsers) User " + u.id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }

            return true;
        }

        public bool AddGroups (List<KGroup> groups)
        {
            string sqlins = "insert into groups (id,name,group_type,adi_guid,member_count,current_risk_score,status) values (@id,@name,@group_type,@adi_guid,@member_count,@current_risk_score,@status);",
                sqlup = "update groups set name = @name,group_type = @group_type,adi_guid = @adi_guid,member_count = @member_count,current_risk_score = @current_risk_score,status = @status where id = @id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (KGroup g in groups)
                {
                    try
                    {
                        long i = (long)(g.id ?? g.group_id);
                        cn.Execute(sqlins,new {id = i,name = g.name,group_type = g.group_type,adi_guid = g.adi_guid,member_count = g.member_count,current_risk_score = g.current_risk_score,status = g.status});

                        //insert risk scores
                        AddRiskScores(g.risk_score_history,"g",i.ToString());
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                long i = (long)(g.id ?? g.group_id);
                                cn.Execute(sqlup,new {id = i,name = g.name,group_type = g.group_type,adi_guid = g.adi_guid,member_count = g.member_count,current_risk_score = g.current_risk_score,status = g.status});
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddGroups) Group " + (g.id != null ? g.id.ToString() : g.group_id.ToString()) + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddGroups) Group " + (g.id != null ? g.id.ToString() : g.group_id.ToString()) + " - " + ex.Message);
                        }
                    }
                }
            }

            return true;
        }

        public bool AddRiskScores (List<RiskScore> scores, string type, string id)
        {
            string sqlrisk = "insert into riskscores(score_type,score_id,risk_score,score_date) values (@score_type,@score_id,@risk_score,@score_date);";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (RiskScore r in scores)
                {
                    try
                    {
                        cn.Execute(sqlrisk,new {score_type = type, score_id = id, risk_score = r.risk_score, score_date = r.date});
                    }
                    catch (Exception ex)
                    {
                        _log.NewLog("(AddRiskScores): " + ex.Message);
                    }
                }
            }
            return true;
        }

        public bool AddStorePurchases(List<StorePurchase> storepurchases)
        {
            string sqlins = "insert into storepurchases (store_purchase_id,content_type,name,description,type,duration,retired,retirement_date,publish_date,publisher,purchase_date,policy_url) values (@store_purchase_id,@content_type,@name,@description,@type,@duration,@retired,@retirement_date,@publish_date,@publisher,@purchase_date,@policy_url);",
                sqlup = "update storepurchases set content_type = @content_type,name = @name,description = @description,type = @type,duration = @duration,retired = @retired,retirement_date = @retirement_date,publish_date = @publish_date,publisher = @publisher,purchase_date = @purchase_date,policy_url = @policy_url where store_purchase_id = @store_purchase_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (StorePurchase s in storepurchases)
                {
                    try
                    {
                        cn.Execute(sqlins,s);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,s);
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddStorePurchases Update) StorePurchase " + s.store_purchase_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("retirement date:" + s.retirement_date);
                            _log.NewLog("(AddStorePurchases Insert) StorePurchase " + s.store_purchase_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        public bool AddEnrollments(List<Enrollment> enrollments)
        {
            string sqlins = "insert into enrollments values (@enrollment_id,@content_type,@module_name,@campaign_name,@enrollment_date,@start_date,@completion_date,@status,@time_spent,@policy_acknowledged);",
                sqlup = "update enrollments set content_type = @content_type,module_name = @module_name,campaign_name = @campaign_name, enrollment_date = @enrollment_date, start_date = @start_date,completion_date = @completion_date,status = @status,time_spent = @time_spent,policy_acknowledged = @policy_acknowledged where enrollment_id = @enrollment_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (Enrollment en in enrollments)
                {
                    try
                    {
                        cn.Execute(sqlins,en);
                        AddEnrollmentUser(en.enrollment_id,en.user);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,en);
                                AddEnrollmentUser(en.enrollment_id,en.user);
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddEnrollments Update) Enrollment " + en.enrollment_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddEnrollments Insert) Enrollment " + en.enrollment_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        private bool AddEnrollmentUser(int enrollment_id, User user)
        {
            string sqlins = "insert into enrollment_user values (@enrollment_id,@user_id);",
                sqlup = "update enrollment_user set user_id = @user_id where enrollment_id = @enrollment_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                try
                {
                    cn.Execute(sqlins,new {enrollment_id = enrollment_id, user_id = user.id});
                }
                catch (Exception ex)
                {
                    if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                    {
                        try
                        {
                            cn.Execute(sqlup,new {enrollment_id = enrollment_id, user_id = user.id});
                        }
                        catch (Exception e)
                        {
                            _log.NewLog("(AddEnrollmentUser Update) Enrollment " + enrollment_id.ToString() + " - " + e.Message);
                        }
                    }
                    else
                    {
                        _log.NewLog("(AddEnrollmentUser Insert) Enrollment " + enrollment_id.ToString() + " - " + ex.Message);
                    }
                }
            }
            return true;
        }

        public bool AddCampaigns(List<Campaign> campaigns)
        {
            string sqlins = "insert into campaigns values (@campaign_id,@name,@status,@duration_type,@start_date,@end_date,@relative_duration,@auto_enroll,@allow_multiple_enrollments);",
                sqlup = "update campaigns set name = @name,status = @status,duration_type = @duration_type, start_date = @start_date, end_date = @end_date,relative_duration = @relative_duration,auto_enroll = @auto_enroll,allow_multiple_enrollments = @allow_multiple_enrollments where campaign_id = @campaign_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (Campaign c in campaigns)
                {
                    try
                    {
                        cn.Execute(sqlins,c);
                        AddCampaignGroups(c.campaign_id,c.groups);
                        AddCampaignModules(c.campaign_id,c.modules);
                        AddCampaignContent(c.campaign_id,c.content);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,c);
                                AddCampaignGroups(c.campaign_id,c.groups);
                                AddCampaignModules(c.campaign_id,c.modules);
                                AddCampaignContent(c.campaign_id,c.content);
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddCampaigns Update) Campaign " + c.campaign_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddCampaigns Insert) Campaign " + c.campaign_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        private bool AddCampaignGroups(long campaign_id, List<KGroup> groups)
        {
            string sqlins = "insert into campaign_groups values (@campaign_id,@group_id);",
                sqlup = "update campaign_groups set group_id = @group_id where campaign_id = @campaign_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (KGroup g in groups)
                {
                    try
                    {
                        cn.Execute(sqlins,new {campaign_id = campaign_id,group_id = g.group_id});
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,new {campaign_id = campaign_id,group_id = g.group_id});
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddCampaignGroups Update) Campaign " + campaign_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddCampaigns Insert) Campaign " + campaign_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        private bool AddCampaignModules(long campaign_id, List<StorePurchase> modules)
        {
            string sqlins = "insert into campaign_modules values (@campaign_id,@store_purchase_id);",
                sqlup = "update campaign_modules set store_purchase_id = @store_purchase_id where campaign_id = @store_purchase_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (StorePurchase m in modules)
                {
                    try
                    {
                        cn.Execute(sqlins,new {campaign_id = campaign_id,store_purchase_id = m.store_purchase_id});
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,new {campaign_id = campaign_id,store_purchase_id = m.store_purchase_id});
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddCampaignModules Update) Campaign " + campaign_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddCampaignModules Insert) Campaign " + campaign_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }

        private bool AddCampaignContent(long campaign_id, List<StorePurchase> content)
        {
            string sqlins = "insert into campaign_content values (@campaign_id,@store_purchase_id);",
                sqlup = "update campaign_content set store_purchase_id = @store_purchase_id where campaign_id = @store_purchase_id;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (StorePurchase c in content)
                {
                    try
                    {
                        cn.Execute(sqlins,new {campaign_id = campaign_id,store_purchase_id = c.store_purchase_id});
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,new {campaign_id = campaign_id,store_purchase_id = c.store_purchase_id});
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddCampaignContent Update) Campaign " + campaign_id.ToString() + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddCampaignContent Insert) Campaign " + campaign_id.ToString() + " - " + ex.Message);
                        }
                    }
                }
            }
            return true;
        }
        
        public bool AddAccount(Account account)
        {
            string sqlins = "insert into accounts values (@name,@type,@subscription_level,@subscription_end_date,@number_of_seats,@current_risk_score);",
                sqlup = "update accounts set type = @type,subscription_level = @subscription_level, subscription_end_date = @subscription_end_date, number_of_seats = @number_of_seats,current_risk_score = @current_risk_score where name = @name;";

            using (SqlConnection cn = ConnectDatabase())
            {
                    try
                    {
                        cn.Execute(sqlins,account);

                        //insert risk scores
                        AddRiskScores(account.risk_score_history,"a",account.name);
                        AddAccountDomains(account);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,account);
                                AddAccountDomains(account);
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddAccount Update) Account " + account.name + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddAccount Insert) Account " + account.name + " - " + ex.Message);
                        }
                    }
            }
            return true;
        }

        private bool AddAccountDomains(Account account)
        {
            string sqlins = "insert into account_domains values (@name,@domain_name);",
                sqlup = "update account_domains set domain_name = @domain_name where name = @name;";

            using (SqlConnection cn = ConnectDatabase())
            {
                foreach (string d in account.domains)
                    try
                    {
                        cn.Execute(sqlins,new {name = account.name,domain_name = d});

                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.IndexOf("insert duplicate key", StringComparison.OrdinalIgnoreCase)>=0)
                        {
                            try
                            {
                                cn.Execute(sqlup,new {name = account.name,domain_name = d});
                            }
                            catch (Exception e)
                            {
                                _log.NewLog("(AddAccountDomains Update) Account " + account.name + " - " + e.Message);
                            }
                        }
                        else
                        {
                            _log.NewLog("(AddAccountDomains Insert) Account " + account.name + " - " + ex.Message);
                        }
                    }
            }
            return true;
        }

        public List<User> GetUsers()
        {
            string sql = "select employee_number, first_name, last_name, email from users;";

            using (SqlConnection cn = ConnectDatabase())
            {
                List<User> users = cn.Query<User>(sql).ToList();
                return users;
            }
        }

        public List<KGroup> GetGroups()
        {
            return new List<KGroup>();
        }

        public List<Enrollment> GetEnrollments()
        {
            return new List<Enrollment>();
        }
    }
}
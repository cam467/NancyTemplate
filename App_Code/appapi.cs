
namespace App.API
{
    using System;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.IO;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using RestSharp;
    using Newtonsoft.Json;
    using ApiBase.Interfaces;
    using App.Models;
    using CsvHelper;
    
    public class AppApi : IApi
    {
        private RestClient c {get;set;}
        private HttpClient c2 {get;set;}
        private bool IsLoggedIn = false;
        private string authtoken {get;set;}
        private string importauthtoken {get;set;}
        private string apiurl;
		private string baseurl;
        private string token;
        private readonly ISettings _settings;
        private readonly ILogs _log;
        public AppApi(ISettings settings, ILogs log)
        {
            this._settings = settings;
            this._log = log;
            this.apiurl = settings.GetSettingValue("apiapiurl");
            this.baseurl = settings.GetSettingValue("apidomainurl");
            this.token = settings.GetSettingValue("apitoken");
            this.InitAppApi(this.baseurl);
        }

        private void InitAppApi(string baseurl)
        {
            c = new RestClient();
            c.Encoding = Encoding.UTF8;
            c.BaseUrl = new Uri(baseurl);
            c.CookieContainer = new CookieContainer();
            c.AddDefaultHeader("Authorization","Bearer " + this.token);
            c.AddDefaultHeader("ContentType","application/json");
        }

        private void Login()
        {
            string rstring = _settings.GetSettingValue("loginauthtokenregex"),
                url = _settings.GetSettingValue("loginbaseurl"),
                login = _settings.GetSettingValue("loginurl"),
                username = _settings.GetSettingValue("loginusername"),
                password = _settings.GetSettingValue("loginpassword"),
                importusersurl = _settings.GetSettingValue("loginimportusersurl");
            Regex reg = new Regex(rstring,RegexOptions.Singleline | RegexOptions.Multiline);

            this.c2 = new HttpClient();
            try
            {
                var response = this.c2.GetAsync(url + "/" + login).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                this.authtoken = reg.Match(content).Groups[1].Value;
                // Log.NewLog("authtoken=" + authtoken);
                // Log.NewLog("content=" + content);
                var loginobject = new List<KeyValuePair<string,string>>();
                loginobject.Add(new KeyValuePair<string,string>("utf8","%E2%9C%93"));
                loginobject.Add(new KeyValuePair<string,string>("authenticity_token",this.authtoken));
                loginobject.Add(new KeyValuePair<string,string>("commit","Sign in"));
                loginobject.Add(new KeyValuePair<string,string>("user[email]",username));
                loginobject.Add(new KeyValuePair<string,string>("user[password]",password));
                loginobject.Add(new KeyValuePair<string,string>("user[remember_me]","0"));
                var request = new HttpRequestMessage(HttpMethod.Post, url + "/" + login) { Content = new FormUrlEncodedContent(loginobject) };
                var response2 = c2.SendAsync(request).Result;
                //get authtoken for importing users
                var response3 = c2.GetAsync(url + "/" + importusersurl).Result;
                var content2 = response3.Content.ReadAsStringAsync().Result;
                this.importauthtoken = reg.Match(content2).Groups[1].Value;
                this.IsLoggedIn = true;
            }
            catch (Exception ex)
            {
                _log.NewLog("Login error: " + ex.Message);
            }
        }

        private T Execute<T>(RestRequest request) where T : new()
        {
            try
            {
                request.Resource = this.apiurl + "/" + request.Resource;
                // var rs = JsonConvert.DeserializeObject<T>(this.Execute(request,true).Content);
                var rs = c.Execute<T>(request);
                return rs.Data;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private IRestResponse Execute(RestRequest request, bool full)
        {
            try
            {
                request.Resource = this.apiurl + "/" + request.Resource;
                var rs = c.Execute(request);
                return rs;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string Execute(RestRequest request)
        {
            try
            {
                request.Resource = this.apiurl + "/" + request.Resource;
                return this.Execute(request,false).Content;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public bool UploadUserData(byte[] usersfile)
        {
            if (!this.IsLoggedIn) this.Login();
            string url = _settings.GetSettingValue("loginbaseurl"),
                addusers = _settings.GetSettingValue("loginaddusersurl");
            // using (var content = new MultipartFormDataContent("boundary=----" + DateTime.Now.ToString(CultureInfo.InvariantCulture)))
            using (var content = new MultipartFormDataContent())
            {
                content.Add(new StringContent("%E2%9C%93"),"utf8");
                content.Add(new StringContent(this.importauthtoken),"authenticity_token");
                content.Add(new ByteArrayContent(usersfile),"csv_file","records.csv");
                content.Add(new StringContent("none"),"admin_emails[]");
                content.Add(new StringContent(""),"user_emails");
                content.Add(new StringContent(""),"password");
                content.Add(new StringContent(""),"group_id");
                content.Add(new StringContent("Import Users"),"commit");
                // content.Headers.Clear();

                var request = new HttpRequestMessage(HttpMethod.Post,url + "/" + addusers);
                request.Content = content;
                try
                {
                    using (var message = this.c2.SendAsync(request).Result)
                    {
                        var response = message.Content.ReadAsStringAsync().Result;
                        // Log.NewLog("uploaddata=" + Regex.Match(response,@"<title>(.*?)<\/title>",RegexOptions.Multiline | RegexOptions.Singleline).Groups[1].Value);
                        return true;
                    }
                }
                catch(Exception ex)
                {
                    _log.NewLog("UploadUserData error: " + ex.Message);
                    return false;
                }
            }
        }

        public Account GetAccount()
        {
            var url = _settings.GetSettingValue("apiaccounturl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            try {
                // var u = JsonConvert.DeserializeObject<List<User>>(this.Execute(r,true).Content);
                var u = this.Execute<Account>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetAccount error:" + ex.Message);
                return new Account();
            }
        }

        public List<User> GetUsers()
        {
            var url = _settings.GetSettingValue("apiusersurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            r.AddQueryParameter("per_page","500");
            try {
                // var u = JsonConvert.DeserializeObject<List<User>>(this.Execute(r,true).Content);
                var u = this.Execute<List<User>>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetUsers error:" + ex.Message);
                return new List<User>();
            }
        }

        public User GetUser(int id)
        {
            var url = _settings.GetSettingValue("apiuserurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            r.AddParameter("user_id",id,ParameterType.UrlSegment);
            try {
                var u = this.Execute<User>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetUser error:" + ex.Message);
                return new User();
            }
        }

        public List<StorePurchase> GetStorePurchases()
        {
            var url = _settings.GetSettingValue("apitrainingstorepurchasesurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            try {
                var u = this.Execute<List<StorePurchase>>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetStorePurchases error:" + ex.Message);
                return new List<StorePurchase>();
            }
        }

        public StorePurchase GetStorePurchase(int id)
        {
            var url = _settings.GetSettingValue("apitrainingstorepurchaseurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            r.AddParameter("store_purchase_id",id,ParameterType.UrlSegment);
            try {
                var u = this.Execute<StorePurchase>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetStorePurchase error:" + ex.Message);
                return new StorePurchase();
            }
        }

        public List<KGroup> GetGroups()
        {
            var url = _settings.GetSettingValue("apigroupsurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            try {
                var u = this.Execute<List<KGroup>>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetGroups error:" + ex.Message);
                return new List<KGroup>();
            }
        }

        public List<Campaign> GetCampaigns()
        {
            var url = _settings.GetSettingValue("apitrainingcampaignsurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            try {
                var u = this.Execute<List<Campaign>>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetCampaigns error:" + ex.Message);
                return new List<Campaign>();
            }
        }

        public Campaign GetCampaign(long id)
        {
            var url = _settings.GetSettingValue("apitrainingcampaignurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            r.AddParameter("campaign_id",id,ParameterType.UrlSegment);
            try {
                var u = this.Execute<Campaign>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetCampaign error:" + ex.Message);
                return new Campaign();
            }
        }

        public List<Enrollment> GetEnrollments()
        {
            var url = _settings.GetSettingValue("apitrainingenrollmentsurl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            try {
                var u = this.Execute<List<Enrollment>>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetEnrollments error:" + ex.Message);
                return new List<Enrollment>();
            }
        }

        public Enrollment GetEnrollment(long id)
        {
            var url = _settings.GetSettingValue("apitrainingenrollmenturl");
            var r = new RestRequest {
                Method = Method.GET,
                Resource = url
            };
            r.AddParameter("enrollment_id",id,ParameterType.UrlSegment);
            try {
                var u = this.Execute<Enrollment>(r);
                return u;
            }
            catch (Exception ex)
            {
                _log.NewLog("GetEnrollment error:" + ex.Message);
                return new Enrollment();
            }
        }

        public bool AddUsers(List<User> users)
        {
            //need to load an extension method to convert IEnumerable<T> to CSV list and then copy it to a stream
            var url = _settings.GetSettingValue("apiaddusersurl");
            var r = new RestRequest
            {
                Method = Method.POST,
                Resource = url
            };
			MemoryStream ms = new MemoryStream();
            try
            {
                using (StreamWriter sw = new StreamWriter(ms))
                {
                    using (var csv = new CsvWriter(sw,CultureInfo.InvariantCulture))
                    {
                        csv.Configuration.RegisterClassMap<UserMap>();
                        csv.WriteRecords(users);
                    }
                }
            }
            catch(Exception ex)
            {
                _log.NewLog("AddUsers Error: " + ex.Message);
            }
			//upload to knowbe4
            this.UploadUserData(ms.ToArray());
            return true;
        }

        public bool ArchiveUsers(List<User> users)
        {
            var url = _settings.GetSettingValue("apiarchiveusersurl");
            var query = _settings.GetSettingValue("apiarchiveusersquery");
            if (!this.IsLoggedIn) this.Login();
            try
            {
                var archiveusers = new ArchiveUsers() {
                    query = query,
                    variables = new ArchiveUserIds { userIds = users.Select(x => (int)x.id).ToArray() }
                };
                using (var request = new HttpRequestMessage(HttpMethod.Post, url) { Content = new StringContent(JsonConvert.SerializeObject(archiveusers), Encoding.UTF8, "application/json") })
                {
                    var response = this.c2.SendAsync(request).Result;
                    if (response.StatusCode != HttpStatusCode.OK) _log.NewLog("ArchiveUsers status code returned a " + response.StatusCode.ToString());
                }
            }
            catch(Exception ex)
            {
                _log.NewLog("ArchiveUsers error: " + ex.Message);
            }
            return true;
        }
    }
}
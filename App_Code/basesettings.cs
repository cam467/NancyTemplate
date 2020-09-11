using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;
using System.Reflection;
using PetaPoco;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiBase.Interfaces;
using ApiBase.Models;

namespace ApiBase.SettingParams
{
    public class Settings : ISettings
    {
        private readonly ILogs _log;
        private readonly IGlobal _global;
        public Settings(ILogs log, IGlobal global)
        {
            _log = log;
            _global = global;
        }

        public bool DeleteSetting(string _key)
        {
            string sql = "delete from globalparams where glokey = @0;";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                try
                {
                    cn.Execute(sql,_key);
                    return true;
                }
                catch (System.Exception e)
                {
                    _log.NewLog("DeleteSetting error: " + e.Message);
                    return false;
                }
            }
        }

        public List<Setting> GetEditSetting(string _key)
        {
            List<Setting> sets = GetSettings(-1);
            Setting set = GetSetting(_key);
            foreach (Setting s in sets)
            {
                // s.value = set[s.name];
                switch (s.id)
                {
                    case "glokey":
                        s.value = set.id;
                        break;
                    case "gloname":
                        s.value = set.name;
                        break;
                    case "glotype":
                        s.value = set.type;
                        break;
                    case "glovalue":
                        s.value = set.value;
                        break;
                    case "glovalues":
                        s.value = set.values;
                        break;
                    case "glosection":
                        s.value = set.section.ToString();
                        break;
                    case "gloorder":
                        s.value = set.order.ToString();
                        break;
                }
            }
            return sets;
        }

        public bool UpdateSetting(List<Setting> setting)
        {
            string sql = "update globalparams set glokey = @0,gloname = @1,glovalue = @2,glosection = @3,glotype = @4,gloorder = @5,glovalues = @6 where glokey = @7;";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                Setting n = new Setting {
                    id = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glokey")).value),
                    name = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("gloname")).value),
                    value = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glovalue")).value),
                    section = int.Parse(HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glosection")).value)),
                    type = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glotype")).value),
                    order = int.Parse(HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("gloorder")).value)),
                    values = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glovalues")).value),
                    previousid = setting.First().previousid
                };
                if (n.type == "password") n.value = EncDec.Encrypt(n.value,_global.hpass);
                try
                {
                    //fix the order first then add
                    cn.Execute(sql,n.id,n.name,n.value,n.section,n.type,n.order,n.values,n.previousid);
                    return true;
                }
                catch (System.Exception e)
                {
                    _log.NewLog("UpdateSetting error: " + e.Message);
                    return false;
                }
            }
        }

        public bool CreateSetting(List<Setting> setting)
        {
            string sql = "insert into globalparams (glokey,gloname,glovalue,glosection,glotype,gloorder,glovalues) values (@0,@1,@2,@3,@4,@5,@6);";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                Setting n = new Setting {
                    id = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glokey")).value),
                    name = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("gloname")).value),
                    value = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glovalue")).value),
                    section = int.Parse(HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glosection")).value)),
                    type = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glotype")).value),
                    order = int.Parse(HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("gloorder")).value)),
                    values = HttpUtility.UrlDecode(setting.First(x => x.name.ToLower().Contains("glovalues")).value)
                };
                if (n.type == "password") n.value = EncDec.Encrypt(n.value,_global.hpass);
                try
                {
                    //fix the order first then add
                    cn.Execute(sql,n.id,n.name,n.value,n.section,n.type,n.order,n.values);
                    return true;
                }
                catch (System.Exception e)
                {
                    _log.NewLog("CreateSetting error: " + e.Message);
                    return false;
                }
            }
        }

        public DataTable GetTableFromJson(string _json)
        {
            var json = JToken.Parse(_json);
            DataTable table = new DataTable();
            if (json.Type!=JTokenType.Array) return table;
            var rows = json.Children();
            var cols = rows.First();

            //collect the columns and add
            foreach (var col in cols)
            {
                var prop = col.Value<JProperty>();
                table.Columns.Add(prop.Name, typeof(String));
            }
            
            //collect the row data
            foreach (var row in rows)
            {
                var drow = table.NewRow();
                var tokens = row.Children();
                foreach (var token in tokens)
                {
                    var prop = token.Value<JProperty>();
                    drow[prop.Name] = prop.Value;
                }
                table.Rows.Add(drow);
            }
            return table;
        }

        public string GetJsonFromTable(DataTable _table)
        {
            return JsonConvert.SerializeObject(_table);
        }

        public string GetSettingValue(string _key)
        {
            string sql = "select glokey name, glovalue value from globalparams where glokey = @0";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                var r = cn.Fetch<Setting>(sql,_key).DefaultIfEmpty(new Setting{id="",name="",type="",value="",table=null}).First();
                if (r.name.ToLower().Contains("password"))
                {
                    return EncDec.Decrypt(r.value, _global.hpass);
                }
                return r.value;
            }
        }

        public bool SaveSettingValue(string _key, string _value)
        {
            string sql = "update globalparams set glovalue = @1 where glokey = @0";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                try
                {
                    var r = cn.Execute(sql,_key,_value);
                    return true;
                }
                catch (Exception ex)
                {
                    _log.NewLog("SaveSettingValue error: " + ex.Message);
                    return false;
                }
            }
        }

        private List<Setting> ProcessSettings(List<Setting> settings)
        {
            SQLiteDatabase sq = new SQLiteDatabase();
            foreach (Setting r in settings)
            {
                switch (r.type)
                {
                    case "columnlist":
                    case "table":
                        Match m = Regex.Match(r.values??"",@"^(.*):");
                        if (m.Success && m.Groups.Count>0)
                        {
                            switch (m.Groups[1].Value)
                            {
                                case "query":
                                    r.table = sq.GetDataTable(r.values.Split(':')[1]);
                                    break;
                                case "function":
                                    /*This code block uses Reflection to dynamically load a class and call a method in that
                                    class based on a string. This string must have a format of: [namespace].[class]([constructor parameters]*note: no parenthesis for default constructor).[method]([method parameters or blank for none]*note: parenthesis are required). Optionally you can provide a name of a globalparam to map comma delimited values to the object by adding :mapping:valueofglobalparam to the end of above string. The method must return a 
                                    list of an object in order to be used. This list of object will then be converted to a 
                                    datatable to be used by the template and displayed.
                                    */
                                    Regex rx = new Regex(@"function:([\w\.]+)(?:(\(.*?\)))?\.(.*?)\((.*?)\)");
                                    var md = rx.Match(r.values);
                                    bool inst = md.Groups[2].Value.Contains("(");
                                    var cparms = Regex.Replace(md.Groups[2].Value,"[()]","").Split(',');
                                    var parms = md.Groups[4].Value.Split(',');
                                    //prepare method parameters if any. Returns empty string array if none
                                    for (int i=0;i<parms.Length;i++)
                                    {
                                        parms[i] = parms[i].Contains("password") ? EncDec.Decrypt(this.GetSettingValue(parms[i]),_global.hpass) : this.GetSettingValue(parms[i]);
                                    }
                                    //prepare constructor parameters if any. Return empty string array if none
                                    for (int i=0;i<cparms.Length;i++)
                                    {
                                        cparms[i] = cparms[i].Contains("password") ? EncDec.Decrypt(this.GetSettingValue(cparms[i]),_global.hpass) : this.GetSettingValue(cparms[i]);
                                    }
                                    //cast parameters to object array
                                    object[] parama = (object[])parms;
                                    object[] cparama = (object[])cparms;
                                    //check if the class is static or needs an object instance to call the method
                                    if (inst) 
                                    {
                                        Type ty = Type.GetType(md.Groups[1].Value);
                                        MethodInfo dispose = ty.GetMethod("Dispose");
                                        MethodInfo method = ty.GetMethod(md.Groups[3].Value);
                                        //this creates the object instance
                                        var _class = Activator.CreateInstance(ty,cparama);
                                        var _obj = method.Invoke(_class, parama);
                                        //this takes the object and converts it to a datatable
                                        r.table = _global.ConvertObjectToTable(_obj);
                                        if (dispose!=null) dispose.Invoke(_class,null);
                                    }
                                    else
                                    {
                                        //this code block is for static classes
                                        Type ty = Type.GetType(md.Groups[1].Value);
                                        MethodInfo mi = ty.GetMethod(md.Groups[3].Value);
                                        var _obj = mi.Invoke(ty,parama);
                                        r.table = _global.ConvertObjectToTable(_obj);
                                    }
                                    break;
                            }
                            // //Now check if value is empty and if not then begin mapping to first column
                            if (!string.IsNullOrWhiteSpace(r.value) && r.table!=null && r.table.Rows.Count>0)
                            {
                                string[] vals = null;
                                if (r.type == "columnlist")
                                {
                                    vals = r.value.Split(new string[] {"%3B"}, StringSplitOptions.None);
                                }
                                else
                                {
                                    vals = r.value.Split(',');
                                }
                                DataTable dt = r.table;
                                if (vals.Length>0)
                                {
                                    int iter = dt.Rows.Count > vals.Length ? vals.Length : dt.Rows.Count;
                                    for (int i=0;i<iter;i++)
                                    {
                                        dt.Rows[i][0] = vals[i];
                                    }
                                    r.table = dt;
                                }
                            }
                        }
                        break;
                    case "tableedit":
                        if (!String.IsNullOrWhiteSpace(r.value))
                        {
                            r.table = GetTableFromJson(r.value);
                        }
                        break;
                }
            }
            return settings;
        }

        public List<Setting> GetSettings(int _section)
        {
            string sql = "select glokey id, gloname name, glotype type, glovalue value, glovalues [values] from globalparams where gloactive = 1 and glosection = @0 order by gloorder";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                List<Setting> res = cn.Fetch<Setting>(sql,_section);
                if (res!=null)
                {
                    res = ProcessSettings(res);
                }
                return res;
            }
        }

        public Setting GetSetting(string _key)
        {
            string sql = "select glokey id, gloname name, glotype type, glovalue value, glosection section, glovalues [values], gloorder [order] from globalparams where glokey = @0;";
            using (Database cn = new Database(_global.sqlitedb,"SQLite"))
            {
                List<Setting> res = cn.Fetch<Setting>(sql,_key);
                if (res!=null)
                {
                    res = ProcessSettings(res);
                }
                return res.First();
            }
        }

        public bool SaveSettings(List<Setting> _settings)
        {
            string sql = "update globalparams set glovalue = @0 where glokey = @1",
                val;
            try
            {
                using (Database cn = new Database(_global.sqlitedb, "SQLite"))
                {
                    foreach(Setting s in _settings)
                    {
                        if (s.name.ToLower().Contains("password")) 
                        {
                            if (!s.value.Contains("password")) cn.Execute(sql,EncDec.Encrypt(HttpUtility.UrlDecode(s.value),_global.hpass),s.name);
                        }
                        else
                        {
                            cn.Execute(sql,HttpUtility.UrlDecode(s.value),s.name);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _log.NewLog(ex.Message);
                return false;
            }
            return true;
        }
    }
}
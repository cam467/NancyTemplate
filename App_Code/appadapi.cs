namespace App.API
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.DirectoryServices;
	using ApiBase.Interfaces;
    using App.Models;
    
    public class ADExtensions : IADExtensions
    {
		private readonly ILogs _log;
		public ADExtensions(ILogs log)
		{
			this._log = log;
		}

        public List<User> GetAllADUsersForGroups(List<string> _groups, string _ouroot)
        {
            DirectoryEntry entry = String.IsNullOrEmpty(_ouroot) ? new DirectoryEntry() : new DirectoryEntry(_ouroot);
	        DirectorySearcher search = new DirectorySearcher(entry);
	        if (String.IsNullOrEmpty(_ouroot))
			{
				string gt = "(memberOf={0})",
	        		gr = "({0}{1})",
	        		gl = "";
				// specify the search filter
				foreach (string g in _groups)
				{
					gl += String.Format(gt,g);
				}
	        	search.Filter = String.Format(gr,_groups.Count>1? "|" : "",gl);
			}
			else
			{
				string gt = "(objectCategory=person)";
				search.SearchScope = SearchScope.Subtree;
				search.Filter = gt;
			}
	        search.PropertiesToLoad.Add("givenName"); // first name
	        search.PropertiesToLoad.Add("sn"); // last name
	        search.PropertiesToLoad.Add("mail"); // smtp mail address
			List<User> users = new List<User>();
			try
			{
	        	SearchResultCollection rpc = search.FindAll();
				for (int i=0;i<rpc.Count;i++)
				{
					try
					{
						users.Add(new User {first_name=rpc[i].Properties["givenName"][0].ToString(),last_name=rpc[i].Properties["sn"][0].ToString(),email=rpc[i].Properties["mail"][0].ToString()});
					}
					catch
					{
					}
				}
			}
			catch (Exception ex)
			{
				_log.NewLog("GetAllADUsersForGroups error: " + ex.Message);
			}
	        // return rpc.Count.ToString();
	        return users;
        }
    }
}
using System;
using System.Collections.Generic;
using CsvHelper.Configuration;

namespace App.Models
{
	public class User
	{
		public long id { get; set; }
		public string employee_number { get; set; }
		public string first_name { get; set; }
		public string last_name { get; set; }
		public string job_title { get; set; }
		public string email { get; set; }
		public decimal phish_prone_percentage { get; set; }
		public string phone_number { get; set; }
		public string location { get; set; }
		public string division { get; set; }
		public string manager_name { get; set; }
		public string manager_email { get; set; }
		public string password { get; set; }
		public bool adi_manageable { get; set; }
		public string adi_guid { get; set; }
		public DateTime joined_on { get; set; }
		public DateTime? last_sign_in { get; set; }
		public string status { get; set; }
		public string organization { get; set; }
		public string department { get; set; }
        public List<int> groups { get; set; }
		public DateTime? employee_start_date { get; set; }
        public decimal current_risk_score { get; set; }
        public List<RiskScore> risk_score_history { get; set; }
	}

	public class LoginUser
	{
		public string email { get; set; }
		public string password { get; set; }
		public int remember_me { get; set; }
	}

	public class ArchiveUsers
	{
		public string query { get; set; }
		public ArchiveUserIds variables { get; set; }
	}

	public class ArchiveUserIds
	{
		public int[] userIds { get; set; }
	}

	public class Login
	{
		public string utf8 { get; set; }
		public string authenticity_token { get; set; }
		public LoginUser user { get; set; }
		public string commit { get; set; }
	}

    public class StorePurchase
	{
		public long store_purchase_id { get; set; }
		public string content_type { get; set; }
		public string name { get; set; }
		public string description { get; set; }
		public string type { get; set; }
		public int duration { get; set; }
		public bool retired { get; set; }
		public DateTime retirement_date { get; set; }
		public DateTime publish_date { get; set; }
		public string publisher { get; set; }
		public DateTime purchase_date { get; set; }
		public string policy_url { get; set; }
	}
	
	public class KGroup
	{
		public long? id { get; set; }
		public long? group_id { get; set; }
		public string name { get; set; }
		public string group_type { get; set; }
		public string adi_guid { get; set; }
		public int member_count { get; set; }
		public decimal current_risk_score { get; set; }
		public List<RiskScore> risk_score_history { get; set; }
		public string status { get; set; }
	}

	public class RiskScore
	{
		public double risk_score { get; set; }
		public DateTime date { get; set; }
	}
	
	public class Campaign
	{
		public long campaign_id { get; set; }
		public string name { get; set; }
		public List<KGroup> groups { get; set; }
		public string status { get; set; }
		public List<StorePurchase> modules { get; set; }
		public List<StorePurchase> content { get; set; }
		public string duration_type { get; set; }
		public DateTime start_date { get; set; }
		public DateTime end_date { get; set; }
		public string relative_duration { get; set; }
		public bool auto_enroll { get; set; }
		public bool allow_multiple_enrollments { get; set; }
	}

	public class Account
	{
		public string name { get; set; }
		public string type { get; set; }
		public List<string> domains { get; set; }
		public List<User> admins { get; set; }
		public string subscription_level { get; set; }
		public DateTime subscription_end_date { get; set; }
		public int number_of_seats { get; set; }
		public decimal current_risk_score { get; set; }
		public List<RiskScore> risk_score_history { get; set; }
	}

    public class Enrollment
    {
        public int enrollment_id { get; set; }
        public string content_type { get; set; }
        public string module_name { get; set; }
        public User user { get; set; }
        public string campaign_name { get; set; }
        public DateTime enrollment_date { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? completion_date { get; set; }
        public string status { get; set; }
        public int time_spent { get; set; }
        public bool policy_acknowledged { get; set; }
    }

	public class UserMap : ClassMap<User>
	{
		public UserMap()
		{
			Map(m => m.email).Name("Email");
			Map(m => m.first_name).Name("First Name");
			Map(m => m.last_name).Name("Last Name");
			Map(m => m.phone_number).Name("Phone Number");
			Map(m => m.employee_number).Name("Employee Number");
			Map(m => m.password).Name("Password");
			Map(m => m.adi_manageable).Name("AD Managed");
			Map(m => m.employee_start_date).Name("Employee Start Date");
		}
	}
}
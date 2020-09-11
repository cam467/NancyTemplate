namespace ApiBase.Interfaces
{
    using System;
    using System.Collections.Generic;
    using App.Models;

    public interface IImporter
    {
        bool RunImport();
        string TestConnection();
        bool AddUsers();
        bool AddGroups();
        bool AddStorePurchases();
        bool AddEnrollments();
        bool AddCampaigns();
        bool AddAccount();
    }

    public interface IExporter
    {
        bool RunExport();
    }

    public interface IApi
    {
        bool UploadUserData(byte[] usersfile);
        Account GetAccount();
        List<User> GetUsers();
        User GetUser(int id);
        List<StorePurchase> GetStorePurchases();
        StorePurchase GetStorePurchase(int id);
        List<KGroup> GetGroups();
        List<Campaign> GetCampaigns();
        Campaign GetCampaign(long id);
        List<Enrollment> GetEnrollments();
        Enrollment GetEnrollment(long id);
        bool AddUsers(List<User> users);
        bool ArchiveUsers(List<User> users);
    }

    public interface IKronosApi
    {
        User GetUser(string firstname, string lastname, string email);
    }

    public interface IADExtensions
    {
        List<User> GetAllADUsersForGroups(List<string> _groups,string _ouroot);
    }

    public interface IRepository
    {
        bool AddUsers(List<User> users);
        bool AddGroups(List<KGroup> groups);
        bool AddRiskScores(List<RiskScore> scores,string type,string id);
        bool AddStorePurchases(List<StorePurchase> storepurshases);
        bool AddEnrollments(List<Enrollment> enrollments);
        bool AddCampaigns(List<Campaign> campaigns);
        bool AddAccount(Account account);
        List<User> GetUsers();
        List<KGroup> GetGroups();
        List<Enrollment> GetEnrollments();
    }
}
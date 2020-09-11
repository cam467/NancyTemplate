namespace App.Controller
{
	using System;
	using System.Collections.Generic;
	using Nancy;
	using Nancy.ViewEngines.Razor;
	using Nancy.Extensions;
	using Nancy.Responses;
	using Nancy.ModelBinding;
	using ApiBase.Interfaces;

    public class AppController : NancyModule
    {
		private readonly IApi _api;
		private readonly IImporter _importer;
		private readonly IExporter _export;
        public AppController(IApi api, IImporter importer,IExporter export) : base("/api")
        {
			this._api = api;
			this._importer = importer;
			this._export = export;
            Nancy.Json.JsonSettings.MaxJsonLength = Int32.MaxValue;
			Get["/testsettings"] = _ =>
			{
				return Response.AsJson(_importer.TestConnection());
			};
			Get["/users"] = _ =>
			{
				return _api.GetUsers();
			};
			Get["/user/{id}"] = p =>
			{
				return _api.GetUser(p.id);
			};
			Get["/account"] = _ =>
			{
				return _api.GetAccount();
			};
			Get["/storepurchases"] = _ =>
			{
				return _api.GetStorePurchases();
			};
			Get["/storepurchase/{id}"] = p =>
			{
				return _api.GetStorePurchase(p.id);
			};
			Get["/groups"] = _ =>
			{
				return _api.GetGroups();
			};
			Get["/campaigns"] = _ =>
			{
				return _api.GetCampaigns();
			};
			Get["/campaign/{id}"] = p =>
			{
				return _api.GetCampaign(p.id);
			};
			Get["/enrollments"] = p =>
			{
				return _api.GetEnrollments();
			};
			Get["/enrollment/{id}"] = p =>
			{
				return _api.GetEnrollment(p.id);
			};
			Put["/users"] = _ =>
			{
				return _importer.AddUsers();
			};
			Put["/groups"] = _ =>
			{
				return _importer.AddGroups();
			};
			Put["/storepurchases"] = _ =>
			{
				return _importer.AddStorePurchases();
			};
			Put["/enrollments"] = _ =>
			{
				return _importer.AddEnrollments();
			};
			Put["/campaigns"] = _ =>
			{
				return _importer.AddCampaigns();
			};
			Put["/account"] = _ =>
			{
				return _importer.AddAccount();
			};
			Put["/importall"] = _ =>
			{
				return _importer.RunImport();
			};
			Put["/exportall"] = _ =>
			{
				return _export.RunExport();
			};
        }
    }
}
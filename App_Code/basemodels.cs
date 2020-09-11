namespace ApiBase.Models
{
    using System;
    using System.Data;
    
    public class Setting
    {
        public string id {get;set;}
        public string name {get;set;}
        public string type {get;set;}
        public string value {get;set;}
        public string values {get;set;}
        public DataTable table {get;set;}
        public int section {get;set;}
        public int order {get;set;}
        public string previousid {get;set;}
    }

    public class Job
    {
        public string jobid {get;set;}
        public string jobname {get;set;}
        public string jobcron {get;set;}
        public string jobclass {get;set;}
        public string jobparam {get;set;}
        public bool jobactive {get;set;}
    }
}
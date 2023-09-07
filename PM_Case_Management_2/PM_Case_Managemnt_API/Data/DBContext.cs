﻿using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_API.Models.Case;
using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.Models.Common;
using PM_Case_Managemnt_API.Models.PM;
using Task = PM_Case_Managemnt_API.Models.PM.Task;

namespace PM_Case_Managemnt_API.Data
{
    public class DBContext : DbContext

    {


        public DBContext(DbContextOptions<DBContext> options) : base(options) { }

        // organization
        public virtual DbSet<OrganizationProfile> OrganizationProfile { get; set; }

        public virtual DbSet<OrganizationBranch> OrganizationBranches { get; set; }

        public virtual DbSet<OrganizationalStructure> OrganizationalStructures { get; set; }

        //

        public DbSet<Employee> Employees { get; set; }
       // public DbSet<EmployeeStructures> EmployeesStructures { get; set; }


        // Archive
        public DbSet<Folder> Folder { get; set; }
        public DbSet<Row> Rows { get; set; }
        public DbSet<Shelf> Shelf { get; set; }

        // 

        public DbSet<ProgramBudgetYear> ProgramBudgetYears { get; set; }

        public DbSet<BudgetYear> BudgetYears { get; set; }
        
        public DbSet<UnitOfMeasurment> UnitOfMeasurment { get; set; }


        //Standardized Forms 

        public DbSet<StandrizedForm> StandrizedForms { get; set; }
        public DbSet<StandardizedFormDocuments> FormDocuments { get; set; }


        //case 

        public DbSet<Applicant> Applicants { get; set; }
        public DbSet<Appointement> Appointements { get; set; }
        public DbSet<AppointementWithCalender> AppointementWithCalender { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<CaseIssue> CaseIssues { get; set; }
        public DbSet<CaseAttachment> CaseAttachments { get; set; }
        public DbSet<CaseHistory> CaseHistories { get; set; }
        public DbSet<CaseHistoryAttachment> CaseHistoryAttachments { get; set; }
        public DbSet<CaseMessages> CaseMessages { get; set; }
        public DbSet<CaseType> CaseTypes { get; set; }
       // public DbSet<CaseForward> CaseForwards { get; set; }
        public DbSet<FileSetting> FileSettings { get; set; }
        public DbSet<FilesInformation> FilesInformations { get; set; }

        // pm 

        public DbSet<Programs> Programs { get; set; }
        public DbSet<Plan> Plans { get; set; }
        public DbSet<ActivityParent> ActivityParents { get; set; }
        public DbSet<Activity> Activities { get; set; }
        public DbSet<ActivityProgress> ActivityProgresses { get; set; }
        public DbSet<ActivityTargetDivision> ActivityTargetDivisions { get; set; }
        public DbSet<ActivityTerminationHistories> ActivityTerminationHistories { get; set; }
        public DbSet<Commitees> Commitees { get; set; }
        public DbSet<CommitesEmployees> CommiteEmployees { get; set; }
        public DbSet<EmployeesAssignedForActivities> EmployeesAssignedForActivities { get; set; }
        public DbSet<ProgressAttachment> ProgressAttachments { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskMembers> TaskMembers { get; set; }
        public DbSet<TaskMemo> TaskMemos { get; set; }
        public DbSet<TaskMemoReply> TaskMemoReplies { get; set; }

        public DbSet<QuarterSetting> QuarterSettings { get; set; }





    }
}

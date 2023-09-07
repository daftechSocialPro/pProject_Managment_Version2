﻿using PM_Case_Managemnt_API.Data;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.DTOS.Case;
using System.Text;
using PM_Case_Managemnt_API.Models.Common;
using System.Reflection.Metadata.Ecma335;
using PM_Case_Managemnt_API.Controllers.PM;
using Microsoft.AspNetCore.Mvc;

namespace PM_Case_Managemnt_API.Services.Common.Dashoboard
{
    public class DashboardService: IDashboardService
    {

        private readonly DBContext _dBContext;
        private Random rnd = new Random();

        List<ProjectList> ProjectLists = new List<ProjectList>();
        List<AboutToExpireProjects> aboutToExpireProjects = new List<AboutToExpireProjects>();

        public BudgetYear budget = new BudgetYear();
        public Guid structureId = Guid.Empty;
        List<progress_Strucure> ps = new List<progress_Strucure>();
        public DashboardService(DBContext context)
        {
            _dBContext = context;
        }
        public async Task<DashboardDto> GetPendingCase(string startat, string endat)
        {

            var allAffairps = _dBContext.Cases
                 .Include(a => a.CaseType)
                 .Include(a => a.Applicant)
                .Include(a => a.CaseHistories)
                            .Include(a => a.Employee.OrganizationalStructure)
                .Where(a =>
                a.CreatedAt.Month == DateTime.Now.Month);
            allAffairps = allAffairps.Where(x => x.AffairStatus != AffairStatus.Completed);

            //if (startAt != null)
            //    allAffairs = allAffairs.Where(x => x.CreatedDateTime >= startAt);
            //if (endAt != null)
            //    allAffairs = allAffairs.Where(x => x.CreatedDateTime <= endAt);

            var report = new List<TopAffairsViewmodel>();
            foreach (var affair in allAffairps.ToList())
            {
                var eachReport = new TopAffairsViewmodel();
                eachReport.CaseTypeTitle = affair.CaseType.CaseTypeTitle;
                eachReport.AffairNumber = affair.CaseNumber;
                eachReport.ApplicantName = affair.Applicant?.ApplicantName;
                eachReport.Subject = affair.LetterSubject;
                var firstOrDefault = _dBContext.CaseHistories.Include(x=>x.ToStructure).Where(x=>x.CaseId==affair.Id).OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();
                if (firstOrDefault != null)
                    eachReport.Structure = _dBContext.OrganizationalStructures.Find(firstOrDefault
                            .ToStructureId).StructureName;
                var affairHistory = affair.CaseHistories.OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();
                if (affairHistory != null)
                    eachReport.Employee = _dBContext.Employees.Find(affairHistory
                            .ToEmployeeId).FullName;
                eachReport.CreatedDateTime = affair.CreatedAt;
                var change = DateTime.Now.Subtract(eachReport.CreatedDateTime).TotalHours;
                var d = change / 24;
                d = Math.Round((Double)d, 2);
                eachReport.Elapstime = d;
                eachReport.Level = (change * 100) / affair.CaseType.Counter;
                report.Add(eachReport);

            }
            report = report.OrderByDescending(x => x.Level).ToList();



            if (!string.IsNullOrEmpty(startat))
            {
                string[] startDate = startat.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                DateTime MDateTime = Convert.ToDateTime(XAPI.EthiopicDateTime.GetGregorianDate(Int32.Parse(startDate[0]), Int32.Parse(startDate[1]), Int32.Parse(startDate[2])));
                report = report.Where(x => x.CreatedDateTime >= MDateTime).ToList();
            }

            if (!string.IsNullOrEmpty(endat))
            {

                string[] endDate = endat.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                DateTime MDateTime = Convert.ToDateTime(XAPI.EthiopicDateTime.GetGregorianDate(Int32.Parse(endDate[0]), Int32.Parse(endDate[1]), Int32.Parse(endDate[2])));
                report = report.Where(x => x.CreatedDateTime <= MDateTime).ToList();
            }


            DashboardDto dashboard = new DashboardDto();
            dashboard.pendingReports = report;




            allAffairps = _dBContext.Cases
                .Include(a => a.CaseType)
                 .Include(a => a.Applicant)
                  .Include(a => a.Employee)
                .Include(a => a.CaseHistories)
                .Where(a =>
                a.CreatedAt.Month == DateTime.Now.Month);
            allAffairps = allAffairps.Where(x => x.AffairStatus == AffairStatus.Completed);

            report = new List<TopAffairsViewmodel>();
            foreach (var affair in allAffairps.ToList())
            {
                var eachReport = new TopAffairsViewmodel();
                eachReport.CaseTypeTitle = affair.CaseType.CaseTypeTitle;
                eachReport.AffairNumber = affair.CaseNumber;
                eachReport.ApplicantName = affair.Applicant != null ? affair.Applicant.ApplicantName : affair.Employee.FullName;
                eachReport.Subject = affair.LetterSubject;
                var firstOrDefault = _dBContext.CaseHistories.Include(x => x.ToStructure).Where(x => x.CaseId == affair.Id).OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();
                if (firstOrDefault != null)
                    eachReport.Structure = _dBContext.OrganizationalStructures.Find(firstOrDefault
                            .ToStructureId).StructureName;
                var affairHistory = affair.CaseHistories.OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefault();
                if (affairHistory != null)
                    eachReport.Employee = _dBContext.Employees.Find(affairHistory
                            .ToEmployeeId).FullName;
                eachReport.CreatedDateTime = affair.CreatedAt;
                var change = firstOrDefault.CreatedAt.Subtract(eachReport.CreatedDateTime).TotalHours;
                var d = change / 24;
                d = Math.Round((Double)d, 2);
                eachReport.Elapstime = d;
                eachReport.Level = (change * 100) / affair.CaseType.Counter;
                report.Add(eachReport);

            }
            report = report.OrderByDescending(x => x.Level).ToList();

            if (!string.IsNullOrEmpty(startat))
            {
                string[] startDate = startat.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                DateTime MDateTime = Convert.ToDateTime(XAPI.EthiopicDateTime.GetGregorianDate(Int32.Parse(startDate[0]), Int32.Parse(startDate[1]), Int32.Parse(startDate[2])));
                report = report.Where(x => x.CreatedDateTime >= MDateTime).ToList();
            }

            if (!string.IsNullOrEmpty(endat))
            {

                string[] endDate = endat.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                DateTime MDateTime = Convert.ToDateTime(XAPI.EthiopicDateTime.GetGregorianDate(Int32.Parse(endDate[0]), Int32.Parse(endDate[1]), Int32.Parse(endDate[2])));
                report = report.Where(x => x.CreatedDateTime <= MDateTime).ToList();
            }

            dashboard.completedReports = report;


            var Chart = new CaseReportChartDto();

            Chart.labels = new List<string>() { "LateProgress", "completed"};
            Chart.datasets = new List<DataSets>();

            var datas = new DataSets();

            datas.data = new List<int>() { dashboard.pendingReports.Count(), dashboard.completedReports.Count()};
            datas.hoverBackgroundColor = new List<string>() { "#fe5e2b", "#2cb436" };
            datas.backgroundColor = new List<string>() { "#fe5e2b", "#2cb436" };

            Chart.datasets.Add(datas);


            dashboard.chart = Chart;





            return dashboard;

        }



        public async Task<barChartDto> GetMonthlyReport()
        {

            barChartDto barChart = new barChartDto();
            barChart.labels = new List<string>() { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "sep", "Oct", "Nov", "Dec" };
            barChart.datasets = new List<barChartDetailDto>();



            var allAffairs = _dBContext.Cases.Include(x=>x.CaseType).Where(x => x.CreatedAt.Year == DateTime.Now.Year).ToList();
            var allAffairTypes = _dBContext.CaseTypes.Where(x => x.RowStatus == RowStatus.Active && x.ParentCaseTypeId == null && x.CaseForm == CaseForm.Outside).ToList();
            var monthList = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
            foreach (var affairType in allAffairTypes)
            {


                barChartDetailDto dataset = new barChartDetailDto
                {
                    type = "bar",
                    label = affairType.CaseTypeTitle,
                    backgroundColor = String.Format("#{0:X6}", rnd.Next(0x1000000))
                };

                dataset.data = new List<int>();

                foreach (var month in monthList)
                {

                    dataset.data.Add(
                        allAffairs.Count(x => x.CaseTypeId == affairType.Id && x.CreatedAt.Month == month && x.CaseType.ParentCaseTypeId == null));

                 
                }
                barChart.datasets.Add(dataset);


            }

            return barChart;

        }


        public async Task<PMDashboardDto> GetPMDashboardDto(Guid empID)
        {
           
            var Employee =   _dBContext.Employees.Find(empID);
            var Structure_Hierarchy = _dBContext.OrganizationalStructures.Single(x => x.Id == Employee.OrganizationalStructureId);
            if (Structure_Hierarchy.ParentStructureId != null)
            {
                structureId = Structure_Hierarchy.Id;
            }
            var thisBudgetYear = _dBContext.BudgetYears.Single(x => x.RowStatus == RowStatus.Active);
            budget = thisBudgetYear;
            
            var prog = _dBContext.Programs.Where(x => x.RowStatus == RowStatus.Active && x.ProgramBudgetYearId == thisBudgetYear.ProgramBudgetYearId).ToList();
            var plans = _dBContext.Plans.Include(x=>x.Activities)
                .Include(x=>x.Tasks).ThenInclude(a => a.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.ActivitiesParents).ThenInclude(a => a.Activities).Where(x => x.RowStatus == RowStatus.Active && (x.BudgetYearId == thisBudgetYear.Id || x.Program.ProgramBudgetYearId == thisBudgetYear.ProgramBudgetYearId)).ToList();
            if (structureId != Guid.Empty)
            {
                // prog = prog.Where(x => x.StructureId == structureId).ToList();
                plans = plans.Where(x => x.StructureId == structureId).ToList();
            }

            ExpiredItems();
            PlansProgress();
            PMDashboardDto pMDashboard = new PMDashboardDto
            {
                CountPrograms = prog.Count(),
                CountBudget = plans.Sum(x => x.PlandBudget),
                CountUsedBudget = plans.Sum(x => x.Activities.Sum(y => y.ActualBudget)) + plans.Sum(x => x.Tasks.Sum(y => y.Activities.Sum(z => z.ActualBudget))) + plans.Sum(x => x.Tasks.Sum(y => y.ActivitiesParents.Sum(a => a.Activities.Sum(z => z.ActualBudget)))),
                TotalProjects = plans.Count(),
                TotalContribution = CommisionerPerformanceThisYear(),
                BudgetYear = thisBudgetYear.Year,
                ProjectLists = ProjectLists,
                AboutToExpireProjects = aboutToExpireProjects

            };

            return pMDashboard; 


        }
        public float CommisionerPerformanceThisYear()
        {
            float totalContribution = 0;
            ps = new List<progress_Strucure>();
            if (structureId == Guid.Empty)
            {
                var structu = _dBContext.OrganizationalStructures.Include(x=>x.SubTask).Where(x => x.ParentStructureId == null).FirstOrDefault();
                var structures = structu.SubTask;
                foreach (var structureRow in structures)
                {
                    var child = new Structure();
                    var attribute = new Attribute();
                    float performance = 0;
                    float contribution = 0;

                    float Pro_BeginingPlan = 0;
                    float Pro_ActualPlan = 0;
                    float Pro_Goal = 0;

                    var Plans = _dBContext.Plans.Include(x=>x.Program)
                        .Include(x => x.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.ActivitiesParents)
                .ThenInclude(a => a.Activities).Where(x => x.StructureId == structureRow.Id && (x.BudgetYear.Id == budget.Id || x.Program.ProgramBudgetYearId == budget.ProgramBudgetYearId));
                    foreach (var planItems in Plans)
                    {
                        float BeginingPlan = 0;
                        float ActualPlan = 0;
                        float Goal = 0;
                        var Tasks = planItems.Tasks;
                        if (!Tasks.Any() && !planItems.Activities.Any())
                        {
                            Pro_Goal = Pro_Goal + Plans.Sum(x => x.PlanWeight);
                        }
                        else if (planItems.Activities.Any())
                        {
                            Pro_Goal = Pro_Goal + ((planItems.Activities.First().Goal * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                        }
                        foreach (var taskItems in Tasks)
                        {
                            var Activities = taskItems.ActivitiesParents;
                            float BeginingPercent = 0;
                            float ActualWorkedPercent = 0;
                            float GoalPercent = 0;
                            if (!Activities.Any() && !taskItems.Activities.Any())
                            {
                                Goal = Goal + planItems.PlanWeight;
                            }
                            else if (taskItems.Activities.Any())
                            {
                                Goal = Goal + ((taskItems.Activities.First().Goal * (float)taskItems.Weight) / planItems.PlanWeight);
                            }
                            foreach (var activityPItems in Activities)
                            {
                                foreach (var activityItems in activityPItems.Activities)
                                {
                                    BeginingPercent = BeginingPercent + ((activityItems.Begining * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                                    ActualWorkedPercent = ActualWorkedPercent + ((activityItems.ActualWorked * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                                    GoalPercent = GoalPercent + ((activityItems.Goal * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                                }
                            }
                            float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                            BeginingPlan = BeginingPlan + ((BeginingPercent * taskItemsWeight) / planItems.PlanWeight);
                            ActualPlan = ActualPlan + ((ActualWorkedPercent * taskItemsWeight) / planItems.PlanWeight);
                            Goal = Goal + ((GoalPercent * taskItemsWeight) / planItems.PlanWeight);
                        }
                        Pro_BeginingPlan = Pro_BeginingPlan + ((BeginingPlan * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                        Pro_ActualPlan = Pro_ActualPlan + ((ActualPlan * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                        Pro_Goal = Pro_Goal + ((Goal * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                    }
                    if (Pro_ActualPlan > 0)
                    {
                        if (Pro_ActualPlan == Pro_Goal)
                        {
                            contribution = 100;
                        }
                        else
                        {
                            float Nominator = Pro_ActualPlan;
                            float Denominator = Pro_Goal;
                            contribution = (Nominator / Denominator) * 100;
                        }
                    }
                    else contribution = 0;
                    // Progress.Progress = Progress.Progress = ((ActualPlan - BeginingPlan) / (Progress.Weight - BeginingPlan)) * 100; ;
                    performance = ((float)(contribution));
                    contribution = (performance * structu.Weight) / 100;
                    performance = (float)Math.Round(performance, 2);
                    contribution = (float)Math.Round(contribution, 2);
                    totalContribution = totalContribution + contribution;
                    progress_Strucure pp = new progress_Strucure
                    {
                        structureName = structureRow.StructureName,
                        structureWeight = structureRow.Weight,
                        structureContribution = contribution,
                        structurePerformance = performance

                    };

                    ps.Add(pp);
                }
            }
            else
            {
                var structu = _dBContext.OrganizationalStructures.FirstOrDefault(x => x.Id == structureId);

                var child = new Structure();
                var attribute = new Attribute();
                float performance = 0;
                float contribution = 0;
                //var programs = Db.Programs.Where(x => x.BudgetYear.FromDate >= budget.FromDate && x.BudgetYear.ToDate <= budget.ToDate);

                float Progress = 0;

                float Pro_BeginingPlan = 0;
                float Pro_ActualPlan = 0;
                float Pro_Goal = 0;

                var Plans = _dBContext.Plans.Include(x=>x.Program)
                    .Include(x => x.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.ActivitiesParents).ThenInclude(a => a.Activities).Where(x => x.StructureId == structu.Id && (x.BudgetYear.Id == budget.Id || x.Program.ProgramBudgetYearId == budget.ProgramBudgetYearId))
                .ToList();
                foreach (var planItems in Plans)
                {
                    float BeginingPlan = 0;
                    float ActualPlan = 0;
                    float Goal = 0;
                    var Tasks = planItems.Tasks.ToList();
                    if (!Tasks.Any() && !planItems.Activities.Any())
                    {
                        Pro_Goal = Pro_Goal + Plans.Sum(x => x.PlanWeight);
                    }
                    else if (planItems.Activities.Any())
                    {
                          Pro_Goal = Pro_Goal + ((planItems.Activities.FirstOrDefault().Goal * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                    }
                    foreach (var taskItems in Tasks)
                    {
                        var Activities = taskItems.ActivitiesParents.ToList();
                        float BeginingPercent = 0;
                        float ActualWorkedPercent = 0;
                        float GoalPercent = 0;
                        if (!Activities.Any() && !taskItems.Activities.Any())
                        {
                            Goal = Goal + planItems.PlanWeight;
                        }
                        else if (taskItems.Activities.Any())
                        {
                            if (taskItems.Weight != null)
                            {
                                Goal = Goal + ((taskItems.Activities.FirstOrDefault().Goal * (float)taskItems.Weight) / planItems.PlanWeight);
                            }
                            else
                            {
                                Goal += 0;
                            }
                        }
                        foreach (var activityPItems in Activities)
                        {
                            foreach (var activityItems in activityPItems.Activities)
                            {
                                BeginingPercent = BeginingPercent + ((activityItems.Begining * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                                ActualWorkedPercent = ActualWorkedPercent + ((activityItems.ActualWorked * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                                GoalPercent = GoalPercent + ((activityItems.Goal * activityItems.Weight) / activityPItems.Activities.Sum(x => x.Weight));
                            }
                        }
                        float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                        BeginingPlan = BeginingPlan + ((BeginingPercent * taskItemsWeight) / planItems.PlanWeight);
                        ActualPlan = ActualPlan + ((ActualWorkedPercent * taskItemsWeight) / planItems.PlanWeight);
                        Goal = Goal + ((GoalPercent * taskItemsWeight) / planItems.PlanWeight);
                    }
                    Pro_BeginingPlan = Pro_BeginingPlan + ((BeginingPlan * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                    Pro_ActualPlan = Pro_ActualPlan + ((ActualPlan * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                    Pro_Goal = Pro_Goal + ((Goal * (float)planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                }
                if (Pro_ActualPlan > 0)
                {
                    if (Pro_ActualPlan == Pro_Goal)
                    {
                        Progress = 100;
                    }
                    else
                    {
                        float Nominator = Pro_ActualPlan;
                        float Denominator = Pro_Goal;
                        Progress = (Nominator / Denominator) * 100;
                    }
                }
                else Progress = 0;
                // Progress.Progress = Progress.Progress = ((ActualPlan - BeginingPlan) / (Progress.Weight - BeginingPlan)) * 100; ;
                performance = ((float)(Progress));
                contribution = (performance * structu.Weight) / 100;
                performance = (float)Math.Round(performance, 2);
                contribution = (float)Math.Round(contribution, 2);

                totalContribution = performance;
                progress_Strucure pp = new progress_Strucure
                {
                    structureName = structu.StructureName,
                    structureWeight = structu.Weight,
                    structureContribution = contribution,
                    structurePerformance = performance

                };

                ps.Add(pp);

            }

            return totalContribution;

        }
        public void ExpiredItems()
        {
            var BudgetYear = _dBContext.BudgetYears.Single(x => x.RowStatus == RowStatus.Active);
            int Month = XAPI.EthiopicDateTime.GetEthiopicMonth(DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
            if (Month == 13)
            {
                Month = 12;
            }
            float i = Month;
            if (i == 12)
            {
                i = 2;
            }
            else if (i == 11)
            {
                i = 1;
            }
            else
            {
                i = i + 2;
            }
            // var quarterset = Db.QuarterSettings.Single(x => x.StartMonth == Month || x.EndMonth == Month || x.StartMonth == Month - 1);


            var AboutToExpireList = _dBContext.ActivityTargetDivisions
                .Include(x => x.Activity).ThenInclude(a => a.Plan).ThenInclude(k => k.Structure)
                .Include(x => x.Activity).ThenInclude(a => a.Task).ThenInclude(p => p.Plan).ThenInclude(s => s.Structure)
                .Include(x => x.Activity).ThenInclude(a => a.ActivityParent)
                .ThenInclude(a => a.Task).ThenInclude(p => p.Plan).ThenInclude(s => s.Structure)
                .Where(x => x.Order < i && x.Target != 0).ToList();
            if (structureId != Guid.Empty)
            {
                AboutToExpireList = AboutToExpireList.Where(x => (x.Activity.ActivityParentId != null ? x.Activity.ActivityParent.Task.Plan.StructureId : x.Activity.TaskId != null ? x.Activity.Task.Plan.StructureId : x.Activity.Plan.StructureId) == structureId).ToList();
            }
            foreach (var items in AboutToExpireList)
            {
                int h = 0;
                if (items.Order >= 7)
                {
                    h = items.Order - 6;
                }

                else
                {
                    h = items.Order + 6;
                }

                System.Globalization.DateTimeFormatInfo mfi = new
                    System.Globalization.DateTimeFormatInfo();
                string strMonthName = mfi.GetMonthName(h).ToString();
                AboutToExpireProjects aboutToExpire = new AboutToExpireProjects();
                var Progress = _dBContext.ActivityProgresses.Where(x => x.QuarterId == items.Id);
                if (Progress.Any())
                {
                    if (Progress.Sum(x => x.ActualWorked) < items.Target)
                    {
                        aboutToExpire.DirectorName = items.Activity.Plan != null ? items.Activity.Plan.Structure.StructureName : items.Activity.Task != null ? items.Activity.Task.Plan.Structure.StructureName : items.Activity.ActivityParent.Task.Plan.Structure.StructureName;
                        aboutToExpire.ProjectName = items.Activity.Plan != null ? items.Activity.Plan.PlanName : items.Activity.Task != null ? items.Activity.Task.Plan.PlanName : items.Activity.ActivityParent.Task.Plan.PlanName;
                        aboutToExpire.ActivityName = items.Activity.ActivityDescription == null ? items.Activity.ActivityParent.ActivityParentDescription : items.Activity.ActivityDescription;
                        aboutToExpire.CurrentAchivement = Progress.Sum(x => x.ActualWorked);
                        aboutToExpire.PlanMonthName = strMonthName;
                        aboutToExpire.RequiredAchivement = items.Target;
                        aboutToExpireProjects.Add(aboutToExpire);
                    }
                }
                else
                {
                    aboutToExpire.DirectorName = items.Activity.Plan != null ? items.Activity.Plan.Structure.StructureName : items.Activity.Task != null ? items.Activity.Task.Plan.Structure.StructureName : items.Activity.ActivityParent.Task.Plan.Structure.StructureName;
                    aboutToExpire.ProjectName = items.Activity.Plan != null ? items.Activity.Plan.PlanName : items.Activity.Task != null ? items.Activity.Task.Plan.PlanName : items.Activity.ActivityParent.Task.Plan.PlanName;
                    aboutToExpire.ActivityName = items.Activity.ActivityDescription == null ? items.Activity.ActivityParent.ActivityParentDescription : items.Activity.ActivityDescription;
                    aboutToExpire.CurrentAchivement = 0;
                    aboutToExpire.PlanMonthName = strMonthName;
                    aboutToExpire.RequiredAchivement = items.Target;
                    aboutToExpireProjects.Add(aboutToExpire);
                }
            }

        }

        public void PlansProgress()
        {
            var Plans = _dBContext.Plans
                .Include(x=>x.BudgetYear)
                .Include(x => x.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.Activities)
                .Include(x => x.Tasks).ThenInclude(a => a.ActivitiesParents).ThenInclude(a => a.Activities).Where(x => x.RowStatus == RowStatus.Active && x.BudgetYearId == budget.Id || x.BudgetYear.ProgramBudgetYearId == budget.ProgramBudgetYearId).ToList();
            if (structureId != Guid.Empty)
            {
                Plans = Plans.Where(x => x.StructureId == structureId).ToList();
            }
            float ProgressThisYear = 0;
            float OverallProgress = 0;

            float BeginingPlan = 0;
            float ActualPlan = 0;
            float Goal = 0;
            foreach (var planItems in Plans)
            {
                ProgressThisYear = 0;
                OverallProgress = 0;
                BeginingPlan = 0;
                ActualPlan = 0;
                Goal = 0;
                var Tasks = planItems.Tasks.ToList();
                if (planItems.HasTask && planItems.Tasks.Any())
                {
                    foreach (var taskItems in Tasks)
                    {
                        if (!taskItems.HasActivityParent && taskItems.Activities.Any()&&taskItems.Weight!=null)
                        {
                            BeginingPlan = BeginingPlan + ((taskItems.Activities.FirstOrDefault().Begining * (float)taskItems.Weight) / planItems.PlanWeight);
                            ActualPlan = ActualPlan + ((taskItems.Activities.FirstOrDefault().ActualWorked * (float)taskItems.Weight) / planItems.PlanWeight);

                            var taskGoal = taskItems.Activities.FirstOrDefault().Goal;

                            Goal = planItems.PlanWeight == 0 ? Goal : Goal + (((float)taskGoal * (float)taskItems.Weight) / planItems.PlanWeight);
                            OverallProgress = (OverallProgress + ((taskItems.Activities.FirstOrDefault().ActualWorked + taskItems.Activities.FirstOrDefault().Begining) * (float)taskItems.Weight) / planItems.PlanWeight);
                        }
                        else if (taskItems.ActivitiesParents.Any())
                        {
                            var Activities = taskItems.ActivitiesParents;
                            float BeginingPercent = 0;
                            float ActualWorkedPercent = 0;
                            float GoalPercent = 0;
                            float oProgress = 0;
                            if (!Activities.Any())
                            {
                                Goal = Goal + planItems.PlanWeight;
                            }
                            foreach (var activityItems in Activities)
                            {
                                foreach (var subActs in activityItems.Activities)
                                {
                                    BeginingPercent = BeginingPercent + (((float)subActs.Begining * subActs.Weight) / (float)activityItems.Weight);
                                    ActualWorkedPercent = ActualWorkedPercent + (((float)subActs.ActualWorked * subActs.Weight) / (float)activityItems.Weight);
                                    GoalPercent = GoalPercent + ((subActs.Goal * subActs.Weight) / (float)activityItems.Weight);
                                    oProgress = BeginingPercent + ActualWorkedPercent;
                                }

                            }
                            float taskItemsWeight = taskItems.Weight == null ? 0 : (float)taskItems.Weight;
                            BeginingPlan = BeginingPlan + ((BeginingPercent * taskItemsWeight) / planItems.PlanWeight);
                            ActualPlan = ActualPlan + ((ActualWorkedPercent * taskItemsWeight) / planItems.PlanWeight);
                            Goal = planItems.PlanWeight == 0 ? Goal : Goal + ((GoalPercent * taskItemsWeight) / planItems.PlanWeight);
                            OverallProgress = (OverallProgress + (oProgress * taskItemsWeight) / planItems.PlanWeight);
                        }
                    }
                }
                else if (planItems.Activities.Any())
                {
                    Goal = Goal + (float)planItems.Activities.First().Goal;
                    BeginingPlan = BeginingPlan + (float)planItems.Activities.First().Begining;
                    ActualPlan = ActualPlan + (float)planItems.Activities.First().ActualWorked;
                    OverallProgress = OverallProgress + ((BeginingPlan + ActualPlan) / planItems.PlanWeight);
                }
                if (ActualPlan > 0)
                {
                    if (ActualPlan == Goal)
                    {
                        ProgressThisYear = 100;
                        OverallProgress = 100;
                    }
                    else
                    {
                        float Nominator = ActualPlan;
                        float Denominator = Math.Abs((float)BeginingPlan - Goal);
                        ProgressThisYear = (Nominator / Denominator) * 100;
                    }
                }
                else ProgressThisYear = 0;
                var projectDuration = "";
                if (planItems.BudgetYearId == budget.Id)
                {
                    projectDuration = XAPI.EthiopicDateTime.GetEthiopicMonth(planItems.BudgetYear.FromDate.Day, planItems.BudgetYear.FromDate.Month, planItems.BudgetYear.FromDate.Year) + "/" +
                                               XAPI.EthiopicDateTime.GetEthiopicYear(planItems.BudgetYear.FromDate.Day, planItems.BudgetYear.FromDate.Month, planItems.BudgetYear.FromDate.Year) + "-" +
                                               XAPI.EthiopicDateTime.GetEthiopicMonth(planItems.BudgetYear.ToDate.Day, planItems.BudgetYear.ToDate.Month, planItems.BudgetYear.ToDate.Year) + "/" +
                                               XAPI.EthiopicDateTime.GetEthiopicYear(planItems.BudgetYear.ToDate.Day, planItems.BudgetYear.ToDate.Month, planItems.BudgetYear.ToDate.Year);
                }
                else if (planItems.Program != null)
                {
                    projectDuration = XAPI.EthiopicDateTime.GetEthiopicMonth(planItems.BudgetYear.FromDate.Day, planItems.BudgetYear.FromDate.Month, planItems.BudgetYear.FromDate.Year) + "/" +
                                               XAPI.EthiopicDateTime.GetEthiopicYear(planItems.BudgetYear.FromDate.Day, planItems.BudgetYear.FromDate.Month, planItems.BudgetYear.FromDate.Year) + "-" +
                                               XAPI.EthiopicDateTime.GetEthiopicMonth(planItems.BudgetYear.ToDate.Day, planItems.BudgetYear.ToDate.Month, planItems.BudgetYear.ToDate.Year) + "/" +
                                               XAPI.EthiopicDateTime.GetEthiopicYear(planItems.BudgetYear.ToDate.Day, planItems.BudgetYear.ToDate.Month, planItems.BudgetYear.ToDate.Year);
                }
                
                ProjectList project = new ProjectList()
                {
                    DirectorateName = planItems?.Structure?.StructureName,
                    ProjectName = planItems?.PlanName,
                    ProjectProgress = (float)Math.Round(ProgressThisYear, 2),
                    OverallProgress = Goal != 0 ?(float)Math.Round((ActualPlan / Goal) * 100, 2):0,
                    ProjectDuration = projectDuration,
                    Weight = planItems.PlanWeight

                };
                ProjectLists.Add(project);
            }
        }



        public async Task<PmDashboardBarchartDto> BudgetYearVsContribution(Guid empID)
        {

            var Employee = _dBContext.Employees.Find(empID);
            var Structure_Hierarchy = _dBContext.OrganizationalStructures.Single(x => x.Id == Employee.OrganizationalStructureId);
            if (Structure_Hierarchy.ParentStructureId != null)
            {
                structureId = Structure_Hierarchy.Id;
            }
            PmDashboardBarchartDto bugetYears = new PmDashboardBarchartDto();
            bugetYears.labels = new List<string>();
            bugetYears.datasets = new List<PmDashboardBarchartDateset>();
            var dataset1 = new PmDashboardBarchartDateset
            {
                label = "budget year contributon graph",
                data = new List<float>(),
                backgroundColor= new List<string>(),
                borderColor= new List<string>(),
                borderWidth=1

            };
            bugetYears.datasets.Add(dataset1);

            var BudgetArea = _dBContext.BudgetYears.OrderBy(x => x.Year).ToList();
            var structu = _dBContext.OrganizationalStructures.Include(x=>x.SubTask).Where(x => x.ParentStructureId == null).FirstOrDefault();
            var structures = structu.SubTask;
            if (structureId != Guid.Empty)
            {
                structures = structures.Where(x => x.Id == structureId).ToList();
            }

            foreach (var BudgetItems in BudgetArea)
            {

                //BugetYearContribution bjt = new BugetYearContribution();
                float totalContribution = 0;
                foreach (var structureRow in structures)
                {
                    float performance = 0;
                    float contribution = 0;
                    // var programs = Db.Programs.Where(x =>  x.BudgetYear.FromDate >= BudgetItems.FromDate && x.BudgetYear.ToDate <= BudgetItems.ToDate);


                    float BeginingPlan = 0;
                    float ActualPlan = 0;
                    float Goal = 0;
                    float Progress = 0;
                    var Plans = _dBContext.Plans.Include(x=>x.Program).Where(x => x.StructureId == structureRow.Id && (x.Program.ProgramBudgetYearId == BudgetItems.ProgramBudgetYearId || x.BudgetYearId == BudgetItems.Id));
                    foreach (var planItems in Plans)
                    {
                        float BeginingPercent = 0;
                        float ActualWorkedPercent = 0;
                        float GoalPercent = 0;
                        var Activities = (from x in _dBContext.Activities
                                          join z in _dBContext.Plans on x.PlanId equals z.Id
                                          where z.Id == planItems.Id
                                          select new { x }).ToList();
                        var ActivitiesT = (from x in _dBContext.Activities
                                           join y in _dBContext.Tasks on x.TaskId equals y.Id
                                           where y.PlanId == planItems.Id
                                           select new { x }).ToList();
                        var ActivitiesPAct = (from x in _dBContext.Activities
                                              join a in _dBContext.ActivityParents on x.ActivityParentId equals a.Id
                                              where a.Task.PlanId == planItems.Id
                                              select new { x }).ToList();
                        Activities.AddRange(ActivitiesT);
                        Activities.AddRange(ActivitiesPAct);
                        foreach (var activityItems in Activities)
                        {
                            BeginingPercent = Activities.Sum(x => x.x.Weight) == 0 ? BeginingPercent : BeginingPercent + ((activityItems.x.Begining * activityItems.x.Weight) / Activities.Sum(x => x.x.Weight));
                            ActualWorkedPercent = Activities.Sum(x => x.x.Weight) == 0 ? BeginingPercent : ActualWorkedPercent + ((activityItems.x.ActualWorked * activityItems.x.Weight) / Activities.Sum(x => x.x.Weight));
                            GoalPercent = Activities.Sum(x => x.x.Weight) == 0 ? GoalPercent : GoalPercent + ((activityItems.x.Goal * activityItems.x.Weight) / Activities.Sum(x => x.x.Weight));
                        }
                        BeginingPlan = BeginingPlan + ((BeginingPercent * planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                        ActualPlan = ActualPlan + ((ActualWorkedPercent * planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                        Goal = Goal + ((GoalPercent * planItems.PlanWeight) / Plans.Sum(x => x.PlanWeight));
                    }
                    if (ActualPlan > 0)
                    {
                        if (ActualPlan == Goal)
                        {
                            Progress = 100;
                        }
                        else
                        {
                            float Nominator = ActualPlan;
                            float Denominator = Goal;
                            Progress = (Nominator / Denominator) * 100;
                        }
                    }
                    else Progress = 0;

                    performance = ((float)(Progress));
                    contribution = (performance * structureRow.Weight) / 100;
                    performance = (float)Math.Round(performance, 2);
                    contribution = (float)Math.Round(contribution, 2);
                    totalContribution += contribution;

                }
                bugetYears.labels.Add(BudgetItems.Year.ToString());
                bugetYears.datasets[0].data.Add(totalContribution);
                var color = String.Format("#{0:X6}", rnd.Next(0x1000000));
                bugetYears.datasets[0].backgroundColor.Add(color);
                bugetYears.datasets[0].borderColor.Add(color);


            }

          

            return bugetYears;
        }
        public class Structure
        {

            public string parent { get; set; }
            public string name { get; set; }
            public string id { get; set; }
            public int label_style_fontSize { get; set; }
            public string color { get; set; }
            //public string annotation_label_text { get; set; }
            public Attribute attributes { get; set; }

        }
        public class progress_Strucure
        {
            public string structureName { get; set; }
            public float structureWeight { get; set; }
            public float structurePerformance { get; set; }
            public float structureContribution { get; set; }

        }
        public class Attribute
        {
            public string position { get; set; }
            public string units { get; set; }
        }

        public class ProjectList
        {
            public string ProjectName { get; set; }
            public string DirectorateName { get; set; }
            public float ProjectProgress { get; set; }
            public float OverallProgress { get; set; }
            public string ProjectDuration { get; set; }
            public float Weight { get; set; }
        }


        public class AboutToExpireProjects
        {
            public string DirectorName { get; set; }
            public string ProjectName { get; set; }
            public string TaskName { get; set; }
            public string ActivityName { get; set; }
            public float RequiredAchivement { get; set; }
            public float CurrentAchivement { get; set; }
            public string PlanMonthName { get; set; }
            public Guid? activityId { get; set; }
            public Guid? TaskId { get; set; }
            public string programName { get; set; }
        }

        public class PmDashboardBarchartDto
        {

           public List<string> labels { get; set; }
           public List<PmDashboardBarchartDateset> datasets { get; set; }


        }

        public class PmDashboardBarchartDateset
        {
            public string label { get; set; }
           public List<float> data { get; set; }

            public List<string> backgroundColor { get; set; }
            public List<string> borderColor { get; set; }
            public int borderWidth { get; set; }
            }
    }
    }



﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_API.Services.PM;
using PM_Case_Managemnt_API.Services.PM.ProgressReport;
using static PM_Case_Managemnt_API.Services.PM.ProgressReport.ProgressReportService;

namespace PM_Case_Managemnt_API.Controllers.PM
{
    [Route("api/PM/[controller]")]
    [ApiController]
    public class ProgressReportController : ControllerBase
    {

        private readonly IProgressReportService _progressReportService;
        public ProgressReportController(IProgressReportService progressReportService)
        {
            _progressReportService = progressReportService;
        }


        [HttpGet("DirectorLevelPerformance")]
        public async Task<IActionResult> GetOrganizationDiaram(Guid? BranchId)
        {
            try
            {
                return Ok(await _progressReportService.GetDirectorLevelPerformance(BranchId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("ProgramBudgetReport")]
        public async Task<IActionResult> ProgramBudgetReport(string BudgetYear, string ReportBy)
        {
            try
            {
                return Ok(await _progressReportService.PlanReportByProgram(BudgetYear, ReportBy));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("plandetailreport")]
        public async Task<IActionResult> PlanDetailReport(string BudgetYear, string ProgramId, string ReportBy)
        {
            try
            {
                return Ok(await _progressReportService.StructureReportByProgram(BudgetYear, ProgramId, ReportBy));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpGet("plannedreport")]
        public async Task<IActionResult> plannedreport(string BudgetYea, Guid selectStructureId, string ReportBy)
        {
            try
            {
                return Ok(await _progressReportService.PlanReports(BudgetYea, selectStructureId, ReportBy));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("GetProgressReport")]
        public async Task<IActionResult> ProgressReport(FilterationCriteria filterationCriteria)
        {
            try
            {
                return Ok(await _progressReportService.ProgresssReport(filterationCriteria));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }


        [HttpGet("GetProgressReportByStructure")]
        public async Task<IActionResult> GetProgressReportByStructure(int BudgetYea, Guid selectStructureId, string ReportBy)
        {
            try
            {
                return Ok(await _progressReportService.GetProgressByStructure(BudgetYea, selectStructureId, ReportBy));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }



        }
        [HttpPost("GetPerformanceReport")]
        public async Task<IActionResult> GetPerformanceReport(FilterationCriteria filterationCriteria)
        {
            try
            {
                return Ok(await _progressReportService.PerformanceReports(filterationCriteria));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        
        [HttpGet("GetActivityProgress")]
        public async Task<IActionResult> GetActivityProgress(Guid activityId)
        {
            try
            {
                return Ok(await _progressReportService.GetActivityProgress(activityId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
        [HttpGet("GetEstimatedCost")]
        public async Task<IActionResult> GetEstimatedCost(Guid structureId, int budegtYear)
        {
            try
            {
                return Ok(await _progressReportService.GetEstimatedCost(structureId, budegtYear));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

    }
    }

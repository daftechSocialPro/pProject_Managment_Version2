﻿using Microsoft.Identity.Client;
using PM_Case_Managemnt_API.Data;
using PM_Case_Managemnt_API.DTOS.CaseDto;
using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.Models.Common;

namespace PM_Case_Managemnt_API.Services.CaseMGMT.CaseForwardService
{
    public class CaseForwardService: ICaseForwardService
    {
        private readonly DBContext _dbContext;

        public CaseForwardService(DBContext dbContext)
        {
            _dbContext = dbContext;
        }


        public async Task AddMany(CaseForwardPostDto caseForwardPostDto)
        {
            try
            {
                List<CaseForward> caseForwards = new List<CaseForward>();

                //foreach(Guid forwardToStructureId in caseForwardPostDto.ForwardedToStructureId)
                //{
                //    caseForwards.Add(
                //    new()
                //    {
                //        Id = Guid.NewGuid(),
                //        CreatedAt = DateTime.Now,
                //        CreatedBy = caseForwardPostDto.CreatedBy,
                //        CaseId = caseForwardPostDto.CaseId,
                //        ForwardedByEmployeeId = caseForwardPostDto.ForwardedByEmployeeId,
                //        ForwardedToStructureId = forwardToStructureId,
                //        RowStatus = RowStatus.Active,
                //    }
                //        );
                //}

                //await _dbContext.CaseForwards.AddRangeAsync(caseForwards);
                //await _dbContext.SaveChangesAsync();
                
            } catch (Exception ex)
            {
                throw new Exception(ex.Message);    
            }
        }
    }
}

﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using PM_Case_Managemnt_API.DTOS.CaseDto;
using PM_Case_Managemnt_API.Services.CaseMGMT.CaseForwardService;

namespace PM_Case_Managemnt_API.Controllers.Case
{
    [Route("api/case")]
    [ApiController]
    public class CaseForwardController : ControllerBase
    {
        private readonly ICaseForwardService _caseForwardService;

        public CaseForwardController(ICaseForwardService caseForwardService)
        {
            _caseForwardService = caseForwardService;
        }

        [HttpPost("forward")]
        public async Task<IActionResult> Create(CaseForwardPostDto caseForwardPostDto)
        {
            try
            {
                await _caseForwardService.AddMany(caseForwardPostDto);
                return NoContent();
            } catch (Exception ex) {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}

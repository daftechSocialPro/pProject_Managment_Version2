﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_API.DTOS.Common;
using PM_Case_Managemnt_API.Models.Common;
using PM_Case_Managemnt_API.Services.Common;


namespace PM_Case_Managemnt_API.Controllers.Common.Organization
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrgStructureController : ControllerBase
    {
        private readonly IOrgStructureService _orgStructureService;
        public OrgStructureController(IOrgStructureService orgStructureService)
        {

            _orgStructureService = orgStructureService;

        }



        [HttpPost]

        public IActionResult Create([FromBody] OrgStructureDto orgStructure)
        {
            try
            {
                var response = _orgStructureService.CreateOrganizationalStructure(orgStructure);

                return Ok(new { response });

            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }
        [HttpGet]

        public async Task<List<OrgStructureDto>> GetStructures(Guid? BranchId)
        {
            return await _orgStructureService.GetOrganizationStructures(BranchId);
        }
        [HttpGet("parentStructures")]

        public async Task<List<SelectListDto>> GetParentStructureList(string branchid)
        {

            return await _orgStructureService.getParentStrucctureSelectList(Guid.Parse(branchid));

        }


        [HttpPut]

        public IActionResult Update([FromBody] OrgStructureDto orgStructure)
        {
            try
            {
                var response = _orgStructureService.UpdateOrganizationalStructure(orgStructure);

                return Ok(new { response });

            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error : {ex}");
            }
        }

        [HttpGet("orgdiagram")]
        public async Task<IActionResult> GetOrganizationDiaram(Guid? BranchId)
        {
            try
            {
                return Ok(await _orgStructureService.getDIagram(BranchId));
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}

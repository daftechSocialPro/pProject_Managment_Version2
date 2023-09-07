﻿using PM_Case_Managemnt_API.DTOS.Common;

namespace PM_Case_Managemnt_API.Services.Common
{
    public interface IOrgStructureService
    {

        public Task<int> CreateOrganizationalStructure(OrgStructureDto orgStructure);

        public Task<int> UpdateOrganizationalStructure(OrgStructureDto organizationProfile);
        public Task<List<OrgStructureDto>> GetOrganizationStructures(Guid? BranchId);

        public Task<List<SelectListDto>> getParentStrucctureSelectList(Guid branchId);

        public Task<List<DiagramDto>> getDIagram(Guid? BranchId);


    }
}

﻿using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PM_Case_Managemnt_API.DTOS.PM
{
    public class ProgramDto
    {
        public Guid Id { get; set; }
        public string ProgramName { get; set; }
        public float ProgramPlannedBudget { get; set; }
        public float RemainingBudget{ get; set; }

        public float RemainingWeight { get; set; }
        public string ProgramBudgetYear { get; set; }
        public List<ProgramStructureDto> ProgramStructure { get; set; }
        public int NumberOfProjects { get; set; }
        public string Remark { get; set; }

    }

    public class ProgramStructureDto {
        public string  StructureName { get; set; }
        public string  StructureHead { get; set; }
    }
}


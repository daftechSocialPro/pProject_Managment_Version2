﻿using Microsoft.AspNetCore.Mvc;
using PM_Case_Managemnt_API.DTOS.Common;
using PM_Case_Managemnt_API.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace PM_Case_Managemnt_API.DTOS.PM
{

    public class AddCommiteDto
    {
        public string Name { get; set; } = null!;
        public string Remark { get; set; } = null!;
        public Guid CreatedBy { get; set; }
    }

    public class UpdateCommiteDto: AddCommiteDto
    {
        [Required]
        public Guid Id { get; set; }

        public RowStatus RowStatus { get; set; }
    }
    public class CommiteListDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public int NoOfEmployees { get; set; }
        public List<SelectListDto> EmployeeList { get; set; } = null!;
        public string? Remark { get; set; }
    }

    public class CommiteEmployeesdto
    {
        public Guid CommiteeId { get; set; }
        public Guid[] EmployeeList { get; set; }

        public Guid CreatedBy { get; set; }
    }

   
}

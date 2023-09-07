﻿using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.Models.Common;

namespace PM_Case_Managemnt_API.DTOS.CaseDto
{
    public class FileSettingPostDto
    {
        public Guid CaseTypeId { get; set; }
        public string Name { get; set; }
        public string FileType { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class FileSettingGetDto
    {
        public Guid Id { get; set; }
        public string CaseTypeTitle { get; set; }
        public string Name { get; set; }

        public string FileType { get; set; }
        public string RowStatus { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid CreatedBy { get; set; }
    }
}

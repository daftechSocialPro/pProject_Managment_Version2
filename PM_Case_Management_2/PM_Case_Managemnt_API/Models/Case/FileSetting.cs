﻿
using PM_Case_Managemnt_API.Models.Common;

namespace PM_Case_Managemnt_API.Models.CaseModel
{
    public class FileSetting : CommonModel
    {
        
        public Guid CaseTypeId { get; set; }
        public virtual CaseType CaseType { get; set; } = null!;

        public string FileName { get; set; } = null!;

       
        public FileType FileType { get; set; }

    }

    public enum FileType
    {
        files,
        text
    }
}

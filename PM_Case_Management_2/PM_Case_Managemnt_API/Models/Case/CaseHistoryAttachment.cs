﻿using PM_Case_Managemnt_API.Models.Common;


namespace PM_Case_Managemnt_API.Models.CaseModel
{
    public class CaseHistoryAttachment : CommonModel
    {
        public Guid CaseHistoryId { get; set; }
        public virtual CaseHistory CaseHistory { get; set; } = null!;
        public string FilePath { get; set; } = null!;

    }
}

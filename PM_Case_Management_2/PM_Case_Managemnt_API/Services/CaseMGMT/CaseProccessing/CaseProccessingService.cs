﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_API.Data;
using PM_Case_Managemnt_API.DTOS.CaseDto;
using PM_Case_Managemnt_API.DTOS.Common;
using PM_Case_Managemnt_API.Helpers;
using PM_Case_Managemnt_API.Hubs;
using PM_Case_Managemnt_API.Hubs.EncoderHub;
using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.Models.Common;
using PM_Case_Managemnt_API.Services.CaseMGMT.CaseForwardService;
using PM_Case_Managemnt_API.Services.CaseMGMT.History;
using PM_Case_Managemnt_API.Services.CaseService.Encode;
using static System.Net.Mime.MediaTypeNames;

namespace PM_Case_Managemnt_API.Services.CaseMGMT
{
    public class CaseProccessingService : ICaseProccessingService
    {

        private readonly DBContext _dbContext;
        private readonly AuthenticationContext _authenticationContext;
        private readonly ISMSHelper _smshelper;
        private readonly ICaseEncodeService _caseEncodeService;
        private IHubContext<EncoderHub, IEncoderHubInterface> _encoderHub;


        public CaseProccessingService(
            DBContext dbContext, 
            AuthenticationContext authenticationContext, 
            ISMSHelper smshelper,
            ICaseEncodeService caseEncodeService,
            IHubContext<EncoderHub, IEncoderHubInterface> encoderHub          )
        {
            _dbContext = dbContext;
            _authenticationContext = authenticationContext;
            _smshelper = smshelper;
            _caseEncodeService = caseEncodeService;
            _encoderHub = encoderHub;
            
        }

        public async Task<int> ConfirmTranasaction(ConfirmTranscationDto confirmTranscationDto)
        {

            try
            {

                var history = _dbContext.CaseHistories.Find(confirmTranscationDto.CaseHistoryId);
                history.IsConfirmedBySeretery = true;
                history.SecreteryConfirmationDateTime = DateTime.Now;
                history.SecreteryId = confirmTranscationDto.EmployeeId;

                _dbContext.CaseHistories.Attach(history);
                _dbContext.Entry(history).Property(c => c.IsConfirmedBySeretery).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SecreteryConfirmationDateTime).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SecreteryId).IsModified = true;
                _dbContext.SaveChanges();

                return 1;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AssignTask(CaseAssignDto caseAssignDto)
        {
            try
            {
                string userId = _authenticationContext.ApplicationUsers.Where(x => x.EmployeesId == caseAssignDto.AssignedByEmployeeId).FirstOrDefault().Id;
                Case caseToAssign = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                // CaseHistory caseHistory = await _dbContext.CaseHistories.SingleOrDefaultAsync(el => el.CaseId.Equals(caseAssignDto.CaseId));

                var toEmployeeId = caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null ?
             _dbContext.Employees.FirstOrDefault(
                 e =>
                     e.OrganizationalStructureId == caseAssignDto.AssignedToStructureId &&
                     e.Position == Position.Director).Id : caseAssignDto.AssignedToEmployeeId;

                var fromEmployeestructure = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == caseAssignDto.AssignedByEmployeeId).First().OrganizationalStructure.Id;

                Case currCase = await _dbContext.Cases.SingleOrDefaultAsync(el => el.Id.Equals(caseAssignDto.CaseId));
                currCase.AffairStatus = AffairStatus.Assigned;

                _dbContext.Entry(currCase).Property(curr => curr.AffairStatus).IsModified = true;
                //await _dbContext.SaveChangesAsync();
                if (caseAssignDto.ForwardedToStructureId != null)
                {
                    var startupHistory = new CaseHistory
                    {
                        Id = Guid.NewGuid(),
                        CreatedAt = DateTime.Now,
                        CreatedBy = Guid.Parse(userId),
                        RowStatus = RowStatus.Active,
                        CaseId = caseAssignDto.CaseId,
                        CaseTypeId = _dbContext.Cases.Find(caseAssignDto.CaseId).CaseTypeId,
                        AffairHistoryStatus = AffairHistoryStatus.Pend,
                        FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                        FromStructureId = fromEmployeestructure,
                        ReciverType = ReciverType.Orginal,
                        ToStructureId = caseAssignDto.AssignedToStructureId,
                        ToEmployeeId = toEmployeeId,

                    };
                    startupHistory.SecreateryNeeded = (caseAssignDto.AssignedToEmployeeId == Guid.Empty || caseAssignDto.AssignedToEmployeeId == null) ? true : false;

                    caseToAssign.AffairStatus = AffairStatus.Assigned;
                    _dbContext.Entry(caseToAssign).Property(x => x.AffairStatus).IsModified = true;

                    await _dbContext.CaseHistories.AddAsync(startupHistory);
                    await _dbContext.SaveChangesAsync();

                    foreach (var row in caseAssignDto.ForwardedToStructureId)
                    {

                        var toEmployee=new Guid();
                        if (_dbContext.Employees.FirstOrDefault(
                                 e =>
                                     e.OrganizationalStructureId == row &&
                                     e.Position == Position.Director) == null)
                        {
                            toEmployee = Guid.Empty;
                        }
                        else
                        {
                            toEmployee = _dbContext.Employees.FirstOrDefault(
                                 e =>
                                     e.OrganizationalStructureId == row &&
                                     e.Position == Position.Director).Id;
                        }

                        CaseHistory history = new()
                            {
                                Id = Guid.NewGuid(),
                                CreatedAt = DateTime.Now,
                                CreatedBy = Guid.Parse(userId),
                                CaseId = caseAssignDto.CaseId,
                                AffairHistoryStatus = AffairHistoryStatus.Pend,
                                FromEmployeeId = caseAssignDto.AssignedByEmployeeId,
                                FromStructureId = fromEmployeestructure,
                                ToStructureId = row,
                                ReciverType = ReciverType.Cc,
                                ToEmployeeId = toEmployee,
                                RowStatus = RowStatus.Active,

                            };
                            await _dbContext.CaseHistories.AddAsync(history);
                            await _dbContext.SaveChangesAsync();
                        
                    }
                }
                var assigndCase = await _caseEncodeService.GetAllTransfred(toEmployeeId.Value);


                await _encoderHub.Clients.Group(toEmployeeId.Value.ToString()).getNotification(assigndCase,toEmployeeId.Value.ToString());

                //await _encoderHub.Clients.All.getNotification(assigndCase);


            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task CompleteTask(CaseCompleteDto caseCompleteDto)
        {
            try
            {
                CaseHistory selectedHistory = _dbContext.CaseHistories.Find(caseCompleteDto.CaseHistoryId);
                Guid UserId = Guid.Parse((await _authenticationContext.ApplicationUsers.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);

                if (selectedHistory.ToEmployeeId != caseCompleteDto.EmployeeId)
                    throw new Exception("You are unauthorized to complete this case.");


                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Completed;
                selectedHistory.CompletedDateTime = DateTime.Now;
                selectedHistory.Remark = caseCompleteDto.Remark;


                Case currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                CaseHistory currentHist = await _dbContext.CaseHistories.Include(x => x.Case).Include(x => x.ToStructure).FirstOrDefaultAsync(x => x.Id == selectedHistory.Id);

                _dbContext.CaseHistories.Attach(selectedHistory);
                _dbContext.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.CompletedDateTime).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;
                //_dbContext.Entry(selectedHistory).Property(x => x.IsSmsSent).IsModified = true;
                //_dbContext.SaveChanges();

                var selectedCase = _dbContext.Cases.Find(selectedHistory.CaseId);
                selectedCase.CompletedAt = DateTime.Now;
                selectedCase.AffairStatus = AffairStatus.Completed;

                _dbContext.Cases.Attach(selectedCase);
                _dbContext.Entry(selectedCase).Property(x => x.CompletedAt).IsModified = true;
                _dbContext.Entry(selectedCase).Property(x => x.AffairStatus).IsModified = true;

                await _dbContext.SaveChangesAsync();

                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + currentHist.ToStructure.StructureName + " ተጠናቋል\nየቢሮ ቁጥር: - ";

                await _smshelper.SendSmsForCase(message, currentHist.CaseId, currentHist.Id, UserId.ToString(), MessageFrom.Complete);

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public async Task RevertTask(CaseRevertDto revertCase)
        {
            try
            {

                Employee currEmp = await _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id.Equals(revertCase.EmployeeId)).FirstOrDefaultAsync();
                CaseHistory selectedHistory = _dbContext.CaseHistories.Find(revertCase.CaseHistoryId);
                Guid UserId = Guid.Parse((await _authenticationContext.ApplicationUsers.Where(appUsr => appUsr.EmployeesId.Equals(selectedHistory.ToEmployeeId)).FirstAsync()).Id);

                selectedHistory.AffairHistoryStatus = AffairHistoryStatus.Revert;
                selectedHistory.RevertedAt = DateTime.Now;
                selectedHistory.Remark = revertCase.Remark;

                _dbContext.CaseHistories.Attach(selectedHistory);
                _dbContext.Entry(selectedHistory).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.RevertedAt).IsModified = true;
                _dbContext.Entry(selectedHistory).Property(x => x.Remark).IsModified = true;
                CaseHistory newHistory = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = Guid.Parse(_authenticationContext.ApplicationUsers.Where(appUsr => appUsr.EmployeesId.Equals(revertCase.EmployeeId)).First().Id),
                    RowStatus = RowStatus.Active,
                    FromEmployeeId = revertCase.EmployeeId,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = selectedHistory.ToEmployeeId,
                    ToStructureId = selectedHistory.ToStructureId,
                    Remark = "",
                    CaseId = selectedHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    childOrder = selectedHistory.childOrder += 1,
                };


                await _dbContext.CaseHistories.AddAsync(newHistory);
                await _dbContext.SaveChangesAsync();

                Case currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == selectedHistory.CaseId);
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                var message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ በ፡" + selectedHistory.ToStructure.StructureName + " ወደኋላ ተመልሷል  \nየቢሮ ቁጥር: -";

                await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Revert);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task TransferCase(CaseTransferDto caseTransferDto)
        {
            try
            {
                Employee currEmp = await _dbContext.Employees.Where(el => el.Id.Equals(caseTransferDto.FromEmployeeId)).FirstOrDefaultAsync();
                CaseHistory currentLastHistory = await _dbContext.CaseHistories.Where(el => el.Id.Equals(caseTransferDto.CaseHistoryId)).OrderByDescending(x => x.CreatedAt).FirstAsync();

                Guid UserId = Guid.Parse((await _authenticationContext.ApplicationUsers.Where(appUsr => appUsr.EmployeesId.Equals(caseTransferDto.ToEmployeeId)).FirstAsync()).Id);

                if (caseTransferDto.FromEmployeeId != currentLastHistory.ToEmployeeId)
                    throw new Exception("You are not authorized to transfer this case.");

                currentLastHistory.AffairHistoryStatus = AffairHistoryStatus.Transfered;
                currentLastHistory.TransferedDateTime = DateTime.Now;

                _dbContext.CaseHistories.Attach(currentLastHistory);
                _dbContext.Entry(currentLastHistory).Property(c => c.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(currentLastHistory).Property(c => c.TransferedDateTime).IsModified = true;

                var toEmployee = caseTransferDto.ToEmployeeId == Guid.Empty || caseTransferDto.ToEmployeeId == null ?
       _dbContext.Employees.FirstOrDefault(
           e =>
               e.OrganizationalStructureId == caseTransferDto.ToStructureId &&
               e.Position == Position.Director).Id : caseTransferDto.ToEmployeeId;


                var newHistory = new CaseHistory
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = UserId,

                    RowStatus = RowStatus.Active,
                    FromEmployeeId = caseTransferDto.FromEmployeeId,
                    FromStructureId = currEmp.OrganizationalStructureId,
                    ToEmployeeId = toEmployee,
                    ToStructureId = caseTransferDto.ToStructureId,
                    Remark = caseTransferDto.Remark,
                    CaseId = currentLastHistory.CaseId,
                    ReciverType = ReciverType.Orginal,
                    CaseTypeId = currentLastHistory.CaseTypeId,
                    childOrder = currentLastHistory.childOrder + 1
                    //must be change
                };


                await _dbContext.CaseHistories.AddAsync(newHistory);
                await _dbContext.CaseAttachments.AddRangeAsync(caseTransferDto.CaseAttachments);
                await _dbContext.SaveChangesAsync();
               

                /// Sending SMS
                Case currentCase = await _dbContext.Cases.Include(x => x.Applicant).Include(x => x.Employee).FirstOrDefaultAsync(x => x.Id == newHistory.CaseId);
                string name = currentCase.Applicant != null ? currentCase.Applicant.ApplicantName : currentCase.Employee.FullName;
                string toStructure = _dbContext.OrganizationalStructures.Find(newHistory.ToStructureId).StructureName;


                var assigndCase = await _caseEncodeService.GetAllTransfred(toEmployee);
                await _encoderHub.Clients.Group(toEmployee.ToString()).getNotification(assigndCase, toEmployee.ToString());

                string message = name + "\nበጉዳይ ቁጥር፡" + currentCase.CaseNumber + "\nየተመዘገበ ጉዳዮ ለ " + toStructure + " ተላልፏል\nየቢሮ ቁጥር:";

                await _smshelper.SendSmsForCase(message, newHistory.CaseId, newHistory.Id, UserId.ToString(), MessageFrom.Transfer);
            }
            catch (Exception ex)

            {
                throw new Exception(ex.Message);
            }
        }



        public async Task AddToWaiting(Guid caseHistoryId)
        {
            try
            {


                var history = _dbContext.CaseHistories.Find(caseHistoryId);
                history.AffairHistoryStatus = AffairHistoryStatus.Waiting;
                history.SeenDateTime = null;
                _dbContext.CaseHistories.Attach(history);
                _dbContext.Entry(history).Property(c => c.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(history).Property(c => c.SeenDateTime).IsModified = true;



                await _dbContext.SaveChangesAsync();





            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task SendSMS(CaseCompleteDto smsdetail)
        {

            var history = _dbContext.CaseHistories.Find(smsdetail.CaseHistoryId);
            await _smshelper.SendSmsForCase(smsdetail.Remark, history.CaseId, history.Id, smsdetail.EmployeeId.ToString(), MessageFrom.Custom_text);
        }

        public async Task<CaseEncodeGetDto> GetCaseDetial(Guid employeeId, Guid historyId)
        {

            Employee user = _dbContext.Employees.Include(x => x.OrganizationalStructure).Where(x => x.Id == employeeId).FirstOrDefault();


            CaseHistory currentHistry = _dbContext.CaseHistories
                .Include(x => x.Case.CaseType)
                 .Include(x => x.Case.Applicant)
                 .Include(x => x.Case.Employee)
                .Include(x => x.FromEmployee)
                .Include(x => x.FromStructure)
                .Include(x => x.Case)?.FirstOrDefault(x => x.AffairHistoryStatus != AffairHistoryStatus.Completed
                                     && x.ToEmployeeId == employeeId
                                     && x.Id == historyId);




            if (currentHistry != null && (currentHistry.AffairHistoryStatus == AffairHistoryStatus.Pend || currentHistry.AffairHistoryStatus == AffairHistoryStatus.Waiting))
            {
                currentHistry.AffairHistoryStatus = AffairHistoryStatus.Seen;
                currentHistry.SeenDateTime = DateTime.Now;
                _dbContext.CaseHistories.Attach(currentHistry);
                _dbContext.Entry(currentHistry).Property(x => x.AffairHistoryStatus).IsModified = true;
                _dbContext.Entry(currentHistry).Property(x => x.SeenDateTime).IsModified = true;
                _dbContext.SaveChanges();
            }



            List<SelectListDto> attachments = (from x in _dbContext.CaseAttachments.Where(x => x.CaseId == currentHistry.CaseId)
                                               select new SelectListDto
                                               {
                                                   Id = x.Id,
                                                   Name = x.FilePath


                                               }).ToList();

            attachments.AddRange((from x in _dbContext.FilesInformations.Where(x => x.CaseId == currentHistry.CaseId)
                             select new SelectListDto { Id = x.Id, Name = x.FilePath, Photo  = x.FileDescription }).ToList());

            List<CaseDetailStructureDto> caseDetailstructures = _dbContext.CaseHistories.Include(x => x.FromEmployee).Include(x => x.FromStructure).Where(x => x.CaseId == currentHistry.CaseId).OrderByDescending(x => x.CreatedAt).Select(x => new CaseDetailStructureDto
            {
                FromEmployee = x.FromEmployee.FullName,
                FormStructure = x.FromStructure.StructureName,
                SeenDate = x.CreatedAt.ToString()

            }).ToList();

            CaseEncodeGetDto result = new CaseEncodeGetDto
            {
                Id = currentHistry.Id,
                CaseTypeName = currentHistry.Case.CaseType.CaseTypeTitle,
                CaseNumber = currentHistry.Case.CaseNumber,
                CreatedAt = currentHistry.Case.CreatedAt.ToString(),
                ApplicantName = currentHistry.Case.Applicant.ApplicantName,
                ApplicantPhoneNo = currentHistry.Case.Applicant.PhoneNumber,
                EmployeeName = currentHistry.Case.Employee?.FullName,
                EmployeePhoneNo = currentHistry.Case.Employee?.PhoneNumber,
                LetterNumber = currentHistry.Case.LetterNumber,
                LetterSubject = currentHistry.Case.LetterSubject,
                Position = user.Position.ToString(),
                FromStructure = currentHistry.FromStructure.StructureName,
                FromEmployeeId = currentHistry.FromEmployee.FullName,
                ReciverType = currentHistry.ReciverType.ToString(),
                SecreateryNeeded = currentHistry.SecreateryNeeded,
                IsConfirmedBySeretery = currentHistry.IsConfirmedBySeretery,
                ToEmployee = currentHistry.ToEmployee.FullName,
                ToStructure = currentHistry.ToStructure.StructureName,
                AffairHistoryStatus = currentHistry.AffairHistoryStatus.ToString(),
                Attachments = attachments,
                CaseTypeId = currentHistry.Case.CaseTypeId.ToString(),
                CaseDetailStructures = caseDetailstructures

            };


            return result;







        }


        public async Task<int> ArchiveCase(ArchivedCaseDto archivedCaseDto)
        {

            Case cases = _dbContext.Cases.Find(archivedCaseDto.CaseId);

            cases.FolderId = archivedCaseDto.FolderId;
            cases.IsArchived = true;


            _dbContext.Entry(cases).Property(x => x.FolderId).IsModified = true;
            _dbContext.Entry(cases).Property(x => x.IsArchived).IsModified = true;

            await _dbContext.SaveChangesAsync();

            return 1;
        }


        public async Task<CaseState> GetCaseState(Guid CaseTypeId, Guid caseHistoryId)
        { 

            var childCaseType = await _dbContext.CaseTypes.Where(x => x.ParentCaseTypeId == CaseTypeId).ToListAsync();
            CaseState caseState = new CaseState();

            int childcount = _dbContext.CaseHistories.Find(caseHistoryId).childOrder + 1;
            foreach (var childaffair in childCaseType)
            {
                

              
                if (childaffair.OrderNumber == childcount)
                {
                    caseState.CurrentState = childaffair.CaseTypeTitle;

                    caseState.NeededDocuments = new List<string>();

                    var files = await _dbContext.FileSettings.Where(x => x.CaseTypeId == childaffair.Id).ToListAsync();

                    foreach (var file in files)
                    {
                        caseState.NeededDocuments.Add(file.FileName);
                    }
                }

                if (childaffair.OrderNumber == childcount + 1)
                {

                    caseState.NextState = childaffair.CaseTypeTitle;
                }
            }

            return caseState;
        }

        public async Task<bool> Ispermitted(Guid employeeId, Guid caseId)
        {
            var caseIDD = _dbContext.CaseHistories.Find(caseId).CaseId;
            var employee = _dbContext.CaseHistories.Where(x => x.CaseId == caseIDD && x.ReciverType==ReciverType.Orginal ).OrderByDescending(z => z.childOrder).FirstOrDefault().ToEmployeeId;
            if (employeeId.ToString().ToLower() == employee.ToString().ToLower())
            {
                return true;
            }
            return false; 

        }


    }

  

    public class CaseState
    {
        public string CurrentState { get; set; }
        public string NextState { get; set; }
        public List<string> NeededDocuments { get; set; }
    }
}


﻿using GsmComm.PduConverter;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using PM_Case_Managemnt_API.Data;
using PM_Case_Managemnt_API.DTOS.Case;
using PM_Case_Managemnt_API.Helpers;
using PM_Case_Managemnt_API.Models.CaseModel;
using PM_Case_Managemnt_API.Models.Common;
using System.Net;
using System.Text;

namespace PM_Case_Managemnt_API.Services.CaseMGMT.CaseMessagesService
{
    public class CaseMessagesService : ICaseMessagesService
    {
        private readonly DBContext _dbContext;

        private readonly IConfiguration _configuration;


        public CaseMessagesService(DBContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;

            _configuration = configuration;
        }

        public async Task Add(CaseMessagesPostDto caseMessagePost)
        {
            try
            {
                CaseMessages caseMessage = new()
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.Now,
                    CreatedBy = caseMessagePost.CreatedBy,
                    CaseId = caseMessagePost.CaseId,
                    MessageBody = caseMessagePost.MessageBody,
                    MessageFrom = caseMessagePost.MessageFrom,
                    Messagestatus = caseMessagePost.Messagestatus,
                    RowStatus = Models.Common.RowStatus.Active,
                };

                await _dbContext.CaseMessages.AddAsync(caseMessage);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<CaseUnsentMessagesGetDto>> GetMany(bool messageStatus = false)
        {
            try
            {

                return await (from m in _dbContext.CaseMessages.Include(x => x.Case.Applicant).Include(x => x.Case.CaseType).Where(el => el.Messagestatus.Equals(messageStatus))
                              select new CaseUnsentMessagesGetDto
                              {
                                  Id = m.Id,
                                  CaseNumber = m.Case.CaseNumber,
                                  ApplicantName = m.Case.Applicant.ApplicantName,
                                  LetterNumber = m.Case.LetterNumber,
                                  Subject = m.Case.LetterSubject,
                                  CaseTypeTitle = m.Case.CaseType.CaseTypeTitle,
                                  PhoneNumber = m.Case.Applicant.PhoneNumber,
                                  PhoneNumber2 = m.Case.PhoneNumber2,
                                  Message = m.MessageBody,
                                  MessageGroup = m.MessageFrom.ToString(),
                                  IsSmsSent = m.Messagestatus

                              }).ToListAsync();



            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }





        public async Task SemdMessages(List<CaseUnsentMessagesGetDto> Messages)
        {
            try
            {

                string ipAddress = _configuration["ApplicationSettings:SMS_IP"];
                string coder = _configuration["ApplicationSettings:ORG_CODE"];

                foreach (var message in Messages)
                {
                    string uri = $"http://{ipAddress}/api/SmsSender?orgId={coder}&message={message.Message}&recipantNumber={message.PhoneNumber}";
                    string uri2 = $"http://{ipAddress}/api/SmsSender?orgId={coder}&message={message.Message}&recipantNumber={message.PhoneNumber2}";

                    byte[] byteArray = Encoding.UTF8.GetBytes(message.Message);
                    using (HttpClient c = new HttpClient())
                    {
                        Uri apiUri = new Uri(uri);
                        ByteArrayContent body = new ByteArrayContent(byteArray, 0, byteArray.Length);
                        MultipartFormDataContent multiPartFormData = new MultipartFormDataContent
                                {
                                    body
                                };
                        HttpResponseMessage result = await c.PostAsync(apiUri, multiPartFormData);

                        if (result.StatusCode == HttpStatusCode.OK)
                        {
                            var messa = _dbContext.CaseMessages.Find(message.Id);
                            messa.Messagestatus = true;
                            _dbContext.Entry(messa).State = EntityState.Modified;
                            await _dbContext.SaveChangesAsync();

                        }
                    }

                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}

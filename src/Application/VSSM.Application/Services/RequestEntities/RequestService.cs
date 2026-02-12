using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Arrival;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;
using static Dapper.SqlMapper;

namespace VSSM.Application.Services.RequestEntities
{
    public class RequestService : IRequestService
    {
        private readonly VSSMDbContext _dbContext;
        public RequestService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddLine(int requestId, RequestLineDto dto, CancellationToken token)
        {
            var line = new TblRequestLine
            {
                RequestId = requestId,
                Note = dto.Note,
                Arivcount = dto.Arivcount,
                DosePerVialId = dto.DosePerVialId,
                Usedcount = dto.Usedcount,
                Offercount = dto.Offercount,
                ItemId = dto.ItemId,
                Forperiod = dto.Forperiod,
                Currentcount = dto.Currentcount,
                Confirmcount = dto.Confirmcount,
                Anncount = dto.Anncount,
                Reqcount = dto.Reqcount,
                ItemClassId = dto.ItemClassId,
            };

            _dbContext.TblRequestLines.Add(line);
            await _dbContext.SaveChangesAsync(token);
        }

        public async Task<bool> ChangeStatus(int id, RequestStatusEnum status, string note,  CancellationToken token)
        {
            var request = await _dbContext.TblRequests
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (request == null) return false;

            request.Status = (int)status;
            await _dbContext.SaveChangesAsync(token);
            return true;
        }

        public async Task<int> Create(RequestCreateDto dto, CancellationToken token)
        {
            var req = new TblRequest
            {
                ConfirmDate = dto.ConfirmDate,
                EnteredBy = dto.EnteredBy,
                FiscalPeriodId = dto.FiscalPeriodId,
                Note = dto.Note,
                ReceiverId = dto.ReceiverId,
                RequestNumber = dto.RequestNumber,
                SendDate = dto.SendDate,
                SenderId = dto.SenderId,
                Status = (int)dto.Status,
            };

            _dbContext.TblRequests.Add(req);
            await _dbContext.SaveChangesAsync(token);

            var recepCode = _dbContext.TblRecipients.Where(x => x.RecipientId == dto.SenderId).First().Code;
            req.RequestNumber = recepCode + "-" + req.Id.ToString();
            await _dbContext.SaveChangesAsync(token);

            return req.Id;
        }

        public async Task Delete(int id)
        {
            var request = await _dbContext.TblRequests
                .FirstOrDefaultAsync(x => x.Id == id);

            var lines = await _dbContext.TblRequestLines
                .Where(x => x.RequestId == id)
                .ToListAsync();
            if(lines.Count > 0)
                _dbContext.TblRequestLines.RemoveRange(lines);
            _dbContext.TblRequests.Remove(request);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteLine(int requestLineId, CancellationToken token)
        {
            
            var reqLine = await _dbContext.TblRequestLines.FindAsync(new object[] { requestLineId }, token);
            if (reqLine == null) return;

            var lineCount = _dbContext.TblRequestLines.Count(x => x.RequestId == reqLine.RequestId);
            _dbContext.TblRequestLines.Remove(reqLine);
            if (lineCount == 1)
            {
                await Delete(reqLine.RequestId.Value);
            }
            await _dbContext.SaveChangesAsync(token);
        }

        public async Task<RequestGetDto?> GetById(int id, CancellationToken token)
        {
            var request = await _dbContext.TblRequests
                .FirstOrDefaultAsync(x => x.Id == id, token);

            if (request == null) return null;

            var reqLines = await _dbContext.TblRequestLines.Where(x => x.RequestId == id).Include(i => i.Item).ToListAsync();

            return new RequestGetDto
            {
                ConfirmDate = request.ConfirmDate,
                EnteredBy = request.EnteredBy,
                Id = request.Id,
                RequestNumber= request.RequestNumber,
                RequestStatus = request.Status,
                SendDate = request.SendDate,
                RequestLines = reqLines.Select(r => new RequestLineDto
                {
                    Id = r.Id,
                    RequestId = r.RequestId,
                    Anncount = r.Anncount,
                    Arivcount = r.Arivcount,
                    Confirmcount = r.Confirmcount,
                    Currentcount = r.Currentcount,
                    DosePerVialId = r.DosePerVialId,
                    Forperiod = r.Forperiod,
                    ItemClassId = r.ItemClassId,
                    ItemId = r.ItemId,
                    ItemName = r.Item.FullName,
                    Note = r.Note,
                    Offercount = r.Offercount,
                    Reqcount = r.Reqcount,
                    Usedcount = r.Usedcount
                }).ToList()
            };
        }

        public async Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(string? search, int page, int pageSize, 
            string sortField, string sortOrder,
            int currentRecipientId, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblRequest> query = _dbContext.TblRequests;

            query = query.Where(q => q.SenderId == currentRecipientId || (q.ReceiverId == currentRecipientId && q.Status.Value == (int)RequestStatusEnum.Draft));

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.RequestNumber.Contains(search) || x.SendDate.ToString().Contains(search)
                );
            }

            // 2) Sorting
            query = sortField switch
            {
                "SendDate" => sortOrder == "asc" ? query.OrderBy(x => x.SendDate) : query.OrderByDescending(x => x.SendDate),
                //"Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
            };

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new RequestListDto
                {
                    Id = x.Id,
                    SendDate = x.SendDate,
                    RequestNumber = x.RequestNumber,
                    ConfirmDate = x.ConfirmDate,
                    EnteredBy = x.EnteredBy,
                    EnteredByName = x.EnteredByNavigation.FirstName + " " + x.EnteredByNavigation.LastName,
                    RequestStatus = x.Status,
                    Status = (RequestStatusEnum)x.Status.Value
                })
                .OrderByDescending(r => r.Id)
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<RequestListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }


        public async Task<PagingSearchOrderDto<RequestListDto>> GetReceivedRequestsPagedList(string? search, int page, int pageSize,
                string sortField, string sortOrder,
                int currentRecipientId, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblRequest> query = _dbContext.TblRequests;

            query = query.Where(q => q.ReceiverId == currentRecipientId);

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.RequestNumber.Contains(search) || x.SendDate.ToString().Contains(search)
                );
            }

            // 2) Sorting
            query = sortField switch
            {
                "SendDate" => sortOrder == "asc" ? query.OrderBy(x => x.SendDate) : query.OrderByDescending(x => x.SendDate),
                //"Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
                _ => sortOrder == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
            };

            // 3) Total count before paging
            paging.TotalItems = await query.CountAsync();

            // 4) Apply paging
            var items = await query
                .Skip(paging.Skip)
                .Take(paging.PageSize)
                .Select(x => new RequestListDto
                {
                    Id = x.Id,
                    SendDate = x.SendDate,
                    RequestNumber = x.RequestNumber,
                    ConfirmDate = x.ConfirmDate,
                    EnteredBy = x.EnteredBy,
                    EnteredByName = x.EnteredByNavigation.FirstName + " " + x.EnteredByNavigation.LastName,
                    RequestStatus = x.Status,
                    Status = (RequestStatusEnum)x.Status.Value
                })
                .OrderByDescending(r => r.Id)
                .ToListAsync(cancellationToken);

            return new PagingSearchOrderDto<RequestListDto>
            {
                Items = items,
                Paging = paging,
                SearchText = search,
                SortField = sortField,
                SortOrder = sortOrder
            };
        }



    }
}

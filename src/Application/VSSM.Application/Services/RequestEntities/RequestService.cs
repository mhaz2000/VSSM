using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.RequestEntities;

public class RequestService(VSSMDbContext dbContext) : IRequestService
{
    public async Task AddLine(int requestId, RequestLineDto dto, CancellationToken token)
    {
        var line = new TblRequestLine
        {
            RequestId = requestId,
            Note = dto.Note,
            DosePerVialId = dto.DosePerVialId,
            ArriveCount = dto.ArriveCount,
            UsedCount = dto.UsedCount,
            ItemId = dto.ItemId,
            CurrentCount = dto.CurrentCount,
            ConfirmCount = dto.ConfirmCount,
            ReqCount = dto.ReqCount,
            ItemClassId = dto.ItemClassId,
        };

        dbContext.TblRequestLines.Add(line);
        await dbContext.SaveChangesAsync(token);
    }

    public async Task<bool> ChangeStatus(int id, RequestStatusEnum status, string note, CancellationToken token)
    {
        var request = await dbContext.TblRequests
            .FirstOrDefaultAsync(x => x.Id == id, token);

        if (request == null) return false;

        request.Status = status;
        await dbContext.SaveChangesAsync(token);
        return true;
    }

    public async Task<int> Create(RequestCreateDto dto, CancellationToken token)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(token);

        var shamsiYear = new PersianCalendar().GetYear(DateTime.Now);
        var prefix = $"{shamsiYear}-";
        var count = await dbContext.TblRequests.CountAsync(x => x.RequestNumber.StartsWith(prefix), token);
        var requestNumber = $"{prefix}{(count + 1):D3}";

        var req = new TblRequest
        {
            ConfirmDate = dto.ConfirmDate,
            EnteredBy = dto.EnteredBy,
            FiscalPeriodId = dto.FiscalPeriodId,
            ReceiverId = dto.ReceiverId,
            RequestNumber = requestNumber,
            SendDate = dto.SendDate,
            SenderId = dto.SenderId,
            Status = dto.Status,
        };

        dbContext.TblRequests.Add(req);
        await dbContext.SaveChangesAsync(token);
        await transaction.CommitAsync(token);

        return req.Id;
    }

    public async Task Delete(int id)
    {
        var request = await dbContext.TblRequests
            .FirstOrDefaultAsync(x => x.Id == id);

        if (request == null) return;

        var lines = await dbContext.TblRequestLines
            .Where(x => x.RequestId == id)
            .ToListAsync();
        if (lines.Count > 0)
            dbContext.TblRequestLines.RemoveRange(lines);

        dbContext.TblRequests.Remove(request);
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteLine(int requestLineId, CancellationToken token)
    {
        var reqLine = await dbContext.TblRequestLines.FindAsync(new object[] { requestLineId }, token);
        if (reqLine == null) return;

        var lineCount = dbContext.TblRequestLines.Count(x => x.RequestId == reqLine.RequestId);
        dbContext.TblRequestLines.Remove(reqLine);
        if (lineCount == 1)
        {
            await Delete(reqLine.RequestId.Value);
        }
        await dbContext.SaveChangesAsync(token);
    }

    public async Task<RequestGetDto?> GetById(int id, CancellationToken token)
    {
        var request = await dbContext.TblRequests
            .FirstOrDefaultAsync(x => x.Id == id, token);

        if (request == null) return null;

        var reqLines = await dbContext.TblRequestLines.Where(x => x.RequestId == id).Include(i => i.Item).ToListAsync();

        return new RequestGetDto
        {
            ConfirmDate = request.ConfirmDate,
            EnteredBy = request.EnteredBy,
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            RequestStatus = (int)request.Status!,
            SendDate = request.SendDate,
            RequestLines = reqLines.Select(r => new RequestLineDto
            {
                Id = r.Id,
                RequestId = r.RequestId,
                ArriveCount = r.ArriveCount,
                ConfirmCount = r.ConfirmCount,
                CurrentCount = r.CurrentCount,
                DosePerVialId = r.DosePerVialId,
                ItemClassId = r.ItemClassId,
                ItemId = r.ItemId,
                ItemName = r.Item.FullName,
                Note = r.Note,
                ReqCount = r.ReqCount,
                UsedCount = r.UsedCount
            }).ToList()
        };
    }

    public async Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        int currentRecipientId, CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblRequest> query = dbContext.TblRequests;

        query = query.Where(q => q.SenderId == currentRecipientId || (q.ReceiverId == currentRecipientId && q.Status!.Value == RequestStatusEnum.Draft));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.RequestNumber.Contains(search) || x.SendDate.HasValue && x.SendDate.Value.ToString().Contains(search));
        }

        query = sortField switch
        {
            "SendDate" => sortOrder == "asc" ? query.OrderBy(x => x.SendDate) : query.OrderByDescending(x => x.SendDate),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
        };

        paging.TotalItems = await query.CountAsync();

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
                RequestStatus = (int)x.Status,
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

    public async Task<PagingSearchOrderDto<RequestListDto>> GetReceivedRequestsPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        int currentUserRecipientParentId,
        int currentUserRecipientId,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblRequest> query = dbContext.TblRequests;

        query = query.Where(q =>
            (q.ReceiverId == currentUserRecipientParentId ||
             q.ReceiverId == currentUserRecipientId) &&
            q.Status == RequestStatusEnum.Sent);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x => x.RequestNumber.Contains(search) || x.SendDate.ToString().Contains(search));
        }

        query = sortField switch
        {
            "SendDate" => sortOrder == "asc" ? query.OrderBy(x => x.SendDate) : query.OrderByDescending(x => x.SendDate),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id)
        };

        paging.TotalItems = await query.CountAsync();

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
                RequestStatus = (int)x.Status!,
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
    public async Task<ItemAvailabilityDto> GetItemAvailabilityAsync(
    int itemId,
    int recipientId,
    CancellationToken token)
    {
        var storeIds = await dbContext.TblStores
            .Where(s => s.RecipientId == recipientId)
            .Select(s => s.Id)
            .ToListAsync(token);

        var baseQuery = dbContext.TblUidInfo
            .Where(u =>
                u.OrderInfo.FkItemId == itemId &&
                storeIds.Contains(u.CurrentStore ?? 0));

        var arivCount = await baseQuery.CountAsync(token);
        var usedCount = await baseQuery.CountAsync(u => u.Used, token);
        var currentCount = await baseQuery.CountAsync(u => !u.Used, token);

        return new ItemAvailabilityDto
        {
            ArivCount = arivCount,
            UsedCount = usedCount,
            CurrentCount = currentCount
        };
    }

    public Task<int> GetRequestCountByShamsiYearAsync(int shamsiYear, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<TblRequest?> GetEntityById(int id, CancellationToken token)
    {
        return await dbContext.TblRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, token);
    }
}
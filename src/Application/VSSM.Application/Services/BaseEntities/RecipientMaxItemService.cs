using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.RecipientMaxItem;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class RecipientMaxItemService(
    VSSMDbContext dbContext, 
    IValidator<RecipientMaxItemDto> validator) : IRecipientMaxItemService
{
    public async Task<int> Create(RecipientMaxItemDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = new TblRecipientMaxItem
        {
            ItemId = dto.ItemId,
            RecipientId = dto.RecipientId,
            NumberDose = dto.NumberDose,
            EntryDate = dto.EntryDate,
            ReceiverId = dto.ReceiverId,
            RelatedToItem = dto.RelatedToItem
        };

        await dbContext.TblRecipientMaxItems.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.RecipientMaxItemId;
    }

    public async Task<PagingSearchOrderDto<RecipientMaxItemDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        IQueryable<TblRecipientMaxItem> query = dbContext.TblRecipientMaxItems.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.RecipientMaxItemId.ToString().Contains(search) ||
                (x.ItemId.HasValue && x.ItemId.ToString().Contains(search)) ||
                (x.RecipientId.HasValue && x.RecipientId.ToString().Contains(search))
            );
        }

        query = sortField switch
        {
            "ItemId" => sortOrder == "asc" ? query.OrderBy(x => x.ItemId) : query.OrderByDescending(x => x.ItemId),
            "RecipientId" => sortOrder == "asc" ? query.OrderBy(x => x.RecipientId) : query.OrderByDescending(x => x.RecipientId),
            "NumberDose" => sortOrder == "asc" ? query.OrderBy(x => x.NumberDose) : query.OrderByDescending(x => x.NumberDose),
            "EntryDate" => sortOrder == "asc" ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
            "ReceiverId" => sortOrder == "asc" ? query.OrderBy(x => x.ReceiverId) : query.OrderByDescending(x => x.ReceiverId),
            _ => sortOrder == "asc" ? query.OrderBy(x => x.RecipientMaxItemId) : query.OrderByDescending(x => x.RecipientMaxItemId),
        };

        paging.TotalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new RecipientMaxItemDto
            {
                RecipientMaxItemId = x.RecipientMaxItemId,
                ItemId = x.ItemId,
                RecipientId = x.RecipientId,
                NumberDose = x.NumberDose,
                EntryDate = x.EntryDate,
                ReceiverId = x.ReceiverId,
                RelatedToItem = x.RelatedToItem
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<RecipientMaxItemDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<RecipientMaxItemDto> GetById(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblRecipientMaxItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.RecipientMaxItemId == id, cancellationToken);

        if (entity == null)
            return null;

        return new RecipientMaxItemDto
        {
            RecipientMaxItemId = entity.RecipientMaxItemId,
            ItemId = entity.ItemId,
            RecipientId = entity.RecipientId,
            NumberDose = entity.NumberDose,
            EntryDate = entity.EntryDate,
            ReceiverId = entity.ReceiverId,
            RelatedToItem = entity.RelatedToItem
        };
    }

    public async Task<bool> Update(RecipientMaxItemDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await dbContext.TblRecipientMaxItems
            .FirstOrDefaultAsync(x => x.RecipientMaxItemId == dto.RecipientMaxItemId, cancellationToken);

        if (entity == null)
            return false;

        entity.ItemId = dto.ItemId;
        entity.RecipientId = dto.RecipientId;
        entity.NumberDose = dto.NumberDose;
        entity.EntryDate = dto.EntryDate;
        entity.ReceiverId = dto.ReceiverId;
        entity.RelatedToItem = dto.RelatedToItem;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.TblRecipientMaxItems
            .FirstOrDefaultAsync(x => x.RecipientMaxItemId == id, cancellationToken);

        if (entity == null)
            return false;

        dbContext.TblRecipientMaxItems.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

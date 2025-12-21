using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Adjustments;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class AdjustmentService : IAdjustmentService
{
    private readonly VSSMDbContext _dbContext;
    private readonly IValidator<AdjustmentDto> _validator;

    public AdjustmentService(
        VSSMDbContext dbContext,
        IValidator<AdjustmentDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task<int> Create(
        AdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = new TblAdjustment
        {
            EntryDate = DateTime.Now,
            Date = dto.Date,
            Note = dto.Note,
            AdjustmentreasonId = dto.AdjustmentreasonId,
            NewQuantity = dto.NewQuantity,
            OldQuantity = dto.OldQuantity,
            ArrivalLineId = dto.ArrivalLineId,
            DispatchLineId = dto.DispatchLineId,
            EnteredById = dto.EnteredById,
            FiscalPeriodId = dto.FiscalPeriodId,
            ItemDetailStoreId = dto.ItemDetailStoreId
        };

        await _dbContext.TblAdjustments.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return entity.AdjustmentId;
    }

    public async Task<PagingSearchOrderDto<AdjustmentDto>> GetPagedList(
        string? search,
        int page,
        int pageSize,
        string sortField,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel
        {
            Page = page,
            PageSize = pageSize
        };

        IQueryable<TblAdjustment> query =
            _dbContext.TblAdjustments.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.AdjustmentId.ToString().Contains(search) ||
                (x.Note != null && x.Note.Contains(search))
            );
        }

        query = sortField switch
        {
            "Date" => sortOrder == "asc"
                ? query.OrderBy(x => x.Date)
                : query.OrderByDescending(x => x.Date),

            "EntryDate" => sortOrder == "asc"
                ? query.OrderBy(x => x.EntryDate)
                : query.OrderByDescending(x => x.EntryDate),

            "NewQuantity" => sortOrder == "asc"
                ? query.OrderBy(x => x.NewQuantity)
                : query.OrderByDescending(x => x.NewQuantity),

            _ => sortOrder == "asc"
                ? query.OrderBy(x => x.AdjustmentId)
                : query.OrderByDescending(x => x.AdjustmentId)
        };

        paging.TotalItems =
            await query.CountAsync(cancellationToken);

        var items = await query
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .Select(x => new AdjustmentDto
            {
                AdjustmentId = x.AdjustmentId,
                EntryDate = x.EntryDate,
                Date = x.Date,
                Note = x.Note,
                AdjustmentreasonId = x.AdjustmentreasonId,
                NewQuantity = x.NewQuantity,
                OldQuantity = x.OldQuantity,
                ArrivalLineId = x.ArrivalLineId,
                DispatchLineId = x.DispatchLineId,
                EnteredById = x.EnteredById,
                FiscalPeriodId = x.FiscalPeriodId,
                ItemDetailStoreId = x.ItemDetailStoreId
            })
            .ToListAsync(cancellationToken);

        return new PagingSearchOrderDto<AdjustmentDto>
        {
            Items = items,
            Paging = paging,
            SearchText = search,
            SortField = sortField,
            SortOrder = sortOrder
        };
    }

    public async Task<AdjustmentDto?> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustments
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == id,
                cancellationToken);

        if (entity is null)
            return null;

        return new AdjustmentDto
        {
            AdjustmentId = entity.AdjustmentId,
            EntryDate = entity.EntryDate,
            Date = entity.Date,
            Note = entity.Note,
            AdjustmentreasonId = entity.AdjustmentreasonId,
            NewQuantity = entity.NewQuantity,
            OldQuantity = entity.OldQuantity,
            ArrivalLineId = entity.ArrivalLineId,
            DispatchLineId = entity.DispatchLineId,
            EnteredById = entity.EnteredById,
            FiscalPeriodId = entity.FiscalPeriodId,
            ItemDetailStoreId = entity.ItemDetailStoreId
        };
    }

    public async Task<bool> Update(
        AdjustmentDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var entity = await _dbContext.TblAdjustments
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == dto.AdjustmentId,
                cancellationToken);

        if (entity is null)
            return false;

        entity.Date = dto.Date;
        entity.Note = dto.Note;
        entity.AdjustmentreasonId = dto.AdjustmentreasonId;
        entity.NewQuantity = dto.NewQuantity;
        entity.OldQuantity = dto.OldQuantity;
        entity.ArrivalLineId = dto.ArrivalLineId;
        entity.DispatchLineId = dto.DispatchLineId;
        entity.EnteredById = dto.EnteredById;
        entity.FiscalPeriodId = dto.FiscalPeriodId;
        entity.ItemDetailStoreId = dto.ItemDetailStoreId;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> Delete(
        int id,
        CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TblAdjustments
            .FirstOrDefaultAsync(
                x => x.AdjustmentId == id,
                cancellationToken);

        if (entity is null)
            return false;

        _dbContext.TblAdjustments.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }
}

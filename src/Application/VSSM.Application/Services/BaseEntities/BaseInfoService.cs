using FluentValidation;
using Microsoft.EntityFrameworkCore;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.BaseInfo;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Application.Settings;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities
{
    public class BaseInfoService(
        VSSMDbContext dbContext,
        ISecuritySettingsProvider securitySettings) : IBaseInfoService
    {
        public async Task AddAsync(BaseInfoInputDto model, int currentUserId)
        {
            var currentUser = await dbContext.TblUsers.FindAsync(currentUserId);

            var baseInfo = new TblBaseInfo()
            {
                Title = model.Title,
                Code = model.Code,          
                ParentId = model.ParentId,
                Order = model.Order,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                CreateBy = currentUser?.UserName ?? string.Empty,
                UpdateBy = currentUser?.UserName ?? string.Empty,
            };

            await dbContext.AddAsync(baseInfo);
            await dbContext.SaveChangesAsync();

            if (IsSecurityKey(baseInfo.Title))
                securitySettings.Invalidate();
        }

        public async Task<BaseInfoDto> GetByIdAsync(int id)
        {
            var baseInfo = await dbContext.TblBaseInfo.FindAsync(id);

            if (baseInfo is null)
                return null!;

            return new BaseInfoDto()
            {
                ParentId = baseInfo.ParentId,
                Code = baseInfo.Code,
                Id = baseInfo.BaseInfoId,
                Title = baseInfo.Title,
                Order = baseInfo.Order,
            };
        }
        public async Task<(List<BaseInfoDto> data, int count)> GetPagedBaseInfoAsync(
            string? search, int page, int pageSize, CancellationToken cancellationToken)
        {
            var query = dbContext.TblBaseInfo
                .Include(t => t.Parent)
                .Select(s => new BaseInfoDto()
                {
                    Id = s.BaseInfoId,
                    ParentId = s.ParentId,
                    Title = s.Title,
                    Code = s.Code,
                    UpdatedDate = s.UpdateDate,
                    ParentTitle = s.Parent != null ? s.Parent.Title : string.Empty
                });

            query = query.OrderByDescending(t => t.Id).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(d =>
                    d.Title!.Contains(search) ||
                    (!string.IsNullOrEmpty(d.Code) && d.Code.Contains(search)) ||
                    (!string.IsNullOrEmpty(d.ParentTitle) && d.ParentTitle.Contains(search))
                );
            }

            var total = await query.CountAsync(cancellationToken);

            return (await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken), total);
        }

        public async Task RemoveAsync(int id)
        {
            var entity = await dbContext.TblBaseInfo.FindAsync(id);
            if (entity is null) return;

            var wasSecurityKey = IsSecurityKey(entity.Title);

            dbContext.TblBaseInfo.Remove(entity);
            await dbContext.SaveChangesAsync();

            if (wasSecurityKey)
                securitySettings.Invalidate();
        }

        public async Task UpdateAsync(BaseInfoInputDto model, int currentUserId)
        {
            var baseInfo = await dbContext.TblBaseInfo.FindAsync(model.BaseInfoId)
                ?? throw new InvalidOperationException($"BaseInfo {model.BaseInfoId} not found.");

            var currentUser = await dbContext.TblUsers.FindAsync(currentUserId);

            baseInfo.UpdateDate = DateTime.Now;
            baseInfo.UpdateBy = currentUser?.UserName ?? string.Empty;
            baseInfo.Title = model.Title;
            baseInfo.Code = model.Code;    
            baseInfo.Order = model.Order;

            await dbContext.SaveChangesAsync();

            if (IsSecurityKey(baseInfo.Title))
                securitySettings.Invalidate();
        }

        public async Task<List<BaseInfoDto>> GetByParentIdAsync(
            int parentId, CancellationToken cancellationToken = default)
        {
            return await dbContext.TblBaseInfo
                .Where(x => x.ParentId == parentId)
                .OrderBy(x => x.Order)
                .Select(x => new BaseInfoDto
                {
                    Id = x.BaseInfoId,
                    ParentId = x.ParentId,
                    Title = x.Title,
                    Code = x.Code,
                    Order = x.Order,
                })
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
        private static bool IsSecurityKey(string? title) =>
            SecuritySettings.IsSecurityTitle(title);
    }
}
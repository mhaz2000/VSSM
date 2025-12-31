using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.Item;
using VSSM.Application.DTOs.RequestEntities;
using VSSM.Application.Interfaces.RequestEntities;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.RequestEntities
{
    public class RequestService : IRequestService
    {
        private readonly VSSMDbContext _dbContext;
        public RequestService(VSSMDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagingSearchOrderDto<RequestListDto>> GetPagedList(string? search, int page, int pageSize, string sortField, string sortOrder, CancellationToken cancellationToken)
        {
            var paging = new PagingModel { Page = page, PageSize = pageSize };

            IQueryable<TblRequest> query = _dbContext.TblRequests;

            // 1) Search
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x => x.RequestNumber.Contains(search) || x.SendDate.ToString().Contains(search)
                );
            }

            // 2) Sorting
            //query = sortField switch
            //{
            //    "Name" => sortOrder == "asc" ? query.OrderBy(x => x.FullName) : query.OrderByDescending(x => x.FullName),
            //    "Code" => sortOrder == "asc" ? query.OrderBy(x => x.Code) : query.OrderByDescending(x => x.Code),
            //    _ => sortOrder == "asc" ? query.OrderBy(x => x.OrderId) : query.OrderByDescending(x => x.OrderId)
            //};

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
                    RequestStatus = x.Status
                })
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

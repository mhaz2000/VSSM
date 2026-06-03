using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;
using VSSM.Application.DTOs;
using VSSM.Application.DTOs.BaseEntities.OrderInfo;
using VSSM.Application.Helpers;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Domain.Enums;
using VSSM.Infrastructure.Data.Models;

namespace VSSM.Application.Services.BaseEntities;

public class OrderInfoService : IOrderInfoService
{
    private readonly VSSMDbContext _dbContext;
    public OrderInfoService(VSSMDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OrderInfoDto?> GetConfirmedByBatchNum(string batchNum, CancellationToken token)
    {
        return await _dbContext.TblOrderInfo
            .AsNoTracking()
            .Where(o => o.BatchNumber == batchNum && o.Status == OrderInfoStatus.READ.ToString())
            .OrderByDescending(o => o.PkOrderInfo)
            .Select(o => new OrderInfoDto
            {
                Id = o.PkOrderInfo,
                BatchNumber = o.BatchNumber,
                FileName = o.FileName,
                ExpirationDate = o.ExpirationDate,
                ManufactureDate = o.ManufactureDate,
                Status = o.Status.ToString(),
                IsConfirmed = true,
                CreationTime = o.CreationTime.ToShamsi()
            })
            .FirstOrDefaultAsync(token);
    }



    public async Task<(List<OrderInfoDto> data, int count)> GetPagedAsync(string? search, int page, int pageSize, string sortField, string sortOrder,
        CancellationToken cancellationToken)
    {
        var paging = new PagingModel { Page = page, PageSize = pageSize };

        var query = from o in _dbContext.TblOrderInfo
                    join u in _dbContext.TblUidInfo on o.PkOrderInfo equals u.FkOrderInfo
                    join user in _dbContext.TblUsers on o.CreatedBy equals user.UserId
                    group new { o, u, user }
                    by new { o.Status, o.PkOrderInfo, o.FileName, o.CreationTime, o.CreatedBy, o.BatchNumber, user.FirstName, user.LastName }
                    into g
                    select new OrderInfoDto()
                    {
                        Id = g.Key.PkOrderInfo,
                        Status = g.Key.Status.ToString(),
                        FileName = g.Key.FileName,
                        CreationTime = g.Key.CreationTime.ToShamsi(),
                        CreatedBy = $"{g.Key.FirstName} {g.Key.LastName}",
                        BatchNumber = g.Key.BatchNumber,
                        Quantity = g.Count()
                    };

        query = query.OrderByDescending(t => t.Id).AsNoTracking();

        var total = await query.CountAsync();

        query = query.ApplySorting(sortField, sortOrder == "desc");

        return (await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(), total);
    }

    public async Task RemoveAsync(int id, CancellationToken cancellationToken)
    {
        _dbContext.TblUidInfo.RemoveRange(_dbContext.TblUidInfo.Where(t => t.FkOrderInfo == id));
        _dbContext.TblOrderInfo.Remove(_dbContext.TblOrderInfo.Find(id));

        await _dbContext.SaveChangesAsync();
    }

    public async Task UploadAsync(XDocument doc, string fileName, int userId)
    {
        XElement od = doc.Root;

        string orderNo = od.Attribute("NO")?.Value;
        string manufactureDate = od.Attribute("MD")?.Value;
        string expirationDate = od.Attribute("ED")?.Value;
        string irc = od.Attribute("SC")?.Value;
        string batchNumber = od.Attribute("BN")?.Value;
        string distributorCompanyCode = od.Attribute("DC")?.Value;
        string orderCompanyCode = od.Attribute("OC")?.Value;
        string licenseCode = od.Attribute("LC")?.Value;
        string prefix = od.Attribute("PX")?.Value;


        // Read all SP nodes
        var spNodes = od.Elements("SP");

        var ordreInfo = new TblOrderInfo()
        {
            NumberOrder = int.Parse(orderNo),
            ManufactureDate = DateTime.Parse(manufactureDate),
            ExpirationDate = DateTime.Parse(expirationDate),
            Irc = irc,
            BatchNumber = batchNumber,
            DistributorCompanyCode = distributorCompanyCode,
            OrderCompanyCode = orderCompanyCode,
            LicenseCode = licenseCode,
            Prefix = prefix,
            CreationTime = DateTime.Now,
            FileName = fileName,
            CreatedBy = userId,
            Status = OrderInfoStatus.READ.ToString(),
        };

        _dbContext.TblOrderInfo.Add(ordreInfo);

        foreach (var sp in spNodes)
        {
            string pbc = sp.Attribute("PBC")?.Value;


            foreach (var tc in sp.Elements("TC"))
            {
                string hc = tc.Attribute("HC")?.Value;
                string bc = tc.Attribute("BC")?.Value;

                    var uidInfo = new TblUidInfo()
                    {
                        PatchBarcode = pbc,
                        Barcode = bc,
                        HashCode = hc,
                        SecLevelUid = bc.Substring(18, 20),
                        FirstLevelUid = pbc.Substring(18, 20),
                        IsActive = true,
                        OrderInfo = ordreInfo
                    };

                _dbContext.TblUidInfo.Add(uidInfo);
            }
        }

        await _dbContext.SaveChangesAsync();
    }
}

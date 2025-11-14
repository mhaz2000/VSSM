using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VSSM.Application.DTOs.BaseEntities.TblRoadType;
using VSSM.Application.Interfaces.BaseEntities;
using VSSM.Infrastructure.Data.Models;

public class TblRoadTypeService : ITblRoadTypeService
{
    private readonly AppDbContext _context;

    public TblRoadTypeService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TblRoadTypeGetDto>> GetAll()
    {
        return await _context.TblRoadTypes
            .Select(x => new TblRoadTypeGetDto
            {
                RoadTypeId = x.RoadTypeId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }

    public async Task<TblRoadTypeEditDto> GetById(int id)
    {
        return await _context.TblRoadTypes
            .Where(x => x.RoadTypeId == id)
            .Select(x => new TblRoadTypeEditDto
            {
                RoadTypeId = x.RoadTypeId,
                Code = x.Code,
                FullName = x.FullName,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    public async Task<int> Create(TblRoadTypeCreateDto dto)
    {
        var entity = new TblRoadType
        {
            Code = dto.Code,
            FullName = dto.FullName,
            IsActive = dto.IsActive
        };

        _context.TblRoadTypes.Add(entity);
        return await _context.SaveChangesAsync();
    }

    public async Task<int> Edit(TblRoadTypeEditDto dto)
    {
        var entity = await _context.TblRoadTypes.FindAsync(dto.RoadTypeId);

        entity.Code = dto.Code;
        entity.FullName = dto.FullName;
        entity.IsActive = dto.IsActive;

        return await _context.SaveChangesAsync();
    }
}


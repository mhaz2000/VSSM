using VSSM.Application.DTOs.Dashboard;

namespace VSSM.Application.Interfaces.BaseEntities;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(int recipientId);
}

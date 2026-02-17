using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.Common.Interfaces
{
    public interface IMenuPermissionService
    {
        Task<bool> HasAccessAsync(int roleId, string controller, string action);
    }
}

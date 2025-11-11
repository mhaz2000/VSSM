using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSSM.Application.DTOs
{
    public class PagingSearchOrderDto<T>
    {
        public IEnumerable<T> Items { get; set; }

        // Search parameters
        public string? SearchText { get; set; }

        // Paging
        public PagingModel Paging { get; set; }

        // Sorting
        public string SortField { get; set; } = "Id";
        public string SortOrder { get; set; } = "desc";  // asc or desc
    }
}

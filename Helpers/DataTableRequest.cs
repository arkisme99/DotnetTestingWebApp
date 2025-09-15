using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Helpers
{
    public class DataTableRequest
    {
        public string? Draw { get; set; }
        public int Start { get; set; }
        public int Length { get; set; }
        public string? SearchValue { get; set; }
        public string? SortColumn { get; set; }
        public string? SortDir { get; set; }
    }
}
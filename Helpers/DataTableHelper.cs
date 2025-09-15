using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetTestingWebApp.Helpers
{
    public static class DataTableHelper
    {
        public static DataTableRequest GetDataTableRequest(HttpRequest request)
        {
            return new DataTableRequest
            {
                Draw = request.Form["draw"].FirstOrDefault(),
                Start = Convert.ToInt32(request.Form["start"].FirstOrDefault() ?? "0"),
                Length = Convert.ToInt32(request.Form["length"].FirstOrDefault() ?? "10"),
                SearchValue = request.Form["search[value]"].FirstOrDefault(),
                SortColumn = request.Form["order[0][column]"].FirstOrDefault(),
                SortDir = request.Form["order[0][dir]"].FirstOrDefault()
            };
        }
    }
}
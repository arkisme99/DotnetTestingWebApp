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
            var form = request.Form;
            var req = new DataTableRequest
            {
                Draw = int.TryParse(form["draw"], out var d) ? d : 0,
                Start = int.TryParse(form["start"], out var s) ? s : 0,
                Length = int.TryParse(form["length"], out var l) ? l : 10,
                Search = new Search
                {
                    Value = form["search[value]"].FirstOrDefault() ?? "",
                    Regex = form["search[regex]"].FirstOrDefault() == "true"
                }
            };

            // Ambil semua columns
            var colIndex = 0;
            while (form.ContainsKey($"columns[{colIndex}][data]"))
            {
                var col = new Column
                {
                    Data = form[$"columns[{colIndex}][data]"].FirstOrDefault() ?? "",
                    Name = form[$"columns[{colIndex}][name]"].FirstOrDefault() ?? "",
                    Searchable = form[$"columns[{colIndex}][searchable]"].FirstOrDefault() == "true",
                    Orderable = form[$"columns[{colIndex}][orderable]"].FirstOrDefault() == "true",
                    Search = new Search
                    {
                        Value = form[$"columns[{colIndex}][search][value]"].FirstOrDefault() ?? "",
                        Regex = form[$"columns[{colIndex}][search][regex]"].FirstOrDefault() == "true"
                    }
                };
                req.Columns.Add(col);
                colIndex++;
            }

            // Ambil semua order
            var orderIndex = 0;
            while (form.ContainsKey($"order[{orderIndex}][column]"))
            {
                var ord = new Order
                {
                    Column = int.TryParse(form[$"order[{orderIndex}][column]"], out var c) ? c : 0,
                    Dir = form[$"order[{orderIndex}][dir]"].FirstOrDefault() ?? "asc"
                };
                req.Order.Add(ord);
                orderIndex++;
            }

            return req;
        }
    }
}
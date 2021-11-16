using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GS.MFH.Lease.Api.Infrastructure.DataTables
{
    /// <summary>
    /// DataTable params model binder
    /// </summary>
    public class DataTableParamModelBinder : IModelBinder
    {
        /// <summary>
        /// Bind te bidingContext to a DataTableRequestModel result
        /// </summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var request = bindingContext.HttpContext.Request.Query;
            if (request.Count == 0)
            {
                bindingContext.Model = new DataTableRequestModel();
                return Task.FromResult(true);
            }

            // Retrieve request Data
            var draw = Convert.ToInt32(request["Draw"]);
            var start = Convert.ToInt32(request["start"]);
            var length = Convert.ToInt32(request["length"]);

            // DataTablesSearch
            var search = new DataTablesSearch
            {
                Value = request["search[value]"],
                Regex = Convert.ToBoolean(request["search[regex]"])
            };

            // DataTablesOrder
            var o = 0;
            var order = new List<DataTablesOrder>();
            while (request.ContainsKey("order[" + o + "][column]"))
            {
                order.Add(new DataTablesOrder
                {
                    Column = Convert.ToInt32(request["order[" + o + "][column]"]),
                    Dir = request["order[" + o + "][dir]"]
                });
                o++;
            }

            // Columns
            var c = 0;
            var columns = new List<DataTablesColumn>();
            while (request.ContainsKey("columns[" + c + "][name]"))
            {
                columns.Add(new DataTablesColumn
                {
                    Data = request["columns[" + c + "][Data]"],
                    Name = request["columns[" + c + "][name]"],
                    Orderable = Convert.ToBoolean(request["columns[" + c + "][orderable]"]),
                    Searchable = Convert.ToBoolean(request["columns[" + c + "][searchable]"]),
                    Search = new DataTablesSearch
                    {
                        Value = request["columns[" + c + "][search][value]"],
                        Regex = Convert.ToBoolean(request["columns[" + c + "][search][regex]"])
                    }
                });
                c++;
            }

            bindingContext.Result = ModelBindingResult.Success(new DataTableRequestModel
            {
                Draw = draw,
                Start = start,
                Length = length,
                Search = search,
                Order = order,
                Columns = columns
            });

            return Task.FromResult(0);
        }
    }
}
using System.Collections.Generic;

namespace Api.Infrastructure.DataTables
{
    /// <summary>
    /// DataTable request model definition
    /// </summary>
    public class DataTableRequestModel
    {
        /// <summary>
        /// Draw counter. This is used by DataTables to ensure that the Ajax returns from server-side processing requests are drawn in sequence by DataTables (Ajax requests are asynchronous and thus can return out of sequence). This is used as part of the draw return parameter (see below).
        /// </summary>
        public int Draw { get; set; }
        /// <summary>
        /// Paging first record indicator. This is the start point in the current data set (0 index based - i.e. 0 is the first record).
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// Number of records that the table can display in the current draw. It is expected that the number of records returned will be equal to this number, unless the server has fewer records to return. Note that this can be -1 to indicate that all records should be returned (although that negates any benefits of server-side processing!)
        /// </summary>
        public int Length { get; set; }
        /// <summary>
        /// List of Columns with the search column by index e.g = columns[i][data] = 'Data'
        /// </summary>
        public List<DataTablesColumn> Columns { get; set; }
        /// <summary>
        /// Search global params
        /// </summary>
        public DataTablesSearch Search { get; set; }
        /// <summary>
        /// List of order by column dfeinition e.g = order[i][value] = 'asc'
        /// </summary>
        public List<DataTablesOrder> Order { get; set; }
    }

    /// <summary>
    /// DataTable column definition
    /// </summary>
    public class DataTablesColumn
    {
        /// <summary>
        /// Column's data source, as defined by columns.data.
        /// </summary>
        public string Data { get; set; }
        /// <summary>
        /// Column's name, as defined by columns.name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Flag to indicate if this column is searchable (true) or not (false). This is controlled by columns.searchable.
        /// </summary>
        public bool Searchable { get; set; }
        /// <summary>
        /// Flag to indicate if this column is orderable (true) or not (false). This is controlled by columns.orderable.
        /// </summary>
        public bool Orderable { get; set; }
        /// <summary>
        /// Search options for the columns [search][value]=string, [search][regex]
        /// </summary>
        public DataTablesSearch Search { get; set; }
    }

    /// <summary>
    /// DataTable order definition
    /// </summary>
    public class DataTablesOrder
    {
        /// <summary>
        /// Column to which ordering should be applied. This is an index reference to the columns array of information that is also submitted to the server.
        /// </summary>
        public int Column { get; set; }
        /// <summary>
        /// Ordering direction for this column. It will be asc or desc to indicate ascending ordering or descending ordering, respectively.
        /// </summary>
        public string Dir { get; set; }
    }

    /// <summary>
    /// DataTable search definition
    /// </summary>
    public class DataTablesSearch
    {
        /// <summary>
        /// Global search value. To be applied to all columns which have searchable as true.
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// true if the global filter should be treated as a regular expression for advanced searching, false otherwise. Note that normally server-side processing scripts will not perform regular expression searching for performance reasons on large data sets, but it is technically possible and at the discretion of your script.
        /// </summary>
        public bool Regex { get; set; }
    }

    /// <summary>
    /// DataTables Response Model
    /// </summary>
    /// <typeparam name="TEntity">Type of List Data</typeparam>
    public class DataTablesResponse<TEntity> where TEntity : class
    {
        /// <summary>
        /// The draw counter that this object is a response to - from the draw parameter sent as part of the data request. Note that it is strongly recommended for security reasons that you cast this parameter to an integer, rather than simply echoing back to the client what it sent in the draw parameter, in order to prevent Cross Site Scripting (XSS) attacks.
        /// </summary>
        public int Draw { get; set; }
        /// <summary>
        /// Total records, before filtering (i.e. the total number of records in the database)
        /// </summary>
        public int RecordsTotal { get; set; }
        /// <summary>
        /// Total records, after filtering (i.e. the total number of records after filtering has been applied - not just the number of records being returned for this page of data).
        /// </summary>
        public int RecordsFiltered { get; set; }
        /// <summary>
        /// The data to be displayed in the table. This is an array of data source objects, one for each row, which will be used by DataTables. Note that this parameter's name can be changed using the ajax option's dataSrc property.
        /// </summary>
        public IList<TEntity> Data { get; set; }
    }
}
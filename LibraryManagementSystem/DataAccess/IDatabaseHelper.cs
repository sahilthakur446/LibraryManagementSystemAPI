using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagementSystem.DataAccess
{
    public interface IDatabaseHelper
    {
        Task<bool> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false, bool useTransaction = false);
        Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false);
        Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false);
        Task<DataSet> ExecuteMultipleQueryUsingFillAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false);
        Task<DataSet> ExecuteQueryResultingMultipleDataTableAsync(string query);
    }
}

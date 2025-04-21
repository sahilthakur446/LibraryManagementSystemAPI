using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using LibraryManagementSystem.Exceptions;

namespace LibraryManagementSystem.DataAccess
{
    public class DatabaseHelper : IDatabaseHelper
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseHelper> _logger;

        public DatabaseHelper(IConfiguration configuration, ILogger<DatabaseHelper> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration.");
            }
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task<SqlConnection> GetConnectionAsync()
        {
            try
            {
                var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                return connection;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex,"Failed to connect to the database in GetConnectionAsync.");
                throw new DataAccessException("A database connection error occurred.",ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetConnectionAsync.");
                throw new DataAccessException("An unexpected error occurred while establishing a database connection.", ex);
            }
        }

        public async Task<bool> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false, bool useTransaction = false)
        {
            await using var connection = await GetConnectionAsync();
            SqlTransaction transaction = null;

            if (useTransaction)
                transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand(query, connection, transaction);
                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                int result = await command.ExecuteNonQueryAsync();

                if (useTransaction)
                    await transaction.CommitAsync();

                _logger.LogInformation("Executed non-query successfully: {Query}", query);
                return result > 0;
            }
            catch (SqlException ex)
            {
                if (useTransaction)
                    await transaction?.RollbackAsync();

                _logger.LogError(ex, "SQL Exception in ExecuteNonQueryAsync.");
                throw new DataAccessException("A SQL error occurred while executing a non-query operation.", ex);
            }
            catch (InvalidOperationException ex)
            {
                if (useTransaction)
                    await transaction?.RollbackAsync();

                _logger.LogError(ex, "Invalid Operation Exception in ExecuteNonQueryAsync.");
                throw new DataAccessException("Invalid operation while executing a non-query operation.", ex);
            }
            catch (Exception ex)
            {
                if (useTransaction)
                    await transaction?.RollbackAsync();

                _logger.LogError(ex, "Unexpected error in ExecuteNonQueryAsync.");
                throw new DataAccessException("An unexpected error occurred while executing a non-query operation.", ex);
            }
        }

        public async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false)
        {
            await using var connection = await GetConnectionAsync();
            try
            {
                using var command = new SqlCommand(query, connection);
                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                var result = await command.ExecuteScalarAsync();
                _logger.LogInformation("Executed scalar query successfully: {Query}", query);
                return result;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Exception in ExecuteScalarAsync.");
                throw new DataAccessException("A SQL error occurred while executing a scalar query.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid Operation Exception in ExecuteScalarAsync.");
                throw new DataAccessException("Invalid operation while executing a scalar query.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExecuteScalarAsync.");
                throw new DataAccessException("An unexpected error occurred while executing a scalar query.", ex);
            }
        }

        public async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false)
        {
            try
            {
                await using var connection = await GetConnectionAsync();
                using var command = new SqlCommand(query, connection);
                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                using var reader = await command.ExecuteReaderAsync();
                var dataTable = new DataTable();
                dataTable.Load(reader);
                _logger.LogInformation("Executed query successfully: {Query}", query);
                return dataTable;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Exception in ExecuteQueryAsync.");
                throw new DataAccessException("A SQL error occurred while executing a query.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid Operation Exception in ExecuteQueryAsync.");
                throw new DataAccessException("Invalid operation while executing a query.", ex);
            }
            catch (DataAccessException ex)
            {
                _logger.LogError(ex, "SQL Exception in ExecuteQueryAsync.");
                throw new DataAccessException("A SQL error occurred while executing a query.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExecuteQueryAsync.");
                throw new DataAccessException("An unexpected error occurred while executing a query.", ex);
            }
        }

        public async Task<DataSet> ExecuteMultipleQueryUsingFillAsync(string query, SqlParameter[] parameters = null, bool isStoredProcedure = false)
        {
            await using var connection = await GetConnectionAsync();
            try
            {
                using var command = new SqlCommand(query, connection);
                if (isStoredProcedure)
                    command.CommandType = CommandType.StoredProcedure;

                if (parameters != null)
                    command.Parameters.AddRange(parameters);

                using var adapter = new SqlDataAdapter(command);
                var dataSet = new DataSet();
                adapter.Fill(dataSet);
                _logger.LogInformation("Executed multiple query fill successfully: {Query}", query);
                return dataSet;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Exception in ExecuteMultipleQueryUsingFillAsync.");
                throw new DataAccessException("A SQL error occurred while executing a multiple-result query.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid Operation Exception in ExecuteMultipleQueryUsingFillAsync.");
                throw new DataAccessException("Invalid operation while executing a multiple-result query.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExecuteMultipleQueryUsingFillAsync.");
                throw new DataAccessException("An unexpected error occurred while executing a multiple-result query.", ex);
            }
        }

        public async Task<DataSet> ExecuteQueryResultingMultipleDataTableAsync(string query)
        {
            await using var connection = await GetConnectionAsync();
            try
            {
                using var command = new SqlCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                var ds = new DataSet();
                int tableIndex = 0;
                do
                {
                    var dt = new DataTable($"Table{tableIndex}");
                    dt.Load(reader);
                    ds.Tables.Add(dt);
                    tableIndex++;
                } while (reader.NextResult());

                _logger.LogInformation("Executed query resulting in multiple data tables successfully: {Query}", query);
                return ds;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL Exception in ExecuteQueryResultingMultipleDataTableAsync.");
                throw new DataAccessException("A SQL error occurred while executing a multiple-result query.", ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid Operation Exception in ExecuteQueryResultingMultipleDataTableAsync.");
                throw new DataAccessException("Invalid operation while executing a multiple-result query.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in ExecuteQueryResultingMultipleDataTableAsync.");
                throw new DataAccessException("An unexpected error occurred while executing a multiple-result query.", ex);
            }
        }
    }
}
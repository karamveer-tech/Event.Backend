using eventManager.DataTableExtension;
using eventManager.Model;
using MySql.Data.MySqlClient;
using System.Data;
using System.Text;

namespace eventManager
{
    public class DataBaseUtil
    {
        private readonly string? _dbSQLStirng;
        private const string whereClause = " WHERE ";
        private const string nullValue = "(NULL)";
        private const string andOperator = " AND ";
        private const string equalOperator = " = ";
        private const string inOperator = " in ";
        private const string selectClause = "SELECT ";
        private const string fromClause = " FROM ";
        private const string groupByClause = " GROUP BY ";

        public DataBaseUtil(IConfiguration _config)
        {
            var dbHost = Environment.GetEnvironmentVariable("DB_HOST");
            var dbUserName = Environment.GetEnvironmentVariable("DB_USERNAME");
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");
            var dbDataBase = Environment.GetEnvironmentVariable("DB_DATABASE");
            _dbSQLStirng = _config.GetConnectionString("DefaultConnection");

            // Build the connection string
            var dbConnectionStr = $"Server={dbHost};Database={dbDataBase};Uid={dbUserName};Pwd={dbPassword};";
            _dbSQLStirng = dbConnectionStr + _dbSQLStirng;
        }

        #region Dynamic queries

        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public async Task<Int32> SaveScalar<T>(Dictionary<string, string> parameters) where T : class, new()
        {
            var tableName = typeof(T).Name;
            var queryBuilder = new StringBuilder();

            // Start building the INSERT statement
            queryBuilder.Append("INSERT INTO ").Append(tableName).Append(" (");

            // Append the column names
            foreach (var parameter in parameters)
            {
                queryBuilder.Append(parameter.Key).Append(',');
            }

            // Remove the trailing comma if there are parameters
            if (parameters.Count > 0)
                queryBuilder.Remove(queryBuilder.Length - 1, 1);

            queryBuilder.Append(") ");

            // Add VALUES part of the query
            queryBuilder.Append("VALUES (");

            // Append the values, wrapping them in single quotes
            foreach (var parameter in parameters)
            {
                queryBuilder.Append('\'').Append(parameter.Value).Append("',");
            }

            // Remove the trailing comma from the values part
            if (parameters.Count > 0)
                queryBuilder.Remove(queryBuilder.Length - 1, 1);

            queryBuilder.Append(");");

            // Convert StringBuilder to string
            var querystring = queryBuilder.ToString();

            // Execute the query
            return ExecuteScalarInsert(querystring);
        }

        public async Task<int> UpdateRecord<T>(Dictionary<string, string> updateColumn, Dictionary<string, string> condition, string operatorCondition = " AND ") where T : class, new()
        {
            if (updateColumn.Count == 0 || condition.Count == 0)
            {
                return 0;
            }

            var queryString = new StringBuilder();
            queryString.Append("UPDATE ").Append(typeof(T).Name).Append(' ');

            // Append SET clause
            queryString.Append(BuildSetClause(updateColumn));

            // Append WHERE clause
            queryString.Append(BuildConditionClause(condition, operatorCondition));

            return await UpdateExecute(queryString.ToString());
        }

        private static string BuildSetClause(Dictionary<string, string> updateColumn)
        {
            var setClause = new StringBuilder("SET ");
            int index = 0;

            foreach (var column in updateColumn)
            {
                if (index > 0)
                {
                    setClause.Append(", ");
                }

                if (string.IsNullOrEmpty(column.Value) || column.Value == nullValue)
                {
                    setClause.Append(column.Key).Append(" = NULL");
                }
                else
                {
                    setClause.Append(column.Key).Append(" = '").Append(column.Value.Replace("\\", "\\\\").Replace("'", "''")).Append('\'');
                }

                index++;
            }

            return setClause.ToString();
        }

        private static string BuildConditionClause(Dictionary<string, string> condition, string operatorCondition)
        {
            var conditionClause = new StringBuilder(whereClause);
            int index = 0;

            foreach (var cond in condition)
            {
                if (index > 0)
                {
                    conditionClause.Append(operatorCondition);
                }

                conditionClause.Append(cond.Key).Append(" = '").Append(cond.Value.Replace("\\", "\\\\").Replace("'", "''")).Append('\'');
                index++;
            }

            return conditionClause.ToString();
        }

        public async Task<int> DeleteRecord<T>(Dictionary<string, string> parameters) where T : class, new()
        {
            if (parameters.Count == 0)
            {
                return 0; // Nothing to delete if no conditions are provided
            }

            var queryString = new StringBuilder();
            queryString.Append("DELETE FROM ").Append(typeof(T).Name);

            // Append WHERE clause
            queryString.Append(BuildConditionClause(parameters, andOperator));

            return await ExecuteDelete(queryString.ToString());
        }

        public async Task<int> DeleteMultipleRecords<T>(Dictionary<string, string> parameters) where T : class, new()
        {
            if (parameters.Count == 0)
            {
                return 0; // Nothing to delete if no conditions are provided
            }

            var queryString = new StringBuilder();
            queryString.Append("DELETE FROM ").Append(typeof(T).Name);

            // Append WHERE clause
            queryString.Append(BuildConditionInClause(parameters, andOperator));

            return await ExecuteDelete(queryString.ToString());
        }

        private static string BuildConditionInClause(Dictionary<string, string> condition, string operatorCondition)
        {
            var conditionClause = new StringBuilder(whereClause);
            int index = 0;

            foreach (var cond in condition)
            {
                if (index > 0)
                {
                    conditionClause.Append(operatorCondition);
                }

                conditionClause.Append(cond.Key).Append(" in ").Append("(" + cond.Value + ") ");
                index++;
            }

            return conditionClause.ToString();
        }

        public async Task<T> GetSingleRecordFromSingleTable<T>(Dictionary<string, string> parameters, string columns) where T : class, new()
        {
            // Default column selection to * if none provided
            string columnNames = string.IsNullOrEmpty(columns) ? "*" : columns;

            // Start building the query
            var queryString = new StringBuilder();
            queryString.Append(selectClause).Append(columnNames).Append(fromClause).Append(typeof(T).Name);

            // Add WHERE clause if there are parameters
            if (parameters.Count > 0)
            {
                queryString.Append(BuildConditionClause(parameters, andOperator));
            }

            // Execute the query and convert the result to the specified type
            var dataTable = await GetDataTableSQL(queryString.ToString());
            return Extensions.ToFromDataTable<T>(dataTable);
        }

        public async Task<List<T>> GetMultipleRecordFromSingleTable<T>(Dictionary<string, string> parameters, Dictionary<string, string> columnList) where T : class, new()
        {
            // Set column names: default to "*" if columnList is empty
            string columnNames = columnList.Count == 0 ? " * " : string.Join(",", columnList.Keys);

            // Build the query string
            var queryString = new StringBuilder();
            queryString.Append(selectClause).Append(columnNames).Append(fromClause).Append(typeof(T).Name);

            // Append WHERE clause if there are parameters
            if (parameters.Count > 0)
            {
                queryString.Append(BuildConditionClause(parameters, andOperator));
            }

            // Execute the query and convert the result to a list of T
            var dataTable = await GetDataTableSQL(queryString.ToString());
            return Extensions.ToListFromDataTable<T>(dataTable);
        }

        public async Task<List<T>> GetMultipleRecordFromSingleTable<T>(Dictionary<string, string> parameters, string columnList) where T : class, new()
        {
            // Use "*" if columnList is empty, otherwise use provided columns
            string columnNames = string.IsNullOrEmpty(columnList) ? "*" : columnList;

            // Build the base query
            var queryString = new StringBuilder();
            queryString.Append(selectClause).Append(columnNames).Append(fromClause).Append(typeof(T).Name);

            // Append WHERE clause if parameters exist
            if (parameters.Count > 0)
            {
                queryString.Append(BuildConditionClause(parameters, andOperator));
            }

            // Execute the query and convert the result to a list of T
            var dataTable = await GetDataTableSQL(queryString.ToString());
            return Extensions.ToListFromDataTable<T>(dataTable);
        }

        #endregion

        #region Call methods by query

        public async Task<Int32> InsertExecute(string querystring)
        {
            return await Task.Run(() => ExecuteScalarInsert(querystring));
        }

        public async Task<int> UpdateExecute(string queryString)
        {
            var connection = new MySqlConnection();
            var adapter = new MySqlDataAdapter();
            var returnVal = 1;
            try
            {
                connection = new MySqlConnection(_dbSQLStirng);
                connection.Open();
                adapter.UpdateCommand = new MySqlCommand(queryString, connection);
                await adapter.UpdateCommand.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception)
            {
                returnVal = 0;
            }
            finally
            {
                connection.Dispose();
            }

            return returnVal;
        }

        public async Task<int> ExecuteDelete(string queryString)
        {
            var connection = new MySqlConnection();
            var adapter = new MySqlDataAdapter();
            var returnVal = 1;

            try
            {
                connection = new MySqlConnection(_dbSQLStirng);
                connection.Open();
                adapter.DeleteCommand = new MySqlCommand(queryString, connection);
                await adapter.DeleteCommand.ExecuteNonQueryAsync();
                connection.Close();
            }
            catch (Exception)
            {
                returnVal = 0;
            }
            finally
            {
                connection.Dispose();
            }

            return returnVal;
        }

        public async Task<T> GetSingleRecordFromQuery<T>(string sqlQuery) where T : class, new()
        {
            var _datatableObj = await GetDataTableSQL(sqlQuery);
            return Extensions.ToFromDataTable<T>(_datatableObj);
        }

        public async Task<List<T>> GetMultipleRecordFromQuery<T>(string sqlQuery) where T : class, new()
        {
            var _datatableObj = await GetDataTableSQL(sqlQuery);
            return Extensions.ToListFromDataTable<T>(_datatableObj);
        }

        public List<T> GetMultipleRecordFromQueryV2<T>(string sqlQuery) where T : class, new()
        {
            var _datatableObj = GetDataTableSQLV2(sqlQuery);
            return Extensions.ToListFromDataTable<T>(_datatableObj);
        }

        private async Task<DataTable> GetDataTableSQL(string queryString)
        {
            var dt = new DataTable();

            MySqlDataAdapter? da = new(queryString, _dbSQLStirng);
            await da.FillAsync(dt);
            return dt;
        }

        private DataTable GetDataTableSQLV2(string queryString)
        {
            var dt = new DataTable();

            MySqlDataAdapter? da = new(queryString, _dbSQLStirng);
            da.FillAsync(dt);
            return dt;
        }

        public Int32 ExecuteScalarInsert(string queryString)
        {
            Int32 returnVal = 1;
            using var connection = new MySqlConnection(_dbSQLStirng);
            try
            {
                connection.Open();
                using var command = new MySqlCommand(queryString, connection);
                // Execute the command and use ExecuteScalar if needed
                command.ExecuteScalar();
            }
            catch (Exception)
            {
                connection.Close();
                returnVal = 0;
            }
            return returnVal;
        }

        #endregion

        public int SaveExecuteNonQuery<T>(Dictionary<string, string> parameters) where T : class, new()
        {
            var queryString = GetSaveExecuteNonQuery<T>(parameters);

            // Execute the query
            return ExecuteScalarInsert(queryString.ToString());
        }

        public string GetSaveExecuteNonQuery<T>(Dictionary<string, string> parameters) where T : class, new()
        {
            var queryString = new StringBuilder();

            // Start building the INSERT INTO part
            queryString.Append("INSERT INTO ").Append(typeof(T).Name).Append(" (");

            // Append column names and values using helper methods
            queryString.Append(BuildColumnsClause(parameters.Keys)).Append(") VALUES (");
            queryString.Append(BuildValuesClause(parameters)).Append(");");

            // Execute the query
            return queryString.ToString();
        }

        // Helper method to build the column names part of the query
        private static string BuildColumnsClause(IEnumerable<string> columns)
        {
            var columnBuilder = new StringBuilder();
            foreach (var column in columns)
            {
                columnBuilder.Append(column).Append(',');
            }

            if (columnBuilder.Length > 0)
                columnBuilder.Length--; // Remove the trailing comma

            return columnBuilder.ToString();
        }

        // Helper method to build the values part of the query
        private static string BuildValuesClause(Dictionary<string, string> parameters)
        {
            const string NullValueLiteral = "NULL";
            var valuesBuilder = new StringBuilder();

            foreach (var value in parameters.Values)
            {
                if (!string.IsNullOrEmpty(value) && value != NullValueLiteral)
                {
                    string escapedValue = value.Replace("\\", "\\\\").Replace("'", "''").Trim(); // Escape single quotes
                    valuesBuilder.Append('\'').Append(escapedValue).Append("',");
                }
                else
                {
                    valuesBuilder.Append(NullValueLiteral).Append(',');
                }
            }

            if (valuesBuilder.Length > 0)
                valuesBuilder.Length--; // Remove the trailing comma

            return valuesBuilder.ToString();
        }

        public async Task<bool> CheckUniqueRecordFromTable(long id, string tableName, string tableColumn, string name, bool isActiveCheck = true)
        {
            var queryBuilder = new StringBuilder();
            string formattedName = FormatValue(name);

            BuildSelectClause(queryBuilder, tableName, tableColumn, formattedName, isActiveCheck);
            BuildWhereClause(queryBuilder, id);

            string finalQuery = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(finalQuery);

            return temp.id > 0;
        }

        private static void BuildSelectClause(StringBuilder queryBuilder, string tableName, string tableColumn, string formattedName, bool isActiveCheck)
        {
            queryBuilder.Append("SELECT id FROM ")
                        .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(equalOperator)
                        .Append(formattedName);

            if (isActiveCheck)
                queryBuilder.Append(" AND isactive = 1");
        }

        public async Task<bool> CheckUniqueRecordByIdWithClient(long id, string tableName, Dictionary<string, string> whereParameters)
        {
            var queryBuilder = new StringBuilder();

            BuildSelectClause(queryBuilder, tableName);
            AppendWhereConditions(queryBuilder, whereParameters);
            AppendIdCondition(queryBuilder, id);

            string finalQuery = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(finalQuery);

            return temp.id > 0;
        }

        public async Task<bool> CheckUniqueRecordByGuidWithClient(string guid, string tableName, Dictionary<string, string> whereParameters)
        {
            var queryBuilder = new StringBuilder();

            BuildSelectClause(queryBuilder, tableName);
            AppendWhereConditions(queryBuilder, whereParameters);
            AppendGuidCondition(queryBuilder, guid);

            string finalQuery = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(finalQuery);

            return temp.id > 0;
        }

        public async Task<bool> CheckUniqueRecordFromTableGUID(string guid, string tableName, string tableColumn, string name)
        {
            var queryBuilder = new StringBuilder();
            string formattedName = FormatValue(name);
            BuildSelectClause(queryBuilder, tableName, tableColumn, formattedName, true);
            AppendGuidCondition(queryBuilder, guid);

            string finalQuery = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(finalQuery);

            return temp.id > 0;
        }

        public async Task<long> getIdByGuid(string guid, string tableName, string tableColumn)
        {
            if (string.IsNullOrEmpty(guid)) return 0;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT id FROM ")
                        .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(equalOperator)
                        .Append(FormatValue(guid))
                        .Append(" AND isactive = 1");

            string query = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(query);

            return temp.id > 0 ? temp.id : 0;
        }
        public async Task<long> getIdByEmail(string email, string tableName, string tableColumn)
        {
            if (string.IsNullOrEmpty(email)) return 0;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT id FROM ")
                        .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(equalOperator)
                        .Append(email);

            string query = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(query);

            return temp.id > 0 ? temp.id : 0;
        }

        public async Task<string> getMultipleIdById(string id, string getColumnNameId, string tableName, string tableColumn)
        {
            if (string.IsNullOrEmpty(id)) return "";

            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT GROUP_CONCAT(" + getColumnNameId + ") as ids FROM ")
                        .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(equalOperator)
                        .Append(FormatValue(id))
                        .Append(groupByClause + tableColumn);

            string query = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(query);

            return !string.IsNullOrEmpty(temp.ids) ? temp.ids : "";
        }
        public async Task<List<string>> GetMultipleIdsByIdUsingInOperator(string ids, string getColumnNameId, string tableName, string tableColumn)
        {
            if (string.IsNullOrEmpty(ids)) return new List<string>();

            // Use a StringBuilder to safely build the query string
            var queryBuilder = new StringBuilder();
            queryBuilder.Append("SELECT GROUP_CONCAT(" + getColumnNameId + ") as ids FROM ")
                       .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(inOperator)
                        .Append("(" + ids + ")")
                        .Append(groupByClause)
                        .Append(tableColumn);

            string query = queryBuilder.ToString();
            // Execute the query and fetch the result
            var temp = await GetMultipleRecordFromQuery<Temp_Model>(query);

            // Use LINQ to flatten and split the 'ids' into a single list
            return temp
                .Where(temp => !string.IsNullOrEmpty(temp?.ids))  // Filter out null or empty ids
                .SelectMany(temp => temp.ids.Split(','))           // Split the ids and flatten into a single sequence
                .ToList();
        }

        public async Task<long> getIdByGuid(string guid, string columnNameId, string tableName, string tableColumn, bool isActiveCheck = true)
        {
            if (string.IsNullOrEmpty(guid)) return 0;

            var queryBuilder = new StringBuilder();
            queryBuilder.Append(selectClause)
                        .Append(columnNameId)
                        .Append(" AS id FROM ")
                        .Append(tableName)
                        .Append(whereClause)
                        .Append(tableColumn)
                        .Append(equalOperator)
                        .Append(FormatValue(guid));

            if (isActiveCheck)
            {
                queryBuilder.Append(" AND isactive = 1");
            }

            string query = queryBuilder.ToString();
            var temp = await GetSingleRecordFromQuery<Temp_Model>(query);

            return temp.id > 0 ? temp.id : 0;
        }

        private void AppendIdCondition(StringBuilder queryBuilder, long id)
        {
            if (id > 0)
            {
                queryBuilder.Append(" AND id <> ").Append(id);
            }
        }

        private void AppendGuidCondition(StringBuilder queryBuilder, string guid)
        {
            if (!string.IsNullOrEmpty(guid))
            {
                queryBuilder.Append(" AND guid <> ").Append(FormatValue(guid));
            }
        }
        private void BuildSelectClause(StringBuilder queryBuilder, string tableName)
        {
            queryBuilder.Append("SELECT id FROM ")
                        .Append(tableName)
                        .Append(" WHERE isactive = 1");
        }

        private void AppendWhereConditions(StringBuilder queryBuilder, Dictionary<string, string> whereParameters)
        {
            foreach (var param in whereParameters)
            {
                var formattedValue = FormatValue(param.Value);
                queryBuilder.Append(andOperator)
                            .Append(param.Key.ToLower())
                            .Append(equalOperator)
                            .Append(formattedValue);
            }
        }

        private void BuildWhereClause(StringBuilder queryBuilder, long id)
        {
            if (id > 0)
            {
                queryBuilder.Append(" AND id <> ").Append(id);
            }
        }

        private string FormatValue(string value)
        {
            if (!string.IsNullOrEmpty(value))
                value = value.Replace("\\", "\\\\").Replace("'", "''").Trim(); // Escape single quotes

            return string.IsNullOrWhiteSpace(value) ? "NULL" : $"'{value}'";
        }

    }
}

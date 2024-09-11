using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace API.Data
{
    public class DataContextDapper
    {
        private readonly IConfiguration _config;
        public DataContextDapper(IConfiguration config) 
        { 
            _config = config;
        }

        /// <summary>
        /// Executes the provided SQL query and returns the result as an IEnumerable of type T.
        /// </summary>
        public IEnumerable<T> LoadData<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            
            return dbConnection.Query<T>(sql);
        }

        /// <summary>
        /// Executes the provided SQL query and returns the result as a single value of type T.
        /// </summary>
        public T LoadDataSingle<T>(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));

            return dbConnection.QuerySingle<T>(sql);
        }

        /// <summary>
        /// Executes the provided SQL query and returns a boolean indicating whether any rows were affected.
        /// </summary>
        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            
            return dbConnection.Execute(sql) > 0;
        }

        /// <summary>
        /// Executes the provided SQL query and returns the number of rows affected by the query.
        /// </summary>
        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            
            return dbConnection.Execute(sql);
        }
    }
}

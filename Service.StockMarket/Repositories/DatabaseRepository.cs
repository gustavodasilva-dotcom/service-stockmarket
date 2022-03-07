using Dapper;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Service.StockMarket.Repositories
{
    public class DatabaseRepository
    {
        private readonly string _defaultConnection;

        public DatabaseRepository()
        {
            _defaultConnection = ConfigurationManager.ConnectionStrings["Default"].ToString();
        }

        public dynamic Execute(string query)
        {
            try
            {
                using (var db = new SqlConnection(_defaultConnection))
                {
                    return db.Query(query, commandTimeout: 30);
                }
            }
            catch (Exception) { throw; }
        }
    }
}

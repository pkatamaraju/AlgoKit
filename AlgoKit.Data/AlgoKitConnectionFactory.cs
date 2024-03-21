using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using AlgoKit.Data.Core.Interfaces;

namespace AlgoKit.Data
{
   public class AlgoKitConnectionFactory:IAlgoKitConnectionFactory
    {
        private readonly string _connectionString;
        public AlgoKitConnectionFactory(IConfiguration configuration)
        {
            string sqlConnectionConfigName = "DevServer";
            
            _connectionString = configuration.GetConnectionString(sqlConnectionConfigName);
        }

        public SqlConnection CreateConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public string GetConnectionString()
        {
            return _connectionString;
        }
    }
}

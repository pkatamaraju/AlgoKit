using System.Data.SqlClient;

namespace AlgoKit.Data.Core.Interfaces
{
  public interface IAlgoKitConnectionFactory
    {
        SqlConnection CreateConnection();
        string GetConnectionString();
    }
}

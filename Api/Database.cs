using System.Data;
using System.Data.SQLite;

namespace Api;

public class Database
{
    private readonly IDbConnection _connection;

    public Database(string connectionString)
    {
        _connection = new SQLiteConnection(connectionString);
        _connection.Open();
    }
    
    public IDbConnection GetConnection() => _connection;

    public void Dispose()
    {
        if (_connection.State != ConnectionState.Closed)
        {
            _connection.Close();
        }
    }
}
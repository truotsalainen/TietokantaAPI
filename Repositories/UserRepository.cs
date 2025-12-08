using Microsoft.Data.Sqlite;

public class UserRepository
{
    private readonly string _connectionString;
    public UserRepository(string databasePath) => _connectionString = $"Data Source={databasePath}";

    public int CreateUser(string username, string passwordHash)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES ($username, $hash); SELECT last_insert_rowid();";
        cmd.Parameters.AddWithValue("$username", username);
        cmd.Parameters.AddWithValue("$hash", passwordHash);
        var result = cmd.ExecuteScalar();
        long id = result != null ? (long)result : throw new InvalidOperationException("Käyttäjän luominen epäonnistui.");
        return (int)id;
    }

    public (int Id, string PasswordHash)? GetByUsername(string username)
    {
        using var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, PasswordHash FROM Users WHERE Username = $username LIMIT 1";
        cmd.Parameters.AddWithValue("$username", username);

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (reader.GetInt32(0), reader.GetString(1));
        }
        return null;
    }
}

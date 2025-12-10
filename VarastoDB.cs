namespace TietokantaAPI;

using System.Data;
using Microsoft.Data.Sqlite;
using System.Text;

// ========================================
// TIETUEET
// ========================================

public record Tuote(string tag, string nimi, int maara, string kunto);
public record VarastoTiedot(int Id, string Nimi);

// Käyttäjätietue (Program.cs tarvitsee tämän kääntyäkseen)
public record User(int Id, string Username, string PasswordHash)
{
    // Käytetään arvoa, jotta se toimii Program.cs:n kanssa
    public (int Id, string Username, string PasswordHash) Value => (Id, Username, PasswordHash);
}

// ========================================
// TIETOKANTALUOKKA
// ========================================

public class VarastoDB
{
    private readonly string _connectionString;

    // Asetetaan yhteyden polku Program.cs:stä (esim. "varasto.db")
    public VarastoDB(string dbPath)
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    // ----------------------------------------
    // 🛢️ Yleinen apumetodi tietokannan alustukseen
    // ----------------------------------------
    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // 1. Users table (Käyttäjät)
            using (var createUsers = connection.CreateCommand())
            {
                createUsers.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Users(
                        Id INTEGER PRIMARY KEY,
                        Username TEXT UNIQUE NOT NULL,
                        PasswordHash TEXT NOT NULL
                    );";
                createUsers.ExecuteNonQuery();
            }

            // 2. Varastot table (Linkitetty käyttäjiin)
            using (var createVarasto = connection.CreateCommand())
            {
                createVarasto.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Varastot(
                        Id INTEGER PRIMARY KEY,
                        Nimi TEXT NOT NULL,
                        UserId INTEGER NOT NULL,
                        FOREIGN KEY(UserId) REFERENCES Users(Id) ON DELETE CASCADE
                    );";
                createVarasto.ExecuteNonQuery();
            }

            // 3. Tuotteet table (Linkitetty varastoihin)
            using (var createTuotteetTableCmd = connection.CreateCommand())
            {
                createTuotteetTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tuotteet (
                        Id INTEGER PRIMARY KEY,
                        Tag TEXT,
                        Nimi TEXT NOT NULL,
                        Maara INTEGER NOT NULL,
                        Kunto TEXT,
                        VarastoId INTEGER NOT NULL,
                        FOREIGN KEY(VarastoId) REFERENCES Varastot(Id) ON DELETE CASCADE
                    );";
                createTuotteetTableCmd.ExecuteNonQuery();
            }
        }
    }

    // ----------------------------------------
    // 🔐 Autentikointi
    // ----------------------------------------

    // Hae käyttäjä
    public User? GetUser(string username)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                // Huom: Käytännössä PasswordHashia ei koskaan pitäisi hakea näin
                cmd.CommandText = "SELECT Id, Username, PasswordHash FROM Users WHERE Username = @Username LIMIT 1";
                cmd.Parameters.AddWithValue("@Username", username);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User(reader.GetInt32(0), reader.GetString(1), reader.GetString(2));
                    }
                    return null;
                }
            }
        }
    }

    // Lisää uusi käyttäjä
    public void AddUser(string username, string password)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                // Huom: Käytännössä salasana pitäisi hashata, tässä käytetään salaamatonta tekstiä Program.cs:n takia.
                cmd.CommandText = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                cmd.Parameters.AddWithValue("@Username", username);
                cmd.Parameters.AddWithValue("@PasswordHash", password); 
                cmd.ExecuteNonQuery();
            }
        }
    }

    // ----------------------------------------
    // 🏢 Varastot
    // ----------------------------------------

    // Tarkista varaston omistajuus
    private bool CheckVarastoOwnership(int varastoId, int userId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM Varastot WHERE Id = @VarastoId AND UserId = @UserId";
                cmd.Parameters.AddWithValue("@VarastoId", varastoId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                return Convert.ToInt64(cmd.ExecuteScalar()) > 0;
            }
        }
    }

    // Luo uusi varasto (Program.cs vaatii tämän)
    public int LuoVarasto(string nimi, int userId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            using (var addVarasto = connection.CreateCommand())
            {
                addVarasto.CommandText = "INSERT INTO Varastot (Nimi, UserId) VALUES (@Nimi, @UserId); SELECT last_insert_rowid();";
                addVarasto.Parameters.AddWithValue("@Nimi", nimi);
                addVarasto.Parameters.AddWithValue("@UserId", userId);
                var result = addVarasto.ExecuteScalar();
                return result != null ? (int)(long)result : throw new InvalidOperationException("Varaston luominen epäonnistui.");
            }
        }
    }

    public List<Tuote> EtsiTuotteet(string column, string value)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            var results = new List<Tuote>();
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Nimi FROM Varastot WHERE UserId = @UserId";
            cmd.Parameters.AddWithValue("@UserId", userId);

            var searchItemCommand = connection.CreateCommand();
            searchItemCommand.CommandText = $@"
                SELECT Id, Nimi, Tag, Kunto, Maara, VarastoId
                FROM Tuotteet
                WHERE LOWER({column}) = LOWER($Value)
                AND VarastoId = $VarastoId;";
            searchItemCommand.Parameters.AddWithValue("$Value", value);
            searchItemCommand.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            while (reader.Read())
            {
                while (reader.Read())
                {
                    results.Add(new Tuote(
                        reader.GetInt32(0),  // Id
                        reader.GetString(2), // Tag
                        reader.GetString(1), // Nimi
                        reader.GetInt32(4),  // Maara
                        reader.GetString(3), // Kunto
                        reader.GetInt32(5)   // VarastoId
                    ));
                }
            }

            return results;
        }
}
    public int LuoVarasto(string nimi)
    {
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            return false;
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Varastot WHERE Id = @Id AND UserId = @UserId";
            deleteCmd.Parameters.AddWithValue("@Id", varastoId);
            deleteCmd.Parameters.AddWithValue("@UserId", userId);
            
            // Tuotteet poistuvat automaattisesti FOREIGN KEY CASCADE:n ansiosta
            return deleteCmd.ExecuteNonQuery() > 0;
        }
    }

    // ----------------------------------------
    // 📦 Tuotteet
    // ----------------------------------------

    // Hae tuotteet varastosta (Program.cs vaatii tämän)
    public List<Tuote> HaeTuotteet(int varastoId, int userId)
    {
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            throw new UnauthorizedAccessException("Käyttäjällä ei ole oikeutta tähän varastoon.");
        }

        var tuotteet = new List<Tuote>();
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT Tag, Nimi, Maara, Kunto 
                FROM Tuotteet 
                WHERE VarastoId = @VarastoId";
            cmd.Parameters.AddWithValue("@VarastoId", varastoId);

            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    tuotteet.Add(new Tuote(
                        reader.IsDBNull(0) ? "" : reader.GetString(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.IsDBNull(3) ? "" : reader.GetString(3)));
                }
            }
        }
        return tuotteet;
    }

    // Lisää tai päivitä tuote (Program.cs vaatii tämän)
    public void LisaaTaiPaivitaTuote(int varastoId, Tuote tuote, int userId)
    {
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            throw new UnauthorizedAccessException("Käyttäjällä ei ole oikeutta tähän varastoon.");
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // 1. Yritä päivittää olemassa oleva tuote (Tag, Nimi, Kunto match)
            using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = @"
                SELECT Id, Maara
                FROM Tuotteet
                WHERE Tag = @Tag AND Nimi = @Nimi AND Kunto = @Kunto AND VarastoId = @VarastoId
                LIMIT 1;";
                checkCmd.Parameters.AddWithValue("@Tag", tuote.tag);
                checkCmd.Parameters.AddWithValue("@Nimi", tuote.nimi);
                checkCmd.Parameters.AddWithValue("@Kunto", tuote.kunto);
                checkCmd.Parameters.AddWithValue("@VarastoId", varastoId);

                using (var reader = checkCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int oldMaara = reader.GetInt32(1);
                        int newMaara = oldMaara + tuote.maara;

                        reader.Close();

                        using (var updateCmd = connection.CreateCommand())
                        {
                            updateCmd.CommandText = "UPDATE Tuotteet SET Maara = @Maara WHERE Id = @Id";
                            updateCmd.Parameters.AddWithValue("@Maara", newMaara);
                            updateCmd.Parameters.AddWithValue("@Id", id);
                            updateCmd.ExecuteNonQuery();
                        }
                        return;
                    }
                }
            }

            // 2. Lisää uusi tuote.
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"
                INSERT INTO Tuotteet (Tag, Nimi, Maara, Kunto, VarastoId)
                VALUES (@Tag, @Nimi, @Maara, @Kunto, @VarastoId);";
                insertCmd.Parameters.AddWithValue("@Tag", tuote.tag);
                insertCmd.Parameters.AddWithValue("@Nimi", tuote.nimi);
                insertCmd.Parameters.AddWithValue("@Maara", tuote.maara);
                insertCmd.Parameters.AddWithValue("@Kunto", tuote.kunto);
                insertCmd.Parameters.AddWithValue("@VarastoId", varastoId);
                insertCmd.ExecuteNonQuery();
            }
        }
    }
    
    // Muokkaa tuotetta ID:n perusteella (Program.cs vaatii tämän)
    public bool MuokkaaTuote(int tuoteId, int varastoId, Tuote tuote, int userId)
    {
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            throw new UnauthorizedAccessException("Käyttäjällä ei ole oikeutta tähän varastoon.");
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Tuotteet 
                SET Nimi = @Nimi, Maara = @Maara, Tag = @Tag, Kunto = @Kunto
                WHERE Id = @TuoteId AND VarastoId = @VarastoId";

            updateCmd.Parameters.AddWithValue("@TuoteId", tuoteId);
            updateCmd.Parameters.AddWithValue("@Nimi", tuote.nimi);
            updateCmd.Parameters.AddWithValue("@Maara", tuote.maara);
            updateCmd.Parameters.AddWithValue("@Tag", tuote.tag);
            updateCmd.Parameters.AddWithValue("@Kunto", tuote.kunto);
            updateCmd.Parameters.AddWithValue("@VarastoId", varastoId);

            return updateCmd.ExecuteNonQuery() > 0;
        }
    }

    // Poista tuote ID:n perusteella (Program.cs vaatii tämän)
    public bool PoistaTuote(int tuoteId, int varastoId, int userId)
    {
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            throw new UnauthorizedAccessException("Käyttäjällä ei ole oikeutta tähän varastoon.");
        }
        
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var komento = connection.CreateCommand();
            komento.CommandText = "DELETE FROM Tuotteet WHERE Id = @Id AND VarastoId = @VarastoId";
            komento.Parameters.AddWithValue("@Id", tuoteId);
            komento.Parameters.AddWithValue("@VarastoId", varastoId);

            return komento.ExecuteNonQuery() > 0;
        }
    }
    
    // Poista tuote/tuotteet NIMEN perusteella (Program.cs vaatii tämän)
    public void PoistaTuote(string nimi, int varastoId, int userId) 
    { 
        if (!CheckVarastoOwnership(varastoId, userId))
        {
            throw new UnauthorizedAccessException("Käyttäjällä ei ole oikeutta tähän varastoon.");
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();
            var deleteItemCmd = connection.CreateCommand();
            deleteItemCmd.CommandText = @"
                DELETE FROM Tuotteet
                WHERE Nimi = @Nimi AND VarastoId = @VarastoId;";
            deleteItemCmd.Parameters.AddWithValue("@Nimi", nimi);
            deleteItemCmd.Parameters.AddWithValue("@VarastoId", varastoId);
            
            if (deleteItemCmd.ExecuteNonQuery() == 0)
            {
                 throw new KeyNotFoundException($"Tuotetta nimeltä '{nimi}' ei löytynyt varastosta {varastoId}.");
            }

            Console.WriteLine($"[DB] Poistettiin tuote(et) nimeltä '{nimi}' varastosta {varastoId} (käyttäjä {userId})"); 
        }
    }
}
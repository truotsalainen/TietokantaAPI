namespace TietokantaAPI;

using System.Data;
using Microsoft.Data.Sqlite;
using System.Globalization;

public record Tuote(int Id, string Tag, string Nimi, int Maara, string Kunto, int VarastoId);

public record VarastoTiedot(int Id, string Nimi);

public class VarastoDB
{
    private static string _connectionString = "Data Source=VarastoDB.db";
    private int? currentVarastoId = null; // Valittu varasto.

    public VarastoDB()
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using (var createVarasto = connection.CreateCommand())
        {
            createVarasto.CommandText = @"
                CREATE TABLE IF NOT EXISTS Varastot(
                    Id INTEGER PRIMARY KEY,
                    Nimi TEXT
                );";
            createVarasto.ExecuteNonQuery();
        }

        using (var createTuotteet = connection.CreateCommand())
        {
            createTuotteet.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tuotteet (
                    Id INTEGER PRIMARY KEY,
                    Tag TEXT,
                    Nimi TEXT,
                    Maara INTEGER,
                    Kunto TEXT,
                    VarastoId INTEGER,
                    FOREIGN KEY(VarastoId) REFERENCES Varastot(Id) ON DELETE CASCADE
                );";
            createTuotteet.ExecuteNonQuery();
        }
    }


    // Lisää tuote varastoon.
    public void LisaaTuote(string tag, string nimi, int maara, string kunto)
    {
        if (currentVarastoId == null)
        {
            throw new InvalidOperationException("Et ole valinnut varastoa. Lataa varasto ensin. ");
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Etsi onko tuote jo olemassa tässä varastossa.
            using (var checkCmd = connection.CreateCommand())
            {
                checkCmd.CommandText = @"
                SELECT Id, Maara
                FROM Tuotteet
                WHERE Tag = $Tag AND Nimi = $Nimi AND Kunto = $Kunto AND VarastoId = $VarastoId
                LIMIT 1;";
                checkCmd.Parameters.AddWithValue("$Tag", tag);
                checkCmd.Parameters.AddWithValue("$Nimi", nimi);
                checkCmd.Parameters.AddWithValue("$Kunto", kunto);
                checkCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

                using (var reader = checkCmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int oldMaara = reader.GetInt32(1);
                        int newMaara = oldMaara + maara;

                        reader.Close();
                        using (var updateCmd = connection.CreateCommand())
                        {
                            updateCmd.CommandText = "UPDATE Tuotteet SET Maara = $Maara WHERE Id = $Id";
                            updateCmd.Parameters.AddWithValue("$Maara", newMaara);
                            updateCmd.Parameters.AddWithValue("$Id", id);
                            updateCmd.ExecuteNonQuery();
                        }

                        return;
                    }
                }
            }

            // Lisää uusi tuote.
            using (var insertCmd = connection.CreateCommand())
            {
                insertCmd.CommandText = @"
                INSERT INTO Tuotteet (Tag, Nimi, Maara, Kunto, VarastoId)
                VALUES ($Tag, $Nimi, $Maara, $Kunto, $VarastoId);";
                insertCmd.Parameters.AddWithValue("$Tag", tag);
                insertCmd.Parameters.AddWithValue("$Nimi", nimi);
                insertCmd.Parameters.AddWithValue("$Maara", maara);
                insertCmd.Parameters.AddWithValue("$Kunto", kunto);
                insertCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    // Poista tuote tietokannasta.
    public void PoistaTuote(string Nimi)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var deleteItemCmd = connection.CreateCommand();
            deleteItemCmd.CommandText = @"
                DELETE FROM Tuotteet
                WHERE Nimi = $Nimi AND VarastoId = $VarastoId;";
            deleteItemCmd.Parameters.AddWithValue("$Nimi", Nimi);
            deleteItemCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);
            deleteItemCmd.ExecuteNonQuery();

            Console.WriteLine("Tuote poistettu varastosta.");
        }
    }


    public IResult MuokkaaTuote(int Id, Tuote muokattuTuote)
    {
        if (currentVarastoId == null)
            return Results.BadRequest(new { message = "Ei aktiivista varastoa." });

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // korvataan olemassaoleva tuote uudella
            var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = @"
                UPDATE Tuotteet 
                SET Nimi = $Nimi, Maara = $Maara, Tag = $Tag, Kunto = $Kunto
                WHERE Id = $Id AND VarastoId = $VarastoId";

            updateCmd.Parameters.AddWithValue("$Id", Id);
            updateCmd.Parameters.AddWithValue("$Nimi", muokattuTuote.Nimi);
            updateCmd.Parameters.AddWithValue("$Maara", muokattuTuote.Maara);
            updateCmd.Parameters.AddWithValue("$Tag", muokattuTuote.Tag);
            updateCmd.Parameters.AddWithValue("$Kunto", muokattuTuote.Kunto);
            updateCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            if (updateCmd.ExecuteNonQuery() == 0)
                return Results.NotFound(new { message = "Tuotetta ei löytynyt"});

            // Tarkistetaan identtiset tuotteet
            var mergeCmd = connection.CreateCommand();
            mergeCmd.CommandText = @"
                SELECT Id, Maara FROM Tuotteet
                WHERE Id != $Id AND Nimi = $Nimi AND Tag = $Tag AND Kunto = $Kunto AND VarastoId = $VarastoId
                LIMIT 1";
            mergeCmd.Parameters.AddWithValue("$Id", Id);
            mergeCmd.Parameters.AddWithValue("$Nimi", muokattuTuote.Nimi);
            mergeCmd.Parameters.AddWithValue("$Tag", muokattuTuote.Tag);
            mergeCmd.Parameters.AddWithValue("$Kunto", muokattuTuote.Kunto);
            mergeCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            int? vanhaId = null;
            int vanhaMaara = 0;

            using (var mergeReader = mergeCmd.ExecuteReader())
            {
                if (mergeReader.Read())
                {
                    vanhaId = mergeReader.GetInt32(0);
                    vanhaMaara = mergeReader.GetInt32(1);
                }
            }

            if (vanhaId.HasValue)
            {
                int combinedMaara = vanhaMaara + muokattuTuote.Maara;

                // Update existing row
                var updateExisting = connection.CreateCommand();
                updateExisting.CommandText = "UPDATE Tuotteet SET Maara = $Maara WHERE Id = $Id";
                updateExisting.Parameters.AddWithValue("$Maara", combinedMaara);
                updateExisting.Parameters.AddWithValue("$Id", vanhaId.Value);
                updateExisting.ExecuteNonQuery();

                // Delete the edited row
                var deleteEdited = connection.CreateCommand();
                deleteEdited.CommandText = "DELETE FROM Tuotteet WHERE Id = $Id";
                deleteEdited.Parameters.AddWithValue("$Id", Id);
                deleteEdited.ExecuteNonQuery();

                return Results.Ok(new { message = "Muokattu tuote yhdistetty toiseen tuotteeseen." });
            }

            return Results.Ok(new { message = "Tuote muokattu onnistuneesti" });
        }
    }

    public List<Tuote> ListaaTuotteet()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Register Finnish collation (ä, ö, å sorted correctly)
            connection.CreateCollation(
                "FINNISH",
                (x, y) => string.Compare(
                    x, 
                    y, 
                    new CultureInfo("fi-FI"),
                    CompareOptions.IgnoreCase
                )
            );

            var selectTuotteetCmd = connection.CreateCommand();
            selectTuotteetCmd.CommandText = @"
                SELECT Id, Tag, Nimi, Maara, Kunto, VarastoId
                FROM Tuotteet
                WHERE VarastoId = $VarastoId
                ORDER BY Nimi COLLATE FINNISH;";
            selectTuotteetCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            using (var reader = selectTuotteetCmd.ExecuteReader())
            {
                List<Tuote> tuotteet = new List<Tuote>();

                while (reader.Read())
                {
                    tuotteet.Add(new Tuote(
                        reader.GetInt32(0),  // Id
                        reader.GetString(1), // Tag
                        reader.GetString(2), // Nimi
                        reader.GetInt32(3),  // Maara
                        reader.GetString(4), // Kunto
                        reader.GetInt32(5)   // VarastoId
                    ));
                }
                return tuotteet;
            }
        }
    }

    public List<Tuote> EtsiTuotteet(string column, string value)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            var results = new List<Tuote>();
            connection.Open();

            var searchItemCommand = connection.CreateCommand();
            searchItemCommand.CommandText = $@"
                SELECT Id, Nimi, Tag, Kunto, Maara, VarastoId
                FROM Tuotteet
                WHERE LOWER({column}) = LOWER($Value)
                AND VarastoId = $VarastoId;";
            searchItemCommand.Parameters.AddWithValue("$Value", value);
            searchItemCommand.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            using (var reader = searchItemCommand.ExecuteReader())
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
        if (string.IsNullOrWhiteSpace(nimi))
            throw new ArgumentException("Varaston nimi ei voi olla tyhjä.", nameof(nimi));

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Luodaan Varastot-taulu, jos sitä ei vielä ole
            using (var createVarasto = connection.CreateCommand())
            {
                createVarasto.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Varastot(
                        Id INTEGER PRIMARY KEY,
                        Nimi TEXT
                    );";
                createVarasto.ExecuteNonQuery();
            }

            // Luodaan Tuotteet-taulu, jos sitä ei vielä ole
            using (var createTuotteetTableCmd = connection.CreateCommand())
            {
                createTuotteetTableCmd.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tuotteet (
                        Id INTEGER PRIMARY KEY,
                        Tag TEXT,
                        Nimi TEXT,
                        Maara INTEGER,
                        Kunto TEXT,
                        VarastoId INTEGER,
                        FOREIGN KEY(VarastoId) REFERENCES Varastot(Id) ON DELETE CASCADE
                    );";
                createTuotteetTableCmd.ExecuteNonQuery();
            }

            // Lisätään uusi varasto Varastot-tauluun ja haetaan sen ID
            using (var addVarasto = connection.CreateCommand())
            {
                addVarasto.CommandText = "INSERT INTO Varastot (Nimi) VALUES (@Nimi); SELECT last_insert_rowid();";
                addVarasto.Parameters.AddWithValue("@Nimi", nimi);
                long uusiVarastoId = (long)addVarasto.ExecuteScalar();

                currentVarastoId = (int)uusiVarastoId; // asetetaan aktiiviseksi
                Console.WriteLine($"Uusi varasto '{nimi}' luotu ja valittu. ID = {currentVarastoId}");

                return currentVarastoId.Value;
            }
        }
    }

    public object? HaeAktiivinenVarasto()
    {
        if (currentVarastoId == null)
            return null;

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Id, Nimi FROM Varastot WHERE Id = @id";
                command.Parameters.AddWithValue("@id", currentVarastoId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new
                        {
                            Id = reader.GetInt32(0),
                            Nimi = reader.GetString(1)
                        };
                    }
                }
            }
        }

        return null;
    }


    public List<VarastoTiedot> HaeVarastot()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT Id, Nimi FROM Varastot";

            var varastot = new List<VarastoTiedot>();
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                varastot.Add(new VarastoTiedot(reader.GetInt32(0), reader.GetString(1)));
            }

            return varastot;
        }
    }

    public bool PoistaVarasto(int varastoId)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Tarkistetaan, että varasto on olemassa
            var checkCmd = connection.CreateCommand();
            checkCmd.CommandText = "SELECT COUNT(*) FROM Varastot WHERE Id = $Id";
            checkCmd.Parameters.AddWithValue("$Id", varastoId);
            int count = Convert.ToInt32(checkCmd.ExecuteScalar());

            if (count == 0)
            {
                Console.WriteLine("Varastoa ei löytynyt.");
                return false;
            }

            // Poistetaan varasto (Tuotteet poistuvat automaattisesti FOREIGN KEY CASCADE:n ansiosta)
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Varastot WHERE Id = $Id";
            deleteCmd.Parameters.AddWithValue("$Id", varastoId);
            int rowsAffected = deleteCmd.ExecuteNonQuery();

            if (rowsAffected > 0)
            {
                if (currentVarastoId == varastoId)
                    currentVarastoId = null; // nollataan aktiivinen varasto

                Console.WriteLine($"Varasto {varastoId} poistettu onnistuneesti.");
                return true;
            }
            else
            {
                Console.WriteLine("Varaston poisto epäonnistui.");
                return false;
            }
        }
    }

    //muokka nimeä
    public IResult MuokkaaVarastoNimi(int id, string uusiNimi)
    {
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();

        var cmd = connection.CreateCommand();
        cmd.CommandText = @"UPDATE Varastot SET Nimi = $Nimi WHERE Id = $Id";
        cmd.Parameters.AddWithValue("$Nimi", uusiNimi);
        cmd.Parameters.AddWithValue("$Id", id);

        int rows = cmd.ExecuteNonQuery();
        if (rows == 0)
            return Results.NotFound(new { message = "Varastoa ei löytynyt" });

        return Results.Ok(new { message = "Varaston nimi päivitetty" });
    }

    public bool PoistaTuote(int id)
    {
        using var yhteys = new SqliteConnection(_connectionString);
        yhteys.Open();

        var komento = yhteys.CreateCommand();
        komento.CommandText = "DELETE FROM Tuotteet WHERE id = $id";
        komento.Parameters.AddWithValue("$id", id);

        int rivit = komento.ExecuteNonQuery();
        return rivit > 0; // True jos poistettiin vähintään yksi rivi
    }

    // (Valinnaisesti: setter aktiiviselle varastolle)
    public void AsetaAktiivinenVarasto(int varastoId)
    {
        currentVarastoId = varastoId;
    }
}



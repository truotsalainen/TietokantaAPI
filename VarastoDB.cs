namespace TietokantaViikko;

using System.Data;
using Microsoft.Data.Sqlite;

public record Tuote(string tag, string nimi, int maara, string kunto);

public class VarastoDB
{
    private static string _connectionString = "Data Source=VarastoDB.db";
    private int? currentVarastoId = null; // Valittu varasto.

    public VarastoDB()
    {

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
    public void PoistaTuote(string nimi)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var deleteItemCmd = connection.CreateCommand();
            deleteItemCmd.CommandText = @"
                DELETE FROM Tuotteet
                WHERE Nimi = $Nimi AND VarastoId = $VarastoId;";
            deleteItemCmd.Parameters.AddWithValue("$Nimi", nimi);
            deleteItemCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);
            deleteItemCmd.ExecuteNonQuery();

            Console.WriteLine("Tuote poistettu varastosta.");
        }
    }


    public void MuokkaaTuote(string nimi)
    {
        if (currentVarastoId == null)
        {
            Console.WriteLine("Et ole valinnut varastoa.");
            return;
        }

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            //Find all products with the given name
            var fetchCmd = connection.CreateCommand();
            fetchCmd.CommandText = @"
                SELECT Id, Nimi, Maara, Tag, Kunto 
                FROM Tuotteet 
                WHERE Nimi = $Nimi AND VarastoId = $VarastoId";
            fetchCmd.Parameters.AddWithValue("$Nimi", nimi);
            fetchCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            var products = new List<(int Id, string Nimi, int Maara, string Tag, string Kunto)>();
            using (var reader = fetchCmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    products.Add((reader.GetInt32(0), reader.GetString(1), reader.GetInt32(2), reader.GetString(3), reader.GetString(4)));
                }
            }

            if (products.Count == 0)
            {
                Console.WriteLine("Tuotetta ei löytynyt.");
                return;
            }

            //Ask user which ID to edit if multiple
            Console.WriteLine("Löydetyt tuotteet:");
            foreach (var p in products)
                Console.WriteLine($"ID: {p.Id} | Nimi: {p.Nimi}, Määrä: {p.Maara}, Tag: {p.Tag}, Kunto: {p.Kunto}");

            Console.WriteLine("Anna ID valitaksesi tuotteen muokattavaksi:");
            if (!int.TryParse(Console.ReadLine(), out int id) || !products.Any(p => p.Id == id))
            {
                Console.WriteLine("Virheellinen ID");
                return;
            }

            var productToEdit = products.First(p => p.Id == id);

            // Editable fields
            string oldNimi = productToEdit.Nimi;
            int oldMaara = productToEdit.Maara;
            string oldTag = productToEdit.Tag;
            string oldKunto = productToEdit.Kunto;

            while (true)
            {
                Console.WriteLine($"\nMuokataan tuotetta (ID={id}):");
                Console.WriteLine($"[1] Nimi: {oldNimi}");
                Console.WriteLine($"[2] Määrä: {oldMaara}");
                Console.WriteLine($"[3] Tag: {oldTag}");
                Console.WriteLine($"[4] Kunto: {oldKunto}");
                Console.WriteLine("[5] Valmis (palaa)");

                Console.Write("\nValitse mitä haluat muuttaa: ");
                string choice = Console.ReadLine().Trim();

                if (choice == "5") break;

                string column = "";
                object newValue = null;

                if (choice == "1")
                {
                    Console.Write("Uusi nimi: ");
                    newValue = Console.ReadLine().Trim();
                    column = "Nimi";
                    oldNimi = (string)newValue;
                }
                else if (choice == "2")
                {
                    Console.Write("Uusi määrä: ");
                    if (int.TryParse(Console.ReadLine(), out int newMaara))
                    {
                        newValue = newMaara;
                        column = "Maara";
                        oldMaara = newMaara;
                    }
                    else
                    {
                        Console.WriteLine("Virheellinen numero!");
                        continue;
                    }
                }
                else if (choice == "3")
                {
                    Console.Write("Uusi tag: ");
                    newValue = Console.ReadLine().Trim().ToLower();
                    column = "Tag";
                    oldTag = (string)newValue;
                }
                else if (choice == "4")
                {
                    Console.Write("Uusi kunto: ");
                    newValue = Console.ReadLine().Trim().ToLower();
                    column = "Kunto";
                    oldKunto = (string)newValue;
                }
                else
                {
                    Console.WriteLine("Virheellinen valinta!");
                    continue;
                }

                // Update the chosen field
                var updateCmd = connection.CreateCommand();
                updateCmd.CommandText = $"UPDATE Tuotteet SET {column} = $Value WHERE Id = $Id";
                updateCmd.Parameters.AddWithValue("$Value", newValue);
                updateCmd.Parameters.AddWithValue("$Id", id);
                updateCmd.ExecuteNonQuery();
                Console.WriteLine($"{column} päivitetty onnistuneesti!");

                //Check if a duplicate now exists
                var mergeCmd = connection.CreateCommand();
                mergeCmd.CommandText = @"
                    SELECT Id, Maara FROM Tuotteet
                    WHERE Id != $Id AND Nimi = $Nimi AND Tag = $Tag AND Kunto = $Kunto AND VarastoId = $VarastoId";
                mergeCmd.Parameters.AddWithValue("$Id", id);
                mergeCmd.Parameters.AddWithValue("$Nimi", oldNimi);
                mergeCmd.Parameters.AddWithValue("$Tag", oldTag);
                mergeCmd.Parameters.AddWithValue("$Kunto", oldKunto);
                mergeCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

                using var mergeReader = mergeCmd.ExecuteReader();
                if (mergeReader.Read())
                {
                    int existingId = mergeReader.GetInt32(0);
                    int existingMaara = mergeReader.GetInt32(1);

                    int combinedMaara = existingMaara + oldMaara;

                    // Update existing row
                    var updateExisting = connection.CreateCommand();
                    updateExisting.CommandText = "UPDATE Tuotteet SET Maara = $Maara WHERE Id = $Id";
                    updateExisting.Parameters.AddWithValue("$Maara", combinedMaara);
                    updateExisting.Parameters.AddWithValue("$Id", existingId);
                    updateExisting.ExecuteNonQuery();

                    // Delete the edited row
                    var deleteEdited = connection.CreateCommand();
                    deleteEdited.CommandText = "DELETE FROM Tuotteet WHERE Id = $Id";
                    deleteEdited.Parameters.AddWithValue("$Id", id);
                    deleteEdited.ExecuteNonQuery();

                    Console.WriteLine("Tuotteet yhdistetty samaan riviin.");
                    break; // Stop editing because the row no longer exists
                }
            }
        }
    }

    public List<Tuote> ListaaTuotteet()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var selectTuotteetCmd = connection.CreateCommand();
            selectTuotteetCmd.CommandText = @"
                SELECT tag, nimi, maara, kunto
                FROM Tuotteet
                WHERE VarastoId = $VarastoId;";
            selectTuotteetCmd.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            using (var reader = selectTuotteetCmd.ExecuteReader())
            {
                List<Tuote> tuotteet = new List<Tuote>();

                while (reader.Read())
                {
                    tuotteet.Add(new Tuote(
                        reader.GetString(0),
                        reader.GetString(1),
                        reader.GetInt32(2),
                        reader.GetString(3)));
                }
                return tuotteet;
            }
        }
    }

    public List<string> EtsiTuotteet(string column, string value)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            var results = new List<string>();

            connection.Open();

            var searchItemCommand = connection.CreateCommand();
            searchItemCommand.CommandText = $@"
                SELECT Nimi, Tag, Kunto
                FROM Tuotteet
                WHERE {column} = $Value AND VarastoId = $VarastoId;";
            searchItemCommand.Parameters.AddWithValue("$Value", value);
            searchItemCommand.Parameters.AddWithValue("$VarastoId", currentVarastoId.Value);

            using (var reader = searchItemCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    string nimi = reader.GetString(0);
                    string tag = reader.GetString(1);
                    string kunto = reader.GetString(2);

                    results.Add($"{nimi}, {tag}, {kunto}");
                }
            }
            return results;
        }

    }
    public void LuoVarasto()
    {
        Console.WriteLine("Anna varaston nimi");
        string? nimi = Console.ReadLine();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Luodaan Varastot-taulu, jos sitä ei vielä ole
            var createVarasto = connection.CreateCommand();
            createVarasto.CommandText = @"
        CREATE TABLE IF NOT EXISTS Varastot(
            Id INTEGER PRIMARY KEY,
            Nimi TEXT
        )";
            createVarasto.ExecuteNonQuery();

            // Luodaan Tuotteet-taulu, jos sitä ei vielä ole
            var createTuotteetTableCmd = connection.CreateCommand();
            createTuotteetTableCmd.CommandText = @"
        CREATE TABLE IF NOT EXISTS Tuotteet (
            Id INTEGER PRIMARY KEY,
            Tag TEXT,
            Nimi TEXT,
            Maara INTEGER,
            Kunto TEXT,
            VarastoId INTEGER,
            FOREIGN KEY(VarastoId) REFERENCES Varastot(Id) ON DELETE CASCADE  
        )";
            createTuotteetTableCmd.ExecuteNonQuery();

            // Lisätään uusi varasto Varastot-tauluun ja haetaan sen ID
            var addVarasto = connection.CreateCommand();
            addVarasto.CommandText = "INSERT INTO Varastot (Nimi) VALUES (@Nimi); SELECT last_insert_rowid();";
            addVarasto.Parameters.AddWithValue("@Nimi", nimi);
            long uusiVarastoId = (long)addVarasto.ExecuteScalar();

            // Asetetaan uusi varasto automaattisesti valituksi
            currentVarastoId = (int)uusiVarastoId;
            Console.WriteLine($"Uusi varasto '{nimi}' luotu ja valittu. ID = {currentVarastoId}");
        }
    }

    public bool LataaVarasto()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            // Tarkistetaan, että Varastot-taulu on olemassa
            var tarkistaTaulu = connection.CreateCommand();
            tarkistaTaulu.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Varastot';";
            var result = tarkistaTaulu.ExecuteScalar();
            if (result == null)
            {
                Console.WriteLine("Varastoja ei ole olemassa. Luo ensin varasto.");
                return false;
            }

            // Haetaan kaikki varastot ja tulostetaan ne
            var kaikkiVarastot = connection.CreateCommand();
            kaikkiVarastot.CommandText = "SELECT Id, Nimi FROM Varastot";

            using (var reader = kaikkiVarastot.ExecuteReader())
            {
                Console.WriteLine("Varastot:");
                bool onkoVarastoja = false;
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nimi = reader.GetString(1);

                    Console.WriteLine($"{nimi} : {id}");
                    onkoVarastoja = true;
                }
                if (!onkoVarastoja)
                {
                    Console.WriteLine("Ei varastoja");
                    return false;
                }
            }

            // Pyydetään käyttäjää valitsemaan ladattava varasto ID:n perusteella
            Console.WriteLine("Anna ladattavan varaston ID");
            string? input = Console.ReadLine();

            // Tarkistetaan, että syöte on validi numero
            if (int.TryParse(input, out int valittuId))
            {
                // Tarkistetaan, että varasto ID:llä on olemassa
                var tarkistaId = connection.CreateCommand();
                tarkistaId.CommandText = "SELECT COUNT(*) FROM Varastot WHERE Id = @Id";
                tarkistaId.Parameters.AddWithValue("@Id", valittuId);
                int count = Convert.ToInt32(tarkistaId.ExecuteScalar());

                if (count > 0)
                {
                    // Asetetaan valittu varasto käyttöön
                    currentVarastoId = valittuId;
                    Console.WriteLine($"Varasto {valittuId} ladattu");
                    return true;
                }
                else
                {
                    Console.WriteLine("Varastoa ei löytynyt");
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Virheellinen ID");
                return false;
            }


        }

    }

    public void SuljeVarasto()
    {

    }

    public bool PoistaVarasto()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var kaikkiVarastot = connection.CreateCommand();
            kaikkiVarastot.CommandText = "SELECT Id, Nimi FROM Varastot";

            using (var reader = kaikkiVarastot.ExecuteReader())
            {
                Console.WriteLine("Varastot:");
                bool onkoVarastoja = false;
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string nimi = reader.GetString(1);

                    Console.WriteLine($"Varaston Nimi: {nimi} : Varaston ID: {id}");
                    onkoVarastoja = true;
                }
                if (!onkoVarastoja)
                {
                    Console.WriteLine("Ei varastoja");
                    return false;
                }
            }

            Console.WriteLine("Anna poistettavan varaston Nimi:");
            string? Nimi = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(Nimi))
            {
                Console.WriteLine("Virhe: Nimi ei voi olla tyhjä");
                return false;
            }

            Console.WriteLine("Anna poistettavan varaston ID:");
            string? input = Console.ReadLine();
            if (!int.TryParse(input, out int varastoId))
            {
                Console.WriteLine("Virhe: ID ei ole kelvollinen numero");
                return false;
            }

            var removeVarastoTable = connection.CreateCommand();
            removeVarastoTable.CommandText = @"
            DELETE FROM Varastot
            WHERE Id = $VarastoId AND Nimi = $Name
        ";

            removeVarastoTable.Parameters.AddWithValue("$VarastoId", varastoId);
            removeVarastoTable.Parameters.AddWithValue("$Name", Nimi);

            int rowsAffected = removeVarastoTable.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                Console.WriteLine("Varasto poistettu onnistuneesti");
                return true;
            }
            else
            {
                Console.WriteLine("Varastoa ei löytynyt tai tiedot eivät täsmänneet");
                return false;
            }
        }
    }
}
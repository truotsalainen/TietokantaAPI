TietokantaAPI - Dokumentaatio

Lyhyt kuvaus:
- Pieni ASP.NET Core Minimal API sovellus, joka tarjoaa varasto- ja tuotehallinnan.
- Tietokanta: SQLite (tiedosto `varasto.db` sovelluksen juurihakemistossa).

Pääendpointit:
- POST /register { username, password } - rekisteröi käyttäjän (password hashataan palvelussa)
- POST /login { username, password } - antaa JWT-tokenin onnistuneella kirjautumisella
- GET /varastot - listaa käyttäjän varastot (vaatii Bearer-token)
- POST /varastot { nimi } - luo uuden varaston
- DELETE /varastot/{id} - poistaa varaston (omistajuus tarkistetaan)
- GET /varastot/{varastoId}/tuotteet - listaa tuotteet
- POST /varastot/{varastoId}/tuotteet - lisää tai päivittää tuotteen määrää
- PUT /varastot/{varastoId}/tuotteet/{tuoteId} - muokkaa tuotetta
- DELETE /varastot/{varastoId}/tuotteet/{tuoteId} - poistaa tuotteen id:llä
- DELETE /varastot/{varastoId}/tuotteet?tuotteenNimi=nimi - poistaa tuotteen nimen perusteella

Tietoturva ja konfiguraatio:
- JWT-avaimen on oltava vähintään 32 tavua pitkä (sisäänkirjattu tarkistus käynnistyksessä).
- Salasanat hashataan BCrypt:llä ennen tietokantaan tallentamista.
- Varastojen ja tuotteiden muokkaus/poisto tarkistaa omistajuuden ennen toimintoja.

Käyttöönotto:
1. Aseta `appsettings.json` sisältämään turvallinen `Jwt:Key` (esim. 64-merkkinen satunnainen merkkijono).
2. Rakenna ja aja sovellus: `dotnet run` projektihakemistosta.
3. Käytä Swagger UI:ta kehitysympäristössä tai Postmania Bearer-tokenilla.

Tietokantarakenne:
- Users(Id, Username UNIQUE, PasswordHash)
- Varastot(Id, Nimi, UserId FK -> Users.Id ON DELETE CASCADE)
- Tuotteet(Id, Tag, Nimi, Maara, Kunto, VarastoId FK -> Varastot.Id ON DELETE CASCADE)

Lisätiedot:
- Katso `Program.cs`, `VarastoDB.cs`, `Services/AuthService.cs` ja `Services/JwtService.cs` kommentit lisäohjeiksi.

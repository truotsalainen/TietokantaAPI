using BCrypt.Net;

namespace TietokantaAPI.Services;

// AuthService vastaa salasanahashauksesta ja tarkistuksesta.
// Käytämme BCrypt-algoritmia, joka automaattisesti generoi suolan
// ja sisältää työmääräparametrin (cost). BCrypt on suunniteltu
// hidastamaan brute-force-hyökkäyksiä.
// Huomioita:
// - HashPassword: tuottaa turvallisen hashin, joka tulee tallentaa tietokantaan.
// - VerifyPassword: vertaa raakatekstiä tallennettuun hash-arvoon.
// - Työparametria (work factor) voi säätää suorituskyvyn ja turvallisuuden välillä.
// - Älä tallenna tai siirrä raakatekstisalanaa lokitiedostoissa.
public interface IAuthService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class AuthService : IAuthService
{
    // Luo BCrypt-hashin annetusta salasanasta.
    // Palautettu arvo sisältää myös suolan ja työparametrin, joten sitä
    // voidaan tallentaa sellaisenaan tietokantaan.
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    // Vertailee raakatekstisalanaa tallennettuun hash:iin.
    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

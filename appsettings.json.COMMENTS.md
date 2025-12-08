appsettings.json - kommentit

Jwt:
- Key: Salainen avain JWT:n allekirjoitukseen. Vähintään 32 tavua (256 bittiä) HS256:lle.
- Issuer: Tokenin antaja.
- Audience: Tokenin vastaanottaja (asiakas).
- ExpiresMinutes: Tokenin voimassaoloaika minuutteina.

Esimerkki:
{
  "Jwt": {
    "Key": "64merkkiä_tms_turvallinen_avain_jota_ei_jakamalla",
    "Issuer": "VarastoAPI",
    "Audience": "VarastoClient",
    "ExpiresMinutes": 120
  }
}

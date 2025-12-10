import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/varasto.dart';
import '../models/tuote.dart' as tuote_model;

// ========================================
// ApiService - API:n kanssa kommunikointiin
// ========================================
// Tämä palvelu hallitsee kaikki HTTP-pyynnöt back-endin kanssa.
// Huomio: JWT-token tulee tallentaa secure_storagen (tai muun turvallisuus-ratkaisun) avulla.
// Tällä hetkellä yksinkertaisesti muuttujassa.
class ApiService {

  static String get baseUrl {
    // Windows / Web: localhost:5154
    // Android emulator: 10.0.2.2:5154
    // Fyysinen laite verkossa: 192.168.x.x:5154
    return 'http://localhost:5154';
  }

  // JWT-token tallennetaan muistiin kirjautumisen jälkeen
  static String? _jwtToken;

  // Hae tallennettu token
  static String? getToken() => _jwtToken;

  // Aseta token (kutsutaan login-endpointin jälkeen)
  static void setToken(String token) {
    _jwtToken = token;
  }

  // Tyhjennä token (logout)
  static void clearToken() {
    _jwtToken = null;
  }

  // Apumetodi: Hae header Authorization-tokenilla
  static Map<String, String> _getHeaders({bool needsAuth = true}) {
    final headers = {"Content-Type": "application/json"};
    if (needsAuth && _jwtToken != null) {
      headers["Authorization"] = "Bearer $_jwtToken";
    }
    return headers;
  }

  // ========================================
  // AUTENTIKOINTI
  // ========================================

  /// Rekisteröi uuden käyttäjän
  static Future<bool> register(String username, String password) async {
    try {
      final res = await http.post(
        Uri.parse("$baseUrl/register"),
        headers: _getHeaders(needsAuth: false),
        body: jsonEncode({"username": username, "password": password}),
      );

      print("Register status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Register error: $e");
      return false;
    }
  }

  /// Kirjaudu sisään ja hae JWT-token
  static Future<bool> login(String username, String password) async {
    try {
      final res = await http.post(
        Uri.parse("$baseUrl/login"),
        headers: _getHeaders(needsAuth: false),
        body: jsonEncode({"username": username, "password": password}),
      );

      print("Login status: ${res.statusCode}");

      if (res.statusCode == 200) {
        final data = jsonDecode(res.body);
        final token = data["token"];
        setToken(token);
        return true;
      }
      return false;
    } catch (e) {
      print("Login error: $e");
      return false;
    }
  }

  // ========================================
  // VARASTOT
  // ========================================

  /// Hae käyttäjän kaikki varastot
  static Future<List<Varasto>> getWarehouses() async {
    try {
      final res = await http.get(
        Uri.parse("$baseUrl/varastot"),
        headers: _getHeaders(needsAuth: true),
      );

      print("Get warehouses status: ${res.statusCode}");

      if (res.statusCode != 200) {
        throw Exception("Failed to load warehouses: ${res.statusCode}");
      }

      final data = jsonDecode(res.body) as List<dynamic>;
      final list = data.map((e) => Varasto.fromJson(e)).toList();
      list.sort((a, b) => a.nimi.toLowerCase().compareTo(b.nimi.toLowerCase()));
      return list;
    } catch (e) {
      print("Get warehouses error: $e");
      rethrow;
    }
  }

  /// Luo uusi varasto
  static Future<bool> createWarehouse(String name) async {
    try {
      final res = await http.post(
        Uri.parse("$baseUrl/varastot"),
        headers: _getHeaders(needsAuth: true),
        body: jsonEncode({"nimi": name}),
      );

      print("Create warehouse status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Create warehouse error: $e");
      return false;
    }
  }

  /// Poista varasto
  static Future<bool> deleteWarehouse(int id) async {
    try {
      final res = await http.delete(
        Uri.parse("$baseUrl/varastot/$id"),
        headers: _getHeaders(needsAuth: true),
      );

      print("Delete warehouse status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Delete warehouse error: $e");
      return false;
    }
  }

  // ========================================
  // TUOTTEET
  // ========================================

  /// Hae tuotteet tietystä varastosta
  static Future<List<tuote_model.Tuote>> getItems(int varastoId) async {
    try {
      final res = await http.get(
        Uri.parse("$baseUrl/varastot/$varastoId/tuotteet"),
        headers: _getHeaders(needsAuth: true),
      );

      print("Get items status: ${res.statusCode}");

      if (res.statusCode != 200) {
        throw Exception("Failed to load items: ${res.statusCode}");
      }

      final data = jsonDecode(res.body) as List<dynamic>;
      return data.map((e) => tuote_model.Tuote.fromJson(e)).toList();
    } catch (e) {
      print("Get items error: $e");
      rethrow;
    }
  }

  /// Lisää tai päivitä tuotetta
  static Future<bool> createOrUpdateItem(int varastoId, tuote_model.Tuote tuote) async {
    try {
      final res = await http.post(
        Uri.parse("$baseUrl/varastot/$varastoId/tuotteet"),
        headers: _getHeaders(needsAuth: true),
        body: jsonEncode({
          "tag": tuote.tag,
          "nimi": tuote.nimi,
          "maara": tuote.maara,
          "kunto": tuote.kunto,
        }),
      );

      print("Create/update item status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Create/update item error: $e");
      return false;
    }
  }

  /// Poista tuote ID:n perusteella
  static Future<bool> deleteItem(int varastoId, int tuoteId) async {
    try {
      final res = await http.delete(
        Uri.parse("$baseUrl/varastot/$varastoId/tuotteet/$tuoteId"),
        headers: _getHeaders(needsAuth: true),
      );

      print("Delete item status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Delete item error: $e");
      return false;
    }
  }

  /// Poista tuote nimen perusteella
  static Future<bool> deleteItemByName(int varastoId, String nimi) async {
    try {
      final res = await http.delete(
        Uri.parse("$baseUrl/varastot/$varastoId/tuotteet?tuotteenNimi=$nimi"),
        headers: _getHeaders(needsAuth: true),
      );

      print("Delete item by name status: ${res.statusCode}");
      return res.statusCode == 200;
    } catch (e) {
      print("Delete item by name error: $e");
      return false;
    }
  }
  //search items
  static Future<List<tuote_model.Tuote>> searchItems({
  required String column,
  required String value,
  }) async {
    final url = Uri.parse('$baseUrl/etsituotteet?column=$column&value=$value');
    print("SEARCH URL: $url");

    final response = await http.get(url);

    print("SEARCH STATUS: ${response.statusCode}");
    print("SEARCH RAW RESPONSE: ${response.body}");

    if (response.statusCode != 200) {
      throw Exception("Server error ${response.statusCode}: ${response.body}");
    }

    final decoded = jsonDecode(response.body);

    if (decoded is! List) {
      throw Exception("Invalid response: expected a list, got: $decoded");
    }

    final List list = decoded;

    // Validate each item is a Map
    return list.where((item) => item is Map<String, dynamic>).map<tuote_model.Tuote>((item) {
      return tuote_model.Tuote.fromJson(item);
    }).toList();
  }



}
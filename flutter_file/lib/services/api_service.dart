import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/varasto.dart';
import '../models/tuote.dart' as tuote_model;

class ApiService {

  static String get baseUrl {
    //final host = html.window.location.hostname; // gets the current IP/host
    return 'http://10.83.16.38:5000';       ///   PUT YOUR OWN IP
  }

  static Future<List<Varasto>> getWarehouses() async {
    final res = await http.get(Uri.parse("$baseUrl/varastot"));

    // Debug: print the raw response
    print('HTTP status: ${res.statusCode}');

    if (res.statusCode != 200) {
      throw Exception('Failed to load warehouses: ${res.statusCode}');
    }
    final data = jsonDecode(res.body) as List<dynamic>;
    final list = data.map((e) => Varasto.fromJson(e)).toList();
    list.sort((a, b) => a.nimi.toLowerCase().compareTo(b.nimi.toLowerCase()));
    return list;
  }

  static Future<void> setActiveWarehouse(int id) async {
    await http.put(Uri.parse("$baseUrl/varasto/aktiivinen/$id"));
  }

  static Future<bool> createWarehouse(String name) async {
    final res = await http.post(
      Uri.parse("$baseUrl/varasto"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"nimi": name}),
    );

    print("Create warehouse status: ${res.statusCode}");

    // Return true if success (200)
    return res.statusCode == 200;
  }

  //lataa tuotteita varastoon
  static Future<List<tuote_model.Tuote>> getItems() async {
    final res = await http.get(Uri.parse("$baseUrl/tuote"));
    if (res.statusCode != 200) throw Exception('Failed to load items');

    final data = jsonDecode(res.body) as List<dynamic>;
    return data.map((e) => tuote_model.Tuote.fromJson(e)).toList();
  }

  // Poistaa varaston.
  Future<String> deleteCollection(int id) async {
    final response = await http.delete(Uri.parse("$baseUrl/varasto/$id"));

    if (response.statusCode == 200) {
      return response.body;
    } 
    
    else {
      throw Exception("Delete failed: ${response.statusCode}");
    }
  }

  //päivittää varaston nimeä
  static Future<bool> updateWarehouse(int id, String newName) async {
    final response = await http.put(
      Uri.parse("$baseUrl/varasto/$id"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"id": id, "nimi": newName}),
    );

    return response.statusCode == 200;
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
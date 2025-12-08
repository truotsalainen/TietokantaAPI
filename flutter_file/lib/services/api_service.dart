import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/varasto.dart';
import '../models/tuote.dart' as tuote_model;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
class ApiService {
  static String get baseUrl {
    //final host = html.window.location.hostname; // gets the current IP/host
    return 'http://192.168.x.xxx:5000';       ///   PUT YOUR OWN IP
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

  //päivittää varaston nimeä
  static Future<bool> updateWarehouse(int id, String newName) async {
    final response = await http.put(
      Uri.parse("$baseUrl/varasto/$id"),
      headers: {"Content-Type": "application/json"},
      body: jsonEncode({"id": id, "nimi": newName}),
    );

    return response.statusCode == 200;
  }

}
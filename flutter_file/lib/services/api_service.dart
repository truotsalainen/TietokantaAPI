import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/varasto.dart';
import '../models/tuote.dart' as tuote_model;
import 'dart:io' show Platform;
import 'package:flutter/foundation.dart' show kIsWeb;

class ApiService {
  // Tämä palauttaa backendin osoitteen automaattisesti
  static String get baseUrl {
    if (kIsWeb) {
      // Web-selaimessa localhost toimii
      return 'http://localhost:5000';
    } else if (Platform.isAndroid) {
      // Android-emulaattorissa localhost on 10.0.2.2
      return 'http://10.0.2.2:5000';
    } else {
      // Muu mobiili tai desktop: oletetaan localhost
      return 'http://localhost:5000';
    }
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

  // POistaa varaston.
  Future<String> deleteCollection(String name) async {
    final response = await http.delete(Uri.parse("$baseUrl/delete/$name"));

    if (response.statusCode == 200) {
      return response.body;
    } else {
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

}
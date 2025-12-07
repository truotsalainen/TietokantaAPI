import 'package:http/http.dart' as http;

class ApiService {
  final String baseUrl = "http://localhost:5000"; // Change to your API URL

  Future<String> getHello() async {
    final response = await http.get(Uri.parse("$baseUrl/hello"));

    if (response.statusCode == 200) {
      return response.body; // "Hello world"
    } else {
      throw Exception("Failed to load hello");
    }
  }

  Future<String> deleteCollection(String name) async {
    final response = await http.delete(Uri.parse("$baseUrl/delete/$name"));

    if (response.statusCode == 200) {
      return response.body;
    } else {
      throw Exception("Delete failed: ${response.statusCode}");
    }
  }
}

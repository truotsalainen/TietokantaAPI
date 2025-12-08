import 'package:flutter/material.dart';
import '../services/api_service.dart';
import '../models/tuote.dart';

class SearchPage extends StatefulWidget {
  const SearchPage({super.key});

  @override
  State<SearchPage> createState() => _SearchPageState();
}

class _SearchPageState extends State<SearchPage> {
  final TextEditingController searchController = TextEditingController();
  String selectedColumn = 'nimi';
  List<Tuote> results = [];
  bool loading = false;

  Future<void> runSearch() async {
    setState(() => loading = true);

    try {
      final response = await ApiService.searchItems(
        column: selectedColumn,
        value: searchController.text.trim(),
      );

      setState(() => results = response);
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Search failed: $e")),
      );
    }

    setState(() => loading = false);
  }

  @override
  Widget build(BuildContext context) {
    final color = Theme.of(context).colorScheme;

    return Scaffold(
      appBar: AppBar(title: const Text("Search Items")),
      body: Column(
        children: [
          // TOP SEARCH BAR SECTION
          Container(
            padding: const EdgeInsets.all(12),
            color: color.primaryContainer.withOpacity(0.3),
            child: Row(
              children: [
                DropdownButton<String>(
                  value: selectedColumn,
                  onChanged: (value) {
                    if (value != null) setState(() => selectedColumn = value);
                  },
                  items: const [
                    DropdownMenuItem(value: "nimi", child: Text("Name")),
                    DropdownMenuItem(value: "tag", child: Text("Tag")),
                    DropdownMenuItem(value: "kunto", child: Text("Condition")),
                  ],
                ),

                const SizedBox(width: 12),

                Expanded(
                  child: TextField(
                    controller: searchController,
                    decoration: const InputDecoration(
                      border: OutlineInputBorder(),
                      hintText: "Search...",
                    ),
                  ),
                ),

                const SizedBox(width: 12),

                ElevatedButton(
                  onPressed: runSearch,
                  child: const Text("Search"),
                ),
              ],
            ),
          ),

          // RESULTS SECTION
          Expanded(
            child: loading
                ? const Center(child: CircularProgressIndicator())
                : results.isEmpty
                    ? const Center(child: Text("No results"))
                    : ListView.builder(
                        itemCount: results.length,
                        itemBuilder: (context, index) {
                          final item = results[index];
                          return Card(
                            child: ListTile(
                              title: Text(item.nimi),
                              subtitle: Text(
                                  "Tag: ${item.tag} — Condition: ${item.kunto} • Qty: ${item.maara}"),
                            ),
                          );
                        },
                      ),
          ),
        ],
      ),
    );
  }

}

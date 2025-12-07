import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';
import '../models/varasto.dart';
import '../models/tuote.dart' as tuote_model;

class EditItemPage extends StatefulWidget {
  final Varasto collection;

  const EditItemPage({super.key, required this.collection});

  @override
  State<EditItemPage> createState() => _EditItemPageState();
}

class _EditItemPageState extends State<EditItemPage> {
  List<tuote_model.Tuote> items = [];
  bool isLoading = true;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  final String baseUrl = "http://192.168.x.xxx:5000"; // Your API IP

  @override
  void initState() {
    super.initState();
    fetchItems();
  }

  Future<void> fetchItems() async {
    setState(() => isLoading = true);

    try {
      final response = await http.get(Uri.parse("$baseUrl/varastot"));
      if (response.statusCode == 200) {
        final data = json.decode(response.body) as List<dynamic>;
        final varastoJson =
            data.firstWhere((v) => v['id'] == widget.collection.id);
        final collection = Varasto.fromJson(varastoJson);

        setState(() {
          items = collection.items;
        });
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Failed to fetch items: ${response.statusCode}")),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error fetching items: $e")),
      );
    }

    setState(() => isLoading = false);
  }

  Future<void> deleteItem(tuote_model.Tuote item) async {
    try {
      final response = await http.delete(Uri.parse("$baseUrl/tuote/${item.id}"));
      if (response.statusCode == 200) {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Item '${item.nimi}' deleted")),
        );
        fetchItems(); // Refetch after deletion
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Failed to delete item: ${response.statusCode}")),
        );
      }
    } catch (e) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error deleting item: $e")),
      );
    }
  }

  Future<void> editItem(tuote_model.Tuote oldItem) async {
    final nameController = TextEditingController(text: oldItem.nimi);
    final tagController = TextEditingController(text: oldItem.tag);
    final conditionController = TextEditingController(text: oldItem.kunto);
    final quantityController = TextEditingController(text: oldItem.maara.toString());

    await showDialog(
      context: context,
      builder: (context) {
        return AlertDialog(
          title: Text("Edit Item"),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(controller: nameController, decoration: InputDecoration(labelText: "Name")),
              TextField(controller: tagController, decoration: InputDecoration(labelText: "Tag")),
              TextField(controller: conditionController, decoration: InputDecoration(labelText: "Condition")),
              TextField(controller: quantityController, decoration: InputDecoration(labelText: "Quantity"), keyboardType: TextInputType.number),
            ],
          ),
          actions: [
            TextButton(onPressed: () => Navigator.pop(context), child: const Text("Cancel")),
            ElevatedButton(
              onPressed: () async {
                final updated = tuote_model.Tuote(
                  id: oldItem.id,
                  nimi: nameController.text,
                  tag: tagController.text,
                  kunto: conditionController.text,
                  maara: int.tryParse(quantityController.text) ?? oldItem.maara,
                );

                try {
                  final response = await http.put(
                    Uri.parse("$baseUrl/tuote/${oldItem.id}"),
                    headers: {"Content-Type": "application/json"},
                    body: json.encode(updated.toJson()),
                  );

                  if (response.statusCode == 200) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text("Item updated successfully")),
                    );
                    fetchItems(); // Refetch after edit
                    Navigator.pop(context);
                  } else {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text("Failed to update item: ${response.statusCode}")),
                    );
                  }
                } catch (e) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    SnackBar(content: Text("Error updating item: $e")),
                  );
                }
              },
              child: const Text("Save"),
            ),
          ],
        );
      },
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text("Edit Items in ${widget.collection.nimi}")),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: isLoading
            ? const Center(child: CircularProgressIndicator())
            : items.isEmpty
                ? const Center(child: Text("No items to edit"))
                : ListView.builder(
                    itemCount: items.length,
                    itemBuilder: (context, index) {
                      final item = items[index];
                      return ListTile(
                        title: Text(item.nimi),
                        subtitle: Text("Tag: ${item.tag} • Condition: ${item.kunto} • Qty: ${item.maara}"),
                        trailing: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            IconButton(icon: const Icon(Icons.edit), onPressed: () => editItem(item)),
                            IconButton(icon: const Icon(Icons.delete), onPressed: () => deleteItem(item)),
                          ],
                        ),
                      );
                    },
                  ),
      ),
    );
  }
}
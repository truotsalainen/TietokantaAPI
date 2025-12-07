import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

import '../models/varasto.dart';

class EditCollectionPage extends StatefulWidget {
  final Varasto collection;

  const EditCollectionPage({super.key, required this.collection});

  @override
  State<EditCollectionPage> createState() => _EditCollectionPageState();
}

class _EditCollectionPageState extends State<EditCollectionPage> {
  late TextEditingController nameController;
  bool isSaving = false;

  @override
  void initState() {
    super.initState();
    nameController = TextEditingController(text: widget.collection.nimi);
  }

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  Future<void> saveName() async {
    final String baseUrl = "http://192.168.x.xxx:5000";
    final String newName = nameController.text.trim();

    if (newName.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Name cannot be empty")),
      );
      return;
    }

    setState(() => isSaving = true);

    final response = await http.put(
      Uri.parse("$baseUrl/varasto/${widget.collection.id}"),
      headers: {"Content-Type": "application/json"},
      body: json.encode({"nimi": newName}),
    );

    setState(() => isSaving = false);

    if (response.statusCode == 200) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Name updated!")),
      );
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Failed: ${response.statusCode}")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Edit Collection")),
      body: Padding(
        padding: const EdgeInsets.all(30),
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              controller: nameController,
              decoration: const InputDecoration(
                labelText: "Rename your collection:",
                border: OutlineInputBorder(),
              ),
            ),
            const SizedBox(height: 30),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: () => Navigator.pop(context),
                  child: const Text("Return"),
                ),

                const SizedBox(width: 10),

                ElevatedButton(
                  onPressed: isSaving ? null : saveName,
                  child: isSaving
                      ? const CircularProgressIndicator(color: Colors.white)
                      : const Text("Save Changes"),
                ),

              decoration: InputDecoration(
                labelText: 'Rename your collection:',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(onPressed: () {}, child: Text('Return')),
                SizedBox(width: 10),
                ElevatedButton(onPressed: () {}, child: Text('Save Changes')),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
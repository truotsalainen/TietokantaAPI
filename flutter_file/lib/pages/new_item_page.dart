import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import 'dart:convert';

class NewItemPage extends StatefulWidget {
  const NewItemPage({super.key});

  @override
  State<NewItemPage> createState() => _NewItemPageState();
}

class _NewItemPageState extends State<NewItemPage> {
  final TextEditingController nameController = TextEditingController();
  final TextEditingController quantityController = TextEditingController();
  final TextEditingController typeController = TextEditingController();
  final TextEditingController conditionController = TextEditingController();

  bool isLoading = false;

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  Future<void> createItem() async {
    final String baseUrl = "http://10.83.16.38:5000/";

    final String name = nameController.text.trim();
    final String quantity = quantityController.text.trim();
    final String type = typeController.text.trim();
    final String condition = conditionController.text.trim();

    if (name.isEmpty || quantity.isEmpty || type.isEmpty || condition.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Fill all fields")),
      );
      return;
    }

    setState(() => isLoading = true);

    try {
      final response = await http.post(
        Uri.parse("$baseUrl/tuote"),
        headers: {"Content-Type": "application/json"},
        body: json.encode({
          "tag": type,
          "nimi": name,
          "maara": int.tryParse(quantity) ?? 0,
          "kunto": condition,
        }),
      );

      if (!mounted) return;

      if (response.statusCode == 200) {
        // Clear fields instead of returning
        nameController.clear();
        quantityController.clear();
        typeController.clear();
        conditionController.clear();

        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Item created successfully")),
        );
      } else {
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(content: Text("Failed: ${response.statusCode}")),
        );
      }
    } catch (e) {
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Error: $e")),
      );
    }

    if (mounted) setState(() => isLoading = false);
  }

  @override
  Widget build(BuildContext context) {
    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              controller: nameController,
              decoration: InputDecoration(
                labelText: 'Enter Name:',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            TextField(
              controller: quantityController,
              decoration: InputDecoration(
                labelText: 'Enter Quantity:',
                border: OutlineInputBorder(),
              ),
              keyboardType: TextInputType.number,
            ),
            SizedBox(height: 30),
            TextField(
              controller: typeController,
              decoration: InputDecoration(
                labelText: 'Enter Type (tag):',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            TextField(
              controller: conditionController,
              decoration: InputDecoration(
                labelText: 'Enter Condition:',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),

            /// Only the "Create Item" button
            ElevatedButton(
              onPressed: isLoading ? null : createItem,
              child: isLoading
                  ? CircularProgressIndicator(color: Colors.white)
                  : Text('Create Item'),
            ),
          ],
        ),
      ),
    );
  }
}
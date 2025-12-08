import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../models/varasto.dart';

class DeleteCollectionPage extends StatefulWidget {
  final Varasto collection;

  const DeleteCollectionPage({super.key, required this.collection});

  @override
  State<DeleteCollectionPage> createState() => _DeleteCollectionPageState();
}

class _DeleteCollectionPageState extends State<DeleteCollectionPage> {
  final TextEditingController confirmController = TextEditingController();
  bool isDeleting = false;

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
  Future<void> deleteCollection() async {
    final String baseUrl = "http://10.83.16.38:5000";

    if (confirmController.text.trim() != "DELETE") {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Type DELETE exactly to confirm")),
      );
      return;
    }

    setState(() => isDeleting = true);

    final response =
        await http.delete(Uri.parse("$baseUrl/varasto/${widget.collection.id}"));

    setState(() => isDeleting = false);

    if (response.statusCode == 200) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(content: Text("Collection deleted successfully")),
      );

      // DO NOT POP ANYTHING AUTOMATICALLY
      // User will manually press Return
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Failed: ${response.statusCode}")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Delete Collection")),
      body: Padding(
        padding: const EdgeInsets.all(30),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(
                'Are you sure you want to delete "${widget.collection.nimi}"?\n'
                'This will permanently remove everything inside it.\n\n'
                'To confirm deletion, type: DELETE',
                textAlign: TextAlign.center,
              ),
              const SizedBox(height: 30),

              TextField(
                controller: confirmController,
                decoration: const InputDecoration(
                  labelText: "Type 'DELETE' here",
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
                    onPressed: isDeleting ? null : deleteCollection,
                    child: isDeleting
                        ? const CircularProgressIndicator(color: Colors.white)
                        : const Text("Delete Collection"),
                  ),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}

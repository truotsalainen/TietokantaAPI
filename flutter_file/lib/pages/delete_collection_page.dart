import 'package:flutter/material.dart';
import '../services/api_service.dart';
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
      appBar: AppBar(
        title: Text('Delete "${widget.collection.nimi}"'),
      ), // Voit halutessasi lisätä otsikon
      body: Padding(
        padding: const EdgeInsets.all(30),
        child: Center(
          child: Column(
            mainAxisAlignment: MainAxisAlignment.center,
            children: [
              Text(
                'Are you sure you want to delete this collection?\n\n'
                'If you are sure, type “DELETE” below.',
              ),

              const SizedBox(height: 30),

              TextField(
                controller: _deleteController,
                decoration: const InputDecoration(
                  labelText: "Type 'DELETE' here",
                  border: OutlineInputBorder(),
                ),
              ),

              const SizedBox(height: 30),

              ElevatedButton(
                onPressed: () async {
                  if (_deleteController.text.trim() != "DELETE") {
                    print("You must type DELETE");
                    return;
                  }

                  try {
                    final api = ApiService();
                    final result = await api.deleteCollection(
                      widget.collection.id,
                    );
                    
                    print("Delete OK: $result");
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(
                        content: Text(
                          "Collection deleted successfully: $result",
                        ),
                      ),
                    );
                  } catch (e) {
                    print("Error deleting: $e");

                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text("Error deleting collection: $e")),
                    );
                  }
                },
                child: const Text('Delete collection'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

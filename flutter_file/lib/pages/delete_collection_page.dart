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
  final TextEditingController _deleteController = TextEditingController();

  @override
  Widget build(BuildContext context) {
    return Padding(
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
                  var result = await ApiService().deleteCollection("varastoDB");
                  print("Delete OK: $result");
                } catch (e) {
                  print("Error deleting: $e");
                }
              },
              child: const Text('Delete collection'),
            ),
          ],
        ),
      ),
    );
  }
}
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

class DeleteCollectionPage extends StatefulWidget {
  const DeleteCollectionPage({super.key});

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
                var appState = context.read<MyAppState>();

                if (_deleteController.text.trim() != "DELETE") {
                  print("You must type DELETE");
                  return;
                }

                try {
                  var result = await appState.api.deleteCollection("varastoDB");
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
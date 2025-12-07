import 'package:flutter/material.dart';
import '../services/api_service.dart';

class CreateCollectionPage extends StatefulWidget {
  const CreateCollectionPage({super.key});

  @override
  State<CreateCollectionPage> createState() => _CreateCollectionPageState();
}

class _CreateCollectionPageState extends State<CreateCollectionPage> {
  final nameController = TextEditingController();
  bool saving = false;

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text("Create Collection")),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            TextField(
              controller: nameController,
              decoration: const InputDecoration(
                labelText: "Collection Name",
              ),
            ),
            const SizedBox(height: 20),
            saving
                ? const CircularProgressIndicator()
                : ElevatedButton(
                    onPressed: () async {
                      setState(() => saving = true);

                      try {
                        await ApiService.createWarehouse(nameController.text.trim());

                        // Clear the input box
                        nameController.clear();

                        // Optional: show a quick confirmation
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text("Collection created successfully!")),
                        );
                      } catch (e) {
                        print("Error creating collection: $e");
                        ScaffoldMessenger.of(context).showSnackBar(
                          SnackBar(content: Text("Failed to create collection")),
                        );
                      } finally {
                        if (mounted) setState(() => saving = false);
                      }
                    },
                    child: const Text("Create"),
                  )
          ],
        ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import 'package:http/http.dart' as http;
import '../services/api_service.dart';
import '../models/varasto.dart';

class DeleteCollectionPage extends StatefulWidget {
  final Varasto collection;

  const DeleteCollectionPage({super.key, required this.collection});

  @override
  State<DeleteCollectionPage> createState() => _DeleteCollectionPageState();
}

class _DeleteCollectionPageState extends State<DeleteCollectionPage> {
  late TextEditingController _deleteController;
  bool isDeleting = false;

  @override
  void initState() {
    super.initState();
    _deleteController = TextEditingController();
  }

  @override
  void dispose() {
    _deleteController.dispose();
    super.dispose();
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
                    ScaffoldMessenger.of(context).showSnackBar(
                      const SnackBar(content: Text("Kirjoita DELETE vahvistukseksi")),
                    );
                    return;
                  }

                  setState(() => isDeleting = true);

                  try {
                    final response = await http.delete(
                      Uri.parse("${ApiService.baseUrl}/varastot/${widget.collection.id}"),
                      headers: {
                        "Authorization": "Bearer ${ApiService.getToken()}",
                      },
                    );

                    setState(() => isDeleting = false);

                    if (!mounted) return;

                    if (response.statusCode == 200) {
                      ScaffoldMessenger.of(context).showSnackBar(
                        const SnackBar(
                          content: Text("Varasto poistettu onnistuneesti"),
                          backgroundColor: Colors.green,
                        ),
                      );
                      Future.delayed(const Duration(seconds: 1), () {
                        if (mounted) Navigator.of(context).pop(true);
                      });
                    } else {
                      ScaffoldMessenger.of(context).showSnackBar(
                        SnackBar(content: Text("Virhe: ${response.statusCode}")),
                      );
                    }
                  } catch (e) {
                    setState(() => isDeleting = false);
                    if (!mounted) return;
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text("Virhe: $e")),
                    );
                  }
                },
                style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
                child: isDeleting
                    ? const SizedBox(
                        height: 20,
                        width: 20,
                        child: CircularProgressIndicator(strokeWidth: 2),
                      )
                    : const Text('Poista varasto'),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

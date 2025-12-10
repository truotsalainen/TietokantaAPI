import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../state/my_app_state.dart';
import '../services/api_service.dart';
import '../models/varasto.dart';
import 'create_collection_page.dart';
import 'edit_collection_page.dart';
import 'delete_collection_page.dart';

class CollectionsPage extends StatefulWidget {
  const CollectionsPage({super.key});

  @override
  State<CollectionsPage> createState() => _CollectionsPageState();
}

class _CollectionsPageState extends State<CollectionsPage> {
  List<Varasto> varastot = [];
  bool loading = true;
  Varasto? selectedCollection; // currently selected collection

  @override
  void initState() {
    super.initState();
    loadVarastot();
  }

  Future<void> loadVarastot() async {
    setState(() => loading = true);

    try {
      final list = await ApiService.getWarehouses();

      if (!mounted) return;

      list.sort((a, b) => a.nimi.toLowerCase().compareTo(b.nimi.toLowerCase()));

      setState(() {
        varastot = list;
        loading = false;
      });
    } catch (e) {
      if (!mounted) return;

      setState(() => loading = false);

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text("Failed to load warehouses: $e")),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Scaffold(
      appBar: AppBar(title: const Text("Collections")),
      body: Center(
        child: Column(
          children: [
            Expanded(
              child: loading
                  ? const Center(child: CircularProgressIndicator())
                  : varastot.isEmpty
                      ? const Center(child: Text("No warehouses found"))
                      : ListView.builder(
                          itemCount: varastot.length,
                          itemBuilder: (context, index) {
                            final v = varastot[index];

                            return ListTile(
                              title: Text(v.nimi),
                              selected: selectedCollection?.id == v.id,
                              onTap: () async {
                                setState(() => selectedCollection = v);

                                try {
                                  if (!mounted) return;
                                  appState.selectWarehouse(v.id, v.nimi);
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(content: Text("Selected: ${v.nimi}")),
                                  );
                                } catch (e) {
                                  if (!mounted) return;
                                  ScaffoldMessenger.of(context).showSnackBar(
                                    SnackBar(content: Text("Failed to select: $e")),
                                  );
                                }
                              },
                            );
                          },
                        ),
            ),
            const SizedBox(height: 20),
            Padding(
              padding: const EdgeInsets.symmetric(vertical: 20.0),
              child: Wrap(
                spacing: 10,
                runSpacing: 10,
                alignment: WrapAlignment.center,
                children: [
                  // Search button stays as before
                  ElevatedButton(
                    onPressed: () {
                      // TODO: implement search functionality
                    },
                    child: const Text("Search"),
                  ),

                  // New Collection always enabled
                  _buildNavButton(context, "New Collection", const CreateCollectionPage()),

                  // Edit Collection enabled only if selected
                  ElevatedButton(
                    onPressed: selectedCollection == null
                        ? null
                        : () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => EditCollectionPage(collection: selectedCollection!),
                              ),
                            );
                          },
                    child: const Text("Edit Collection"),
                  ),

                  // Delete Collection enabled only if selected
                  ElevatedButton(
                    onPressed: selectedCollection == null
                        ? null
                        : () {
                            Navigator.push(
                              context,
                              MaterialPageRoute(
                                builder: (_) => DeleteCollectionPage(collection: selectedCollection!),
                              ),
                            );
                          },
                    child: const Text("Delete Collection"),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildNavButton(BuildContext context, String title, Widget page) {
    return ElevatedButton(
      onPressed: () {
        Navigator.push(
          context,
          MaterialPageRoute(builder: (context) => page),
        );
      },
      child: Text(title),
    );
  }
}
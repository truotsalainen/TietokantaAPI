import 'package:flutter/material.dart';
import '../models/varasto.dart';

class CollectionViewPage extends StatelessWidget {
  final Varasto collection;

  const CollectionViewPage({super.key, required this.collection});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: Text(collection.nimi)),
      body: Padding(
        padding: const EdgeInsets.all(20),
        child: Column(
          children: [
            Expanded(
              child: collection.items.isEmpty
                  ? const Center(child: Text("No items in this collection"))
                  : ListView.builder(
                      itemCount: collection.items.length,
                      itemBuilder: (context, index) {
                        final item = collection.items[index];
                        return ListTile(
                          title: Text(item.nimi),
                          subtitle: Text("Tag: ${item.tag} • Condition: ${item.kunto}"),
                          trailing: Text("Qty: ${item.maara}"),
                        );
                      },
                    ),
            ),
            const SizedBox(height: 20),
            Wrap(
              spacing: 10,
              runSpacing: 10,
              children: [
                ElevatedButton(onPressed: () {}, child: const Text('Search')),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
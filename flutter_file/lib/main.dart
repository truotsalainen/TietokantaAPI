import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'state/my_app_state.dart';
import 'models/varasto.dart';
import 'models/tuote.dart' as tuote_model;
import 'services/api_service.dart';
import 'pages/collections_page.dart';
import 'pages/collection_view_page.dart';
import 'pages/create_collection_page.dart';
import 'pages/edit_collection_page.dart';
import 'pages/delete_collection_page.dart';
import 'pages/new_item_page.dart';
import 'pages/edit_item_page.dart';
import 'pages/login_page.dart';
import 'pages/create_user_page.dart';
import 'pages/forgot_password_page.dart';

void main() {
  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (_) => MyAppState(),
      child: MaterialApp(
        title: 'Hoardr',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepOrange),
        ),
        home: const MyHomePage(),
      ),
    );
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key});

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  int selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    final appState = context.watch<MyAppState>();

    late Widget page;

    switch (selectedIndex) {
      case 0:
        page = const LoginPage();
        break;
      case 1:
        page = const CreateUserPage();
        break;
      case 2:
        page = const ForgotPasswordPage();
        break;

      // Collection View
      case 3:
        page = Center(
          child: ElevatedButton(
            onPressed: () async {
              if (appState.selectedWarehouseId != null &&
                  appState.selectedWarehouseName != null) {

                // Fetch items asynchronously
                List<tuote_model.Tuote> items = [];
                try {
                  items = await ApiService.getItems();
                } catch (e) {
                  if (context.mounted) {
                    ScaffoldMessenger.of(context).showSnackBar(
                      SnackBar(content: Text("Failed to load items: $e")),
                    );
                  }
                  return;
                }

                if (!mounted) return;

                // Create the collection including items
                final selectedCollection = Varasto(
                  id: appState.selectedWarehouseId!,
                  nimi: appState.selectedWarehouseName!,
                  items: items,
                );

                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) => CollectionViewPage(
                      collection: selectedCollection, // only this is needed
                    ),
                  ),
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("No warehouse selected")),
                );
              }
            },
            child: const Text("Open Selected Collection"),
          ),
        );
        break;

      case 4:
        page = const CreateCollectionPage();
        break;

      case 5:
        page = const CollectionsPage();
        break;

      case 6:
        page = Center(
          child: ElevatedButton(
            onPressed: () {
              if (appState.selectedWarehouseId != null &&
                  appState.selectedWarehouseName != null) {
                final selectedCollection = Varasto(
                  id: appState.selectedWarehouseId!,
                  nimi: appState.selectedWarehouseName!,
                  items: [],
                );
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) =>
                        EditCollectionPage(collection: selectedCollection),
                  ),
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("No collection selected")),
                );
              }
            },
            child: const Text("Edit Selected Collection"),
          ),
        );
        break;

      case 7:
        page = Center(
          child: ElevatedButton(
            onPressed: () {
              if (appState.selectedWarehouseId != null &&
                  appState.selectedWarehouseName != null) {
                final selectedCollection = Varasto(
                  id: appState.selectedWarehouseId!,
                  nimi: appState.selectedWarehouseName!,
                  items: [],
                );
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) =>
                        DeleteCollectionPage(collection: selectedCollection),
                  ),
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("No collection selected")),
                );
              }
            },
            child: const Text("Delete Selected Collection"),
          ),
        );
        break;

      case 8:
        page = const NewItemPage();
        break;

      case 9:
        page = Center(
          child: ElevatedButton(
            onPressed: () {
              if (appState.selectedWarehouseId != null &&
                  appState.selectedWarehouseName != null) {
                final selectedCollection = Varasto(
                  id: appState.selectedWarehouseId!,
                  nimi: appState.selectedWarehouseName!,
                  items: [],
                );
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) =>
                        EditItemPage(collection: selectedCollection),
                  ),
                );
              } else {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(content: Text("No collection selected")),
                );
              }
            },
            child: const Text("Edit Selected Item"),
          ),
        );
        break;

      default:
        throw UnimplementedError('No widget for index $selectedIndex');
    }

    return LayoutBuilder(
      builder: (context, constraints) {
        return Scaffold(
          body: Row(
            children: [
              SafeArea(
                child: NavigationRail(
                  extended: constraints.maxWidth >= 600,
                  selectedIndex: selectedIndex,
                  onDestinationSelected: (value) =>
                      setState(() => selectedIndex = value),
                  destinations: const [
                    NavigationRailDestination(
                        icon: Icon(Icons.login), label: Text('Login')),
                    NavigationRailDestination(
                        icon: Icon(Icons.account_box),
                        label: Text('Create User')),
                    NavigationRailDestination(
                        icon: Icon(Icons.question_mark),
                        label: Text('Forgot Password')),
                    NavigationRailDestination(
                        icon: Icon(Icons.browse_gallery),
                        label: Text('Browse Collection')),
                    NavigationRailDestination(
                        icon: Icon(Icons.create_new_folder),
                        label: Text('New Collection')),
                    NavigationRailDestination(
                        icon: Icon(Icons.folder_open),
                        label: Text('Collection Menu')),
                    NavigationRailDestination(
                        icon: Icon(Icons.edit),
                        label: Text('Edit Collection')),
                    NavigationRailDestination(
                        icon: Icon(Icons.delete),
                        label: Text('Delete Collection')),
                    NavigationRailDestination(
                        icon: Icon(Icons.new_label),
                        label: Text('New Item')),
                    NavigationRailDestination(
                        icon: Icon(Icons.edit),
                        label: Text('Edit Item')),
                  ],
                ),
              ),
              Expanded(
                child: Container(
                  color: Theme.of(context).colorScheme.primaryContainer,
                  child: page,
                ),
              ),
            ],
          ),
        );
      },
    );
  }
}

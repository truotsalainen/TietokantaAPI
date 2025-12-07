import 'package:english_words/english_words.dart';
import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import 'api_service.dart';

void main() {
  runApp(MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return ChangeNotifierProvider(
      create: (context) => MyAppState(),
      child: MaterialApp(
        title: 'Hoardr',
        theme: ThemeData(
          colorScheme: ColorScheme.fromSeed(seedColor: Colors.deepOrange),
        ),
        home: MyHomePage(),
      ),
    );
  }
}

class MyAppState extends ChangeNotifier {
  var current = WordPair.random();

  void getNext() {
    current = WordPair.random();
    notifyListeners();
  }

  var favorites = <WordPair>[];

  void toggleFavorite() {
    if (favorites.contains(current)) {
      favorites.remove(current);
    } else {
      favorites.add(current);
    }
    notifyListeners();
  }

  final ApiService api = ApiService();

  Future<void> helloworld() async {
    try {
      String result = await api.getHello();
      print(result); // <-- prints "Hello world"

      // Optional: update your UI state based on API result
      // hello.add(result);

      notifyListeners();
    } catch (e) {
      print("Error: $e");
    }
  }

  Future<String> getHelloMessage() async {
    return await api.getHello();
  }
}

class MyHomePage extends StatefulWidget {
  const MyHomePage({super.key});

  @override
  State<MyHomePage> createState() => _MyHomePageState();
}

class _MyHomePageState extends State<MyHomePage> {
  var selectedIndex = 0;

  @override
  Widget build(BuildContext context) {
    Widget page;
    switch (selectedIndex) {
      case 0:
        page = LoginPage();
      case 1:
        page = CreateUserPage();
      case 2:
        page = ForgotPasswordPage();
      case 3:
        page = CollectionViewPage();
      case 4:
        page = CreateCollectionPage();
      case 5:
        page = CollectionsPage();
      case 6:
        page = EditCollectionPage();
      case 7:
        page = DeleteCollectionPage();
      case 8:
        page = NewItemPage();
      case 9:
        page = EditItemPage();
      default:
        throw UnimplementedError('no widget for $selectedIndex');
    }

    return LayoutBuilder(
      builder: (context, constraints) {
        return Scaffold(
          body: Row(
            children: [
              SafeArea(
                child: NavigationRail(
                  extended: constraints.maxWidth >= 600,
                  destinations: [
                    NavigationRailDestination(
                      icon: Icon(Icons.login),
                      label: Text('Login'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.account_box),
                      label: Text('Create User'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.question_mark),
                      label: Text('Forgot Password'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.browse_gallery),
                      label: Text('Browse Collection'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.create_new_folder),
                      label: Text('New Collection'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.folder_open),
                      label: Text('Collection menu'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.edit),
                      label: Text('Edit Collection'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.delete),
                      label: Text('Delete Collection'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.new_label),
                      label: Text('New Item'),
                    ),
                    NavigationRailDestination(
                      icon: Icon(Icons.edit),
                      label: Text('Edit Item'),
                    ),
                  ],
                  selectedIndex: selectedIndex,
                  onDestinationSelected: (value) {
                    setState(() {
                      selectedIndex = value;
                    });
                  },
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

class GeneratorPage extends StatelessWidget {
  const GeneratorPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();
    var pair = appState.current;

    IconData icon;
    if (appState.favorites.contains(pair)) {
      icon = Icons.favorite;
    } else {
      icon = Icons.favorite_border;
    }

    return Center(
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          BigCard(pair: pair),
          SizedBox(height: 10),
          Row(
            mainAxisSize: MainAxisSize.min,
            children: [
              ElevatedButton.icon(
                onPressed: () {
                  appState.toggleFavorite();
                },
                icon: Icon(icon),
                label: Text('Like'),
              ),
              SizedBox(width: 10),
              ElevatedButton(
                onPressed: () {
                  appState.getNext();
                },
                child: Text('Next'),
              ),
              //Hello world button
              SizedBox(width: 10),
              ElevatedButton(
                onPressed: () async {
                  appState.helloworld();
                  final message = await appState.getHelloMessage();
                  ScaffoldMessenger.of(context).showSnackBar(
                    //return message
                    SnackBar(content: Text(message)),
                  );
                },
                child: Text('Hello World'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class BigCard extends StatelessWidget {
  const BigCard({super.key, required this.pair});

  final WordPair pair;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context);
    final style = theme.textTheme.displayMedium!.copyWith(
      color: theme.colorScheme.onPrimary,
    );

    return Card(
      color: theme.colorScheme.primary,
      child: Padding(
        padding: const EdgeInsets.all(20),
        child: Text(
          pair.asLowerCase,
          style: style,
          semanticsLabel: "${pair.first} ${pair.second}",
        ),
      ),
    );
  }
}

class FavoritesPage extends StatelessWidget {
  const FavoritesPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    if (appState.favorites.isEmpty) {
      return Center(child: Text('No favorites yet.'));
    }

    return ListView(
      children: [
        Padding(
          padding: const EdgeInsets.all(20),
          child: Text(
            'You have '
            '${appState.favorites.length} favorites:',
          ),
        ),
        for (var pair in appState.favorites)
          ListTile(
            leading: Icon(Icons.favorite),
            title: Text(pair.asLowerCase),
          ),
      ],
    );
  }
}

class LoginPage extends StatelessWidget {
  const LoginPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // LOGO GOES HERE
            TextField(
              decoration: InputDecoration(
                labelText: 'Username',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Password',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            ElevatedButton(onPressed: Placeholder.new, child: Text("Login")),

            SizedBox(height: 30),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Forgot\nPassword'),
                ),
                SizedBox(width: 10),
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('New User'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class CreateUserPage extends StatelessWidget {
  const CreateUserPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // LOGO GOES HERE
            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Username',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter E-mail Address',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Create a Password',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Re-Enter Password',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),
                SizedBox(width: 10),
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Create Account'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class ForgotPasswordPage extends StatelessWidget {
  const ForgotPasswordPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text("Enter your username, and we will e-mail you your password."),
            SizedBox(height: 30),
            TextField(
              decoration: InputDecoration(
                labelText: 'Username',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),

                SizedBox(width: 10),

                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Request Password'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class CollectionViewPage extends StatelessWidget {
  const CollectionViewPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Center(
      child: Column(
        children: [
          Row(
            children: [
              Text('PLACEHOLDER COLLECTION NAME'),

              SizedBox(width: 10),

              ElevatedButton(onPressed: Placeholder.new, child: Text('Change')),
            ],
          ),

          SizedBox(height: 30),

          Expanded(
            child: Container(
              child: ListView(
                children: const [
                  ListTile(title: Text('Placeholder')),

                  ListTile(title: Text('Placeholder')),

                  ListTile(title: Text('Placeholder')),

                  ListTile(title: Text('Placeholder')),

                  ListTile(title: Text('Placeholder')),
                ],
              ),
            ),
          ),

          SizedBox(height: 30),

          Row(
            children: [
              ElevatedButton(onPressed: Placeholder.new, child: Text('Search')),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('Add Item'),
              ),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('Edit Item'),
              ),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('Delete Item'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class CreateCollectionPage extends StatelessWidget {
  const CreateCollectionPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              decoration: InputDecoration(
                labelText: 'Name your collection:',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),

                SizedBox(width: 10),

                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Create collection'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class CollectionsPage extends StatelessWidget {
  const CollectionsPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Center(
      child: Column(
        children: [
          Expanded(
            child: Container(
              child: ListView(
                children: const [
                  ListTile(title: Text('Placeholder Collection')),

                  ListTile(title: Text('Placeholder Collection')),

                  ListTile(title: Text('Placeholder Collection')),

                  ListTile(title: Text('Placeholder Collection')),

                  ListTile(title: Text('Placeholder Collection')),
                ],
              ),
            ),
          ),

          SizedBox(height: 30),

          Row(
            children: [
              ElevatedButton(onPressed: Placeholder.new, child: Text('Search')),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('New Collection'),
              ),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('Edit Collection'),
              ),

              SizedBox(width: 10),

              ElevatedButton(
                onPressed: Placeholder.new,
                child: Text('Delete Collection'),
              ),
            ],
          ),
        ],
      ),
    );
  }
}

class EditCollectionPage extends StatelessWidget {
  const EditCollectionPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              decoration: InputDecoration(
                labelText: 'Rename your collection:',
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),

                SizedBox(width: 10),

                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Save Changes'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class DeleteCollectionPage extends StatefulWidget {
  const DeleteCollectionPage({super.key});

  @override
  State<DeleteCollectionPage> createState() => _DeleteCollectionPageState();
}

class _DeleteCollectionPageState extends State<DeleteCollectionPage> {
  final TextEditingController _deleteController = TextEditingController();

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Text(
              'Are you sure you want to delete {collection name}? It will delete all contained data permanently.\n\nIf you are sure you want to delete {collection name}, type ”DELETE” in the box below.',
            ),

            SizedBox(height: 30),

            TextField(
              controller: _deleteController,
              decoration: InputDecoration(
                labelText: "Type 'DELETE' here",
                border: OutlineInputBorder(),
              ),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: () async {
                    var appState = context.read<MyAppState>();

                    if (_deleteController.text.trim() != "DELETE") {
                      print("You must type DELETE");
                      return;
                    }

                    try {
                      var result = await appState.api.deleteCollection(
                        "varastoDB",
                      ); // backendin nimi
                      print("Delete OK: $result");
                    } catch (e) {
                      print("Error deleting: $e");
                    }
                  },
                  child: Text('Delete collection'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class NewItemPage extends StatelessWidget {
  const NewItemPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Name:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Quantity:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Type:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Condition',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),
                SizedBox(width: 10),
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Create Item'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

class EditItemPage extends StatelessWidget {
  //need to link description text to the item being edited!
  const EditItemPage({super.key});

  @override
  Widget build(BuildContext context) {
    var appState = context.watch<MyAppState>();

    return Padding(
      padding: const EdgeInsets.all(30),
      child: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            // LOGO GOES HERE
            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Name:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Quantity:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Type:',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            TextField(
              decoration: InputDecoration(
                labelText: 'Enter Condition',
                border: OutlineInputBorder(),
              ),
            ),

            SizedBox(height: 30),

            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Return'),
                ),
                SizedBox(width: 10),
                ElevatedButton(
                  onPressed: Placeholder.new,
                  child: Text('Save Changes'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}

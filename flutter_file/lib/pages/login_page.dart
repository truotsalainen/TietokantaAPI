import 'package:flutter/material.dart';
import 'package:provider/provider.dart';
import '../state/my_app_state.dart';

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
            ElevatedButton(
              onPressed: () {
                // TODO: Login action
              },
              child: Text("Login"),
            ),
            SizedBox(height: 30),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: [
                ElevatedButton(
                  onPressed: () {}, 
                  child: Text('Forgot\nPassword'),
                ),
                SizedBox(width: 10),
                ElevatedButton(
                  onPressed: () {}, 
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

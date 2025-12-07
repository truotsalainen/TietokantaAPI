import 'package:flutter/material.dart';

class MyAppState extends ChangeNotifier {
  int? selectedWarehouseId;
  String? selectedWarehouseName;

  void selectWarehouse(int id, String name) {
    selectedWarehouseId = id;
    selectedWarehouseName = name;
    notifyListeners();
  }

  bool get hasSelectedWarehouse => selectedWarehouseId != null;
}

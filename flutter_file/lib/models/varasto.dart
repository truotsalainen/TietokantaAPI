import 'tuote.dart' as tuote_model;

class Varasto {
  final int id;
  final String nimi;
  final List<tuote_model.Tuote> items;

  Varasto({
    required this.id,
    required this.nimi,
    List<tuote_model.Tuote>? items,
  }) : items = items ?? [];

  factory Varasto.fromJson(Map<String, dynamic> json) {
    var itemsJson = json['items'] as List<dynamic>? ?? [];
    return Varasto(
      id: json['id'] as int,
      nimi: json['nimi'] as String,
      items: itemsJson.map((e) => tuote_model.Tuote.fromJson(e)).toList(),
    );
  }
}
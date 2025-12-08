class Tuote {
  final int id;
  final String tag;
  final String nimi;
  final int maara;
  final String kunto;

  Tuote({
    required this.id,
    required this.tag,
    required this.nimi,
    required this.maara,
    required this.kunto,
  });

  factory Tuote.fromJson(Map<String, dynamic> json) {
    return Tuote(
      id: json['id'] as int,          // <- include id from JSON
      tag: json['tag'] as String,
      nimi: json['nimi'] as String,
      maara: json['maara'] as int,
      kunto: json['kunto'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,                       // <- optional for PUT/POST
      'tag': tag,
      'nimi': nimi,
      'maara': maara,
      'kunto': kunto,
    };
  }
}

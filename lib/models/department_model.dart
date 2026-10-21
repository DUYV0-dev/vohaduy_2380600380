class DepartmentModel {
  final int id;
  final String code;
  final String name;
  final String description;
  final int employeeCount;

  DepartmentModel({
    required this.id,
    required this.code,
    required this.name,
    required this.description,
    this.employeeCount = 0,
  });

  factory DepartmentModel.fromJson(Map<String, dynamic> json) {
    return DepartmentModel(
      id: json['id'] is int ? json['id'] : int.parse(json['id'].toString()),
      code: json['code'] ?? json['Code'] ?? 'PB0${json['id']}',
      name: json['name'] ?? json['Name'] ?? '',
      description: json['description'] ?? json['Description'] ?? '',
      employeeCount: json['employeeCount'] ?? json['EmployeeCount'] ?? 0,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'code': code,
      'name': name,
      'description': description,
      'employeeCount': employeeCount,
    };
  }
}

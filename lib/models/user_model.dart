class UserModel {
  final String id;
  final String username;
  final String email;
  final String fullName;
  final String employeeCode;
  final String role; // Chỉ có 2 vai trò chính: 'Admin' và 'Employee' (Nhân viên)
  final int? departmentId;
  final String? departmentName;
  final String? phone;
  final String? avatarUrl;
  final bool isFaceRegistered;

  UserModel({
    required this.id,
    required this.username,
    required this.email,
    required this.fullName,
    required this.employeeCode,
    required this.role,
    this.departmentId,
    this.departmentName,
    this.phone,
    this.avatarUrl,
    this.isFaceRegistered = false,
  });

  factory UserModel.fromJson(Map<String, dynamic> json) {
    return UserModel(
      id: (json['id'] ?? json['Id'] ?? json['userId'] ?? '').toString(),
      username: json['username'] ?? json['Username'] ?? '',
      email: json['email'] ?? json['Email'] ?? '',
      fullName: json['fullName'] ?? json['FullName'] ?? json['name'] ?? json['Name'] ?? '',
      employeeCode: json['employeeCode'] ?? json['EmployeeCode'] ?? 'NV${json['id'] ?? '001'}',
      role: json['role'] ?? json['Role'] ?? 'Employee',
      departmentId: json['departmentId'] ?? json['DepartmentId'],
      departmentName: json['departmentName'] ?? json['DepartmentName'],
      phone: json['phone'] ?? json['Phone'] ?? '',
      avatarUrl: json['avatarUrl'] ?? json['AvatarUrl'] ?? '',
      isFaceRegistered: json['isFaceRegistered'] ?? json['IsFaceRegistered'] ?? false,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'username': username,
      'email': email,
      'fullName': fullName,
      'employeeCode': employeeCode,
      'role': role,
      'departmentId': departmentId,
      'departmentName': departmentName,
      'phone': phone,
      'avatarUrl': avatarUrl,
      'isFaceRegistered': isFaceRegistered,
    };
  }

  UserModel copyWith({
    String? fullName,
    String? email,
    String? role,
    int? departmentId,
    String? departmentName,
    String? phone,
    String? avatarUrl,
    bool? isFaceRegistered,
  }) {
    return UserModel(
      id: id,
      username: username,
      email: email ?? this.email,
      fullName: fullName ?? this.fullName,
      employeeCode: employeeCode,
      role: role ?? this.role,
      departmentId: departmentId ?? this.departmentId,
      departmentName: departmentName ?? this.departmentName,
      phone: phone ?? this.phone,
      avatarUrl: avatarUrl ?? this.avatarUrl,
      isFaceRegistered: isFaceRegistered ?? this.isFaceRegistered,
    );
  }
}

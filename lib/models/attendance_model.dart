class AttendanceModel {
  final String id;
  final String employeeId;
  final String employeeName;
  final String employeeCode;
  final DateTime checkInTime;
  final DateTime? checkOutTime;
  final String type; // 'Khuôn mặt', 'Vị trí GPS'
  final String locationName;
  final String status; // 'Đúng giờ', 'Đi trễ', 'Về sớm'
  final String note;

  AttendanceModel({
    required this.id,
    required this.employeeId,
    required this.employeeName,
    required this.employeeCode,
    required this.checkInTime,
    this.checkOutTime,
    required this.type,
    required this.locationName,
    required this.status,
    this.note = '',
  });

  factory AttendanceModel.fromJson(Map<String, dynamic> json) {
    String method = json['checkInMethod'] ?? json['type'] ?? json['Type'] ?? 'Khuôn mặt';
    if (method == 'FaceBiometric' || method == 'Face') method = 'Khuôn mặt';
    if (method == 'LocationGeofence' || method == 'GPS') method = 'Vị trí GPS';

    String rawStatus = json['status'] ?? json['Status'] ?? 'Đúng giờ';
    if (rawStatus == 'OnTime') rawStatus = 'Đúng giờ';
    if (rawStatus == 'Late') rawStatus = 'Đi trễ';

    return AttendanceModel(
      id: (json['id'] ?? json['Id'] ?? '').toString(),
      employeeId: (json['employeeId'] ?? json['EmployeeId'] ?? '').toString(),
      employeeName: json['employeeName'] ?? json['EmployeeName'] ?? (json['employee'] != null ? json['employee']['fullName'] : 'Nhân viên'),
      employeeCode: json['employeeCode'] ?? json['EmployeeCode'] ?? (json['employee'] != null ? json['employee']['employeeCode'] : 'NV001'),
      checkInTime: json['checkInTime'] != null
          ? DateTime.parse(json['checkInTime'].toString())
          : (json['date'] != null ? DateTime.parse(json['date'].toString()) : DateTime.now()),
      checkOutTime: json['checkOutTime'] != null
          ? DateTime.parse(json['checkOutTime'].toString())
          : null,
      type: method,
      locationName: json['locationName'] ?? json['LocationName'] ?? 'Tòa nhà Võ Hà Duy Tower',
      status: rawStatus,
      note: json['notes'] ?? json['note'] ?? json['Note'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'employeeId': employeeId,
      'employeeName': employeeName,
      'employeeCode': employeeCode,
      'checkInTime': checkInTime.toIso8601String(),
      'checkOutTime': checkOutTime?.toIso8601String(),
      'type': type,
      'locationName': locationName,
      'status': status,
      'note': note,
    };
  }
}

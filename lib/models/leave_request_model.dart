class LeaveRequestModel {
  final String id;
  final String employeeId;
  final String employeeName;
  final String employeeCode;
  final String reason;
  final DateTime startDate;
  final DateTime endDate;
  final String leaveType; // 'Nghỉ phép năm', 'Nghỉ bệnh', 'Nghỉ việc riêng'
  final String status; // 'Chờ duyệt', 'Đã duyệt', 'Từ chối'
  final String? adminComment;
  final DateTime createdAt;

  LeaveRequestModel({
    required this.id,
    required this.employeeId,
    required this.employeeName,
    required this.employeeCode,
    required this.reason,
    required this.startDate,
    required this.endDate,
    required this.leaveType,
    required this.status,
    this.adminComment,
    required this.createdAt,
  });

  factory LeaveRequestModel.fromJson(Map<String, dynamic> json) {
    return LeaveRequestModel(
      id: (json['id'] ?? json['Id'] ?? '').toString(),
      employeeId: (json['employeeId'] ?? json['EmployeeId'] ?? '').toString(),
      employeeName: json['employeeName'] ?? json['EmployeeName'] ?? '',
      employeeCode: json['employeeCode'] ?? json['EmployeeCode'] ?? '',
      reason: json['reason'] ?? json['Reason'] ?? '',
      startDate: DateTime.parse(json['startDate'] ?? DateTime.now().toIso8601String()),
      endDate: DateTime.parse(json['endDate'] ?? DateTime.now().toIso8601String()),
      leaveType: json['leaveType'] ?? json['LeaveType'] ?? 'Nghỉ phép năm',
      status: json['status'] ?? json['Status'] ?? 'Chờ duyệt',
      adminComment: json['adminComment'] ?? json['AdminComment'],
      createdAt: DateTime.parse(json['createdAt'] ?? DateTime.now().toIso8601String()),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'employeeId': employeeId,
      'employeeName': employeeName,
      'employeeCode': employeeCode,
      'reason': reason,
      'startDate': startDate.toIso8601String(),
      'endDate': endDate.toIso8601String(),
      'leaveType': leaveType,
      'status': status,
      'adminComment': adminComment,
      'createdAt': createdAt.toIso8601String(),
    };
  }

  LeaveRequestModel copyWith({
    String? status,
    String? adminComment,
  }) {
    return LeaveRequestModel(
      id: id,
      employeeId: employeeId,
      employeeName: employeeName,
      employeeCode: employeeCode,
      reason: reason,
      startDate: startDate,
      endDate: endDate,
      leaveType: leaveType,
      status: status ?? this.status,
      adminComment: adminComment ?? this.adminComment,
      createdAt: createdAt,
    );
  }
}

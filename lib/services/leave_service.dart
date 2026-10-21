import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';
import '../models/leave_request_model.dart';

class LeaveService {
  String get apiUrl => "${Config_URL.baseUrl}LeaveRequests";

  // Danh sách cục bộ tạm thời để hiển thị liền mạch trên UI nếu API chậm/lỗi mạng
  static final List<LeaveRequestModel> _localLeaveRequests = [];

  // Get Leave Requests (Filterable)
  Future<List<LeaveRequestModel>> getLeaveRequests({String? employeeId, String? status}) async {
    List<LeaveRequestModel> apiList = [];
    
    // Tải dữ liệu từ SharedPreferences (Cache Offline)
    await _loadLocalRequestsFromPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.get(
        Uri.parse(apiUrl),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 8));

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(utf8.decode(response.bodyBytes));
        apiList = data.map((e) => LeaveRequestModel.fromJson(e)).toList();
      }
    } catch (_) {}

    // KẾT HỢP LỊCH SỬ TỪ API VÀ CÁC ĐƠN VỪA TẠO CỤC BỘ
    List<LeaveRequestModel> combined = List.from(_localLeaveRequests);
    for (var item in apiList) {
      if (!combined.any((l) => l.id == item.id)) {
        combined.add(item);
      } else {
        // Cập nhật trạng thái mới nhất từ server nếu có thay đổi (Admin vừa duyệt)
        int idx = combined.indexWhere((l) => l.id == item.id);
        combined[idx] = item;
      }
    }

    if (employeeId != null && employeeId.isNotEmpty) {
      combined = combined.where((l) => l.employeeId == employeeId || (l.employeeId == '0') || l.employeeName.contains(employeeId)).toList();
    }
    if (status != null && status.isNotEmpty) {
      combined = combined.where((l) => l.status == status).toList();
    }
    
    combined.sort((a, b) => b.createdAt.compareTo(a.createdAt));
    return combined;
  }

  // Create Leave Request (Employee)
  Future<bool> createLeaveRequest(LeaveRequestModel request) async {
    // 1. Lưu ngay vào bộ nhớ cục bộ để hiển thị lên App tức thì
    _localLeaveRequests.insert(0, request);
    await _saveLocalRequestsToPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.post(
        Uri.parse(apiUrl),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode(request.toJson()),
      ).timeout(const Duration(seconds: 8));

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      }
    } catch (_) {}

    return true; // Trả về true vì đã lưu cục bộ thành công
  }

  // Update Status / Approve / Reject (Admin)
  Future<bool> updateLeaveStatus(String leaveId, String newStatus, {String? adminComment}) async {
    // Cập nhật trạng thái ngay lập tức trên App
    int idx = _localLeaveRequests.indexWhere((l) => l.id == leaveId);
    if (idx != -1) {
      _localLeaveRequests[idx] = _localLeaveRequests[idx].copyWith(
        status: newStatus,
        adminComment: adminComment ?? (newStatus == 'Đã duyệt' ? 'Đã phê duyệt' : 'Từ chối đơn'),
      );
      await _saveLocalRequestsToPrefs();
    }

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.put(
        Uri.parse("$apiUrl/$leaveId/status"),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode({
          "status": newStatus,
          "adminComment": adminComment ?? (newStatus == 'Đã duyệt' ? 'Đã phê duyệt' : 'Từ chối đơn'),
        }),
      ).timeout(const Duration(seconds: 8));

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      }
    } catch (_) {}

    return true;
  }

  // Delete Leave Request
  Future<bool> deleteLeaveRequest(String leaveId) async {
    _localLeaveRequests.removeWhere((l) => l.id == leaveId);
    await _saveLocalRequestsToPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      await http.delete(
        Uri.parse("$apiUrl/$leaveId"),
        headers: {
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 8));
    } catch (_) {}

    return true;
  }

  static Future<void> _saveLocalRequestsToPrefs() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    List<String> rawList = _localLeaveRequests.map((a) => jsonEncode(a.toJson())).toList();
    await prefs.setStringList('local_leave_requests_vohaduy', rawList);
  }

  static Future<void> _loadLocalRequestsFromPrefs() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    List<String>? rawList = prefs.getStringList('local_leave_requests_vohaduy');
    if (rawList != null && rawList.isNotEmpty) {
      _localLeaveRequests.clear();
      for (var str in rawList) {
        try {
          var json = jsonDecode(str);
          _localLeaveRequests.add(LeaveRequestModel.fromJson(json));
        } catch (_) {}
      }
    }
  }
}

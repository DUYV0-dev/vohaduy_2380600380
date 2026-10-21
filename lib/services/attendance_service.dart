import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';
import '../models/attendance_model.dart';

class AttendanceService {
  String get apiUrl => "${Config_URL.baseUrl}Attendance";

  // DANH SÁCH BỘ NHỚ LƯU TẤT CẢ CÁC LẦN CHẤM CÔNG
  static final List<AttendanceModel> _localSessionAttendances = [];

  // GIỜ CA LÀM VIỆC QUY ĐỊNH: 08:00 - 17:00 (Cho phép check-in từ 07:30 đến 17:30)
  static bool isWithinShiftHours(DateTime time) {
    int minutesOfDay = time.hour * 60 + time.minute;
    int shiftStartMinutes = 7 * 60 + 30; // 07:30 sáng
    int shiftEndMinutes = 17 * 60 + 30;  // 17:30 chiều
    return minutesOfDay >= shiftStartMinutes && minutesOfDay <= shiftEndMinutes;
  }

  // Lấy lịch sử chấm công (cho nhân viên hoặc admin)
  Future<List<AttendanceModel>> getAttendances({String? employeeId, DateTime? date}) async {
    List<AttendanceModel> apiList = [];

    // Lấy danh sách đã lưu cục bộ từ SharedPreferences
    await _loadLocalAttendancesFromPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      String endpoint = employeeId != null && employeeId.isNotEmpty
          ? "$apiUrl/my-history"
          : "$apiUrl/all";

      final response = await http.get(
        Uri.parse(endpoint),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200) {
        final decoded = jsonDecode(utf8.decode(response.bodyBytes));
        List<dynamic> items = [];
        if (decoded is List) {
          items = decoded;
        } else if (decoded is Map && decoded['data'] is List) {
          items = decoded['data'];
        } else if (decoded is Map && decoded['records'] is List) {
          items = decoded['records'];
        }

        apiList = items.map((e) => AttendanceModel.fromJson(e)).toList();
      }
    } catch (_) {}

    // KẾT HỢP LỊCH SỬ CHẤM CÔNG CỤC BỘ + WEB API SERVER
    List<AttendanceModel> combined = List.from(_localSessionAttendances);
    for (var item in apiList) {
      if (!combined.any((a) => a.id == item.id)) {
        combined.add(item);
      }
    }

    combined.sort((a, b) => b.checkInTime.compareTo(a.checkInTime));
    return combined;
  }

  // Thực hiện Check-in / Check-out mới
  Future<AttendanceModel> checkIn({
    required String employeeId,
    required String employeeName,
    required String employeeCode,
    required String type,
    required String locationName,
    String note = '',
  }) async {
    DateTime now = DateTime.now();
    String status = 'Đúng giờ';
    if (now.hour > 8 || (now.hour == 8 && now.minute > 30)) {
      int lateMins = (now.hour - 8) * 60 + (now.minute - 30);
      status = 'Đi trễ $lateMins phút';
    }

    AttendanceModel newRecord = AttendanceModel(
      id: 'att_${DateTime.now().millisecondsSinceEpoch}',
      employeeId: employeeId,
      employeeName: employeeName,
      employeeCode: employeeCode,
      checkInTime: now,
      type: type,
      locationName: locationName,
      status: status,
      note: note,
    );

    // GHI NHẬN TỨC THÌ VÀO CỤC BỘ VÀ SHAREDPREFERENCES
    _localSessionAttendances.insert(0, newRecord);
    await _saveLocalAttendancesToPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      String endpoint = type == 'Khuôn mặt'
          ? "$apiUrl/check-in-face"
          : "$apiUrl/check-in-location";

      await http.post(
        Uri.parse(endpoint),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode({
          "employeeId": employeeId,
          "employeeName": employeeName,
          "employeeCode": employeeCode,
          "latitude": 10.8443,
          "longitude": 106.7828,
          "type": type,
          "locationName": locationName,
          "status": status,
          "note": note,
        }),
      ).timeout(const Duration(seconds: 6));
    } catch (_) {}

    return newRecord;
  }

  static Future<void> _saveLocalAttendancesToPrefs() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    List<String> rawList = _localSessionAttendances.map((a) => jsonEncode(a.toJson())).toList();
    await prefs.setStringList('local_attendances_vohaduy', rawList);
  }

  static Future<void> _loadLocalAttendancesFromPrefs() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    List<String>? rawList = prefs.getStringList('local_attendances_vohaduy');
    if (rawList != null && rawList.isNotEmpty) {
      _localSessionAttendances.clear();
      for (var str in rawList) {
        try {
          var json = jsonDecode(str);
          _localSessionAttendances.add(AttendanceModel.fromJson(json));
        } catch (_) {}
      }
    }
  }

  // Check-out lượt hiện tại
  Future<bool> checkOut(String attendanceId) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.post(
        Uri.parse("$apiUrl/check-out"),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode({"attendanceId": attendanceId}),
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200) {
        return true;
      }
    } catch (_) {}

    return true;
  }

  // Delete Attendance Record
  Future<bool> deleteAttendance(String id) async {
    _localSessionAttendances.removeWhere((a) => a.id == id);
    await _saveLocalAttendancesToPrefs();

    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      await http.delete(
        Uri.parse("$apiUrl/$id"),
        headers: {
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 6));
    } catch (_) {}

    return true;
  }
}

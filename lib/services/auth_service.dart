import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';
import '../models/user_model.dart';

class AuthService {
  String get baseUrl => Config_URL.baseUrl;

  // 1. Đăng nhập qua Web API Backend (Hỗ trợ Mật khẩu & Mã OTP Email)
  Future<Map<String, dynamic>> login(String usernameOrEmail, String password, {String? otpCode}) async {
    // Nếu có mã OTP -> Gọi endpoint xác thực OTP: Verify-OTP-Login
    if (otpCode != null && otpCode.trim().isNotEmpty) {
      Uri verifyOtpUri = Uri.parse("${baseUrl}Authenticate/verify-otp-login");
      try {
        final response = await http.post(
          verifyOtpUri,
          headers: {"Content-Type": "application/json"},
          body: jsonEncode({
            "emailOrUsername": usernameOrEmail,
            "otpCode": otpCode.trim(),
          }),
        ).timeout(const Duration(seconds: 30));

        if (response.statusCode == 200) {
          final data = jsonDecode(response.body);
          return await _processAuthResponse(data, usernameOrEmail);
        } else {
          String msg = "Mã OTP xác thực không đúng hoặc đã hết hạn (${response.statusCode})";
          try {
            var err = jsonDecode(response.body);
            if (err['message'] != null) msg = err['message'];
          } catch (_) {}
          return {"success": false, "message": msg};
        }
      } catch (e) {
        return {
          "success": false,
          "message": "Không thể kết nối Web API xác thực OTP tại ($verifyOtpUri): $e"
        };
      }
    }

    // Nếu không nhập OTP -> Đăng nhập nhanh qua API Quick-Login / Login
    Uri loginUri = Uri.parse("${baseUrl}Authenticate/quick-login");
    try {
      final response = await http.post(
        loginUri,
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({
          "username": usernameOrEmail,
          "password": password,
        }),
      ).timeout(const Duration(seconds: 30));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return await _processAuthResponse(data, usernameOrEmail);
      } else {
        // Thử đường dẫn Login chuẩn nếu Quick-Login chưa bật
        return await _fallbackStandardLogin(usernameOrEmail, password);
      }
    } catch (e) {
      return {
        "success": false,
        "message": "Không thể kết nối tới Web API SQL Server tại ($loginUri): $e\nVui lòng kiểm tra Visual Studio / VS Code Web API (vohaduy_ltdd_api) đang chạy."
      };
    }
  }

  Future<Map<String, dynamic>> _fallbackStandardLogin(String usernameOrEmail, String password) async {
    String safeBaseUrl = baseUrl.endsWith('/') ? baseUrl : "$baseUrl/";
    Uri loginUri = Uri.parse("${safeBaseUrl}Authenticate/login");
    try {
      final response = await http.post(
        loginUri,
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({
          "username": usernameOrEmail,
          "password": password,
        }),
      ).timeout(const Duration(seconds: 30));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return await _processAuthResponse(data, usernameOrEmail);
      } else {
        String msg = "Tên đăng nhập hoặc mật khẩu không chính xác (${response.statusCode})";
        try {
          var err = jsonDecode(response.body);
          if (err['message'] != null) msg = err['message'];
        } catch (_) {}
        return {"success": false, "message": msg};
      }
    } catch (e) {
      return {
        "success": false,
        "message": "Lỗi kết nối Web API Backend ($loginUri): $e"
      };
    }
  }

  Future<Map<String, dynamic>> _processAuthResponse(Map<String, dynamic> data, String usernameOrEmail) async {
    bool isSuccess = data['status'] == true || data['Status'] == true || data['token'] != null;
    if (!isSuccess) {
      return {
        "success": false,
        "message": data['message'] ?? data['Message'] ?? "Xác thực thất bại!"
      };
    }

    String token = data['token'] ?? 'jwt_token_sample';
    String role = 'Employee';

    if (data['targetDashboard'] != null) {
      role = data['targetDashboard'].toString().contains('Admin') ? 'Admin' : 'Employee';
    } else if (data['roles'] != null && (data['roles'] as List).isNotEmpty) {
      role = data['roles'][0].toString();
    } else if (data['role'] != null) {
      role = data['role'].toString();
    }

    var userInfo = data['userInfo'] ?? {};
    UserModel user = UserModel(
      id: (userInfo['id'] ?? data['userId'] ?? '1').toString(),
      username: userInfo['username'] ?? usernameOrEmail,
      email: userInfo['email'] ?? data['email'] ?? '$usernameOrEmail@gmail.com',
      fullName: userInfo['fullName'] ?? data['fullName'] ?? usernameOrEmail,
      employeeCode: userInfo['employeeCode'] ?? 'NV001',
      role: role,
    );

    await _saveSession(token, role, user.username, userObj: user);

    return {
      "success": true,
      "token": token,
      "role": role,
      "user": user,
      "message": data['message'] ?? "Đăng nhập thành công!",
    };
  }

  // 2. Đăng ký tài khoản qua Web API (Không cần OTP)
  Future<Map<String, dynamic>> register({
    required String username,
    required String email,
    required String password,
    required String fullName,
    required String role,
  }) async {
    // Đảm bảo URL ghép nối không bị double slash (//)
    String safeBaseUrl = baseUrl.endsWith('/') ? baseUrl : "$baseUrl/";
    Uri regUri = Uri.parse("${safeBaseUrl}Authenticate/register");

    try {
      final response = await http.post(
        regUri,
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({
          "username": username,
          "email": email,
          "password": password,
          "fullName": fullName,
          "role": role,
        }),
      ).timeout(const Duration(seconds: 30));

      if (response.statusCode == 200 || response.statusCode == 201) {
        final data = jsonDecode(response.body);
        return {
          "success": true,
          "message": data['message'] ?? data['Message'] ?? "Đăng ký tài khoản SQL thành công!",
          "devOtp": data['devOtp'],
        };
      } else {
        String msg = "Lỗi đăng ký (${response.statusCode})";
        try {
          var err = jsonDecode(response.body);
          if (err['message'] != null) msg = err['message'];
        } catch (_) {}
        return {"success": false, "message": msg};
      }
    } catch (e) {
      return {
        "success": false,
        "message": "Lỗi kết nối Web API ($regUri): $e\nVui lòng kiểm tra backend API trong Visual Studio / VS Code đang chạy."
      };
    }
  }

  // 3. Gửi OTP Email qua C# Web API Backend (Authenticate/send-otp)
  Future<Map<String, dynamic>> sendOtpEmail(String email) async {
    Uri otpUri = Uri.parse("${baseUrl}Authenticate/send-otp");

    try {
      final response = await http.post(
        otpUri,
        headers: {"Content-Type": "application/json"},
        body: jsonEncode({"email": email}),
      ).timeout(const Duration(seconds: 30));

      if (response.statusCode == 200) {
        final data = jsonDecode(response.body);
        return {
          "success": true,
          "message": data['message'] ?? "Mã OTP đã được gửi về email $email",
          "devOtp": data['devOtp'],
        };
      } else {
        String msg = "Gửi OTP thất bại (${response.statusCode})";
        try {
          var err = jsonDecode(response.body);
          if (err['message'] != null) msg = err['message'];
        } catch (_) {}
        return {"success": false, "message": msg};
      }
    } catch (e) {
      return {
        "success": false,
        "message": "Không thể kết nối Web API SQL Server tại ($otpUri)!\nVui lòng bật Web API backend (vohaduy_ltdd_api) trong Visual Studio / VS Code rồi thử lại."
      };
    }
  }

  Future<void> _saveSession(String token, String role, String username, {UserModel? userObj}) async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.setString('jwt_token', token);
    await prefs.setString('user_role', role);
    await prefs.setString('username', username);
    if (userObj != null) {
      await prefs.setString('user_id', userObj.id);
      await prefs.setString('user_fullname', userObj.fullName);
      await prefs.setString('employee_code', userObj.employeeCode);
      await prefs.setString('email', userObj.email);
    }
  }

  Future<UserModel> getCurrentUser() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    String username = prefs.getString('username') ?? '';
    String role = prefs.getString('user_role') ?? 'Employee';
    String fullName = prefs.getString('user_fullname') ?? username;
    String empCode = prefs.getString('employee_code') ?? 'NV001';
    String email = prefs.getString('email') ?? '';
    String id = prefs.getString('user_id') ?? '1';

    return UserModel(
      id: id,
      username: username,
      email: email,
      fullName: fullName,
      employeeCode: empCode,
      role: role,
    );
  }

  Future<void> logout() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.clear();
  }
}

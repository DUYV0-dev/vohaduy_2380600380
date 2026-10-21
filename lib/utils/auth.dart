import '../services/auth_service.dart';

class Auth {
  static final AuthService _authService = AuthService();

  static Future<Map<String, dynamic>> login(String username, String password, {String? otpCode}) async {
    return await _authService.login(username, password, otpCode: otpCode);
  }

  static Future<Map<String, dynamic>> register({
    required String username,
    required String email,
    required String password,
    required String initials,
    required String role,
  }) async {
    return await _authService.register(
      username: username,
      email: email,
      password: password,
      fullName: initials,
      role: role,
    );
  }
}

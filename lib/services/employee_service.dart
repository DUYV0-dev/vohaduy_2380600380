import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';
import '../models/user_model.dart';

class EmployeeService {
  String get apiUrl => "${Config_URL.baseUrl}Employees";

  // Read / Fetch All Employees
  Future<List<UserModel>> getEmployees({int? departmentId, String? searchQuery}) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.get(
        Uri.parse(apiUrl),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200) {
        final List<dynamic> data = jsonDecode(response.body);
        List<UserModel> list = data.map((e) => UserModel.fromJson(e)).toList();
        if (departmentId != null) {
          list = list.where((e) => e.departmentId == departmentId).toList();
        }
        if (searchQuery != null && searchQuery.isNotEmpty) {
          list = list.where((e) =>
            e.fullName.toLowerCase().contains(searchQuery.toLowerCase()) ||
            e.employeeCode.toLowerCase().contains(searchQuery.toLowerCase())
          ).toList();
        }
        return list;
      }
    } catch (_) {}

    return [];
  }

  // Create Employee
  Future<bool> addEmployee(UserModel employee) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.post(
        Uri.parse(apiUrl),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode(employee.toJson()),
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      }
    } catch (_) {}

    return false;
  }

  // Update Employee
  Future<bool> updateEmployee(UserModel employee) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.put(
        Uri.parse("$apiUrl/${employee.id}"),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode(employee.toJson()),
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      }
    } catch (_) {}

    return false;
  }

  // Delete Employee
  Future<bool> deleteEmployee(String id) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.delete(
        Uri.parse("$apiUrl/$id"),
        headers: {
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      }
    } catch (_) {}

    return false;
  }
}

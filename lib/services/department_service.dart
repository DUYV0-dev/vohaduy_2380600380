import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import '../config/config_url.dart';
import '../models/department_model.dart';

class DepartmentService {
  String get apiUrl => "${Config_URL.baseUrl}Departments";

  // Fetch all departments
  Future<List<DepartmentModel>> getDepartments() async {
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
        return data.map((e) => DepartmentModel.fromJson(e)).toList();
      }
    } catch (_) {}

    return [];
  }

  // Add department
  Future<bool> addDepartment(DepartmentModel dept) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.post(
        Uri.parse(apiUrl),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode(dept.toJson()),
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200 || response.statusCode == 201) {
        return true;
      }
    } catch (_) {}

    return false;
  }

  // Update department
  Future<bool> updateDepartment(DepartmentModel dept) async {
    try {
      SharedPreferences prefs = await SharedPreferences.getInstance();
      String? token = prefs.getString('jwt_token');

      final response = await http.put(
        Uri.parse("$apiUrl/${dept.id}"),
        headers: {
          "Content-Type": "application/json",
          if (token != null && token.isNotEmpty) "Authorization": "Bearer $token",
        },
        body: jsonEncode(dept.toJson()),
      ).timeout(const Duration(seconds: 6));

      if (response.statusCode == 200 || response.statusCode == 204) {
        return true;
      }
    } catch (_) {}

    return false;
  }

  // Delete department
  Future<bool> deleteDepartment(int id) async {
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

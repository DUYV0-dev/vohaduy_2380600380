import 'package:flutter/material.dart';
import '../../models/user_model.dart';
import '../../services/employee_service.dart';

class EmployeeManagementScreen extends StatefulWidget {
  const EmployeeManagementScreen({super.key});

  @override
  State<EmployeeManagementScreen> createState() =>
      _EmployeeManagementScreenState();
}

class _EmployeeManagementScreenState extends State<EmployeeManagementScreen> {
  final EmployeeService _employeeService = EmployeeService();
  List<UserModel> _employees = [];
  bool _isLoading = true;
  String _searchQuery = "";

  @override
  void initState() {
    super.initState();
    _loadEmployees();
  }

  Future<void> _loadEmployees() async {
    setState(() => _isLoading = true);
    List<UserModel> data =
        await _employeeService.getEmployees(searchQuery: _searchQuery);
    setState(() {
      _employees = data;
      _isLoading = false;
    });
  }

  // --- SHOW DIALOG THÊM / SỬA NHÂN VIÊN ---
  void _showEmployeeDialog({UserModel? employee}) {
    final fullNameController =
        TextEditingController(text: employee?.fullName ?? '');
    final usernameController =
        TextEditingController(text: employee?.username ?? '');
    final emailController = TextEditingController(text: employee?.email ?? '');
    final phoneController = TextEditingController(text: employee?.phone ?? '');
    String role = employee?.role ?? 'Employee';

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          shape:
              RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          title: Text(employee == null ? 'Thêm Nhân Viên Mới' : 'Sửa Nhân Viên'),
          content: SingleChildScrollView(
            child: Column(
              mainAxisSize: MainAxisSize.min,
              children: [
                TextField(
                  controller: fullNameController,
                  decoration: const InputDecoration(
                    labelText: 'Họ và tên (*)',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: usernameController,
                  enabled: employee == null,
                  decoration: const InputDecoration(
                    labelText: 'Tên đăng nhập (*)',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: emailController,
                  keyboardType: TextInputType.emailAddress,
                  decoration: const InputDecoration(
                    labelText: 'Email (*)',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                TextField(
                  controller: phoneController,
                  keyboardType: TextInputType.phone,
                  decoration: const InputDecoration(
                    labelText: 'Số điện thoại',
                    border: OutlineInputBorder(),
                  ),
                ),
                const SizedBox(height: 12),
                DropdownButtonFormField<String>(
                  initialValue: role,
                  decoration: const InputDecoration(
                    labelText: 'Chức vụ / Vai trò',
                    border: OutlineInputBorder(),
                  ),
                  items: const [
                    DropdownMenuItem(value: 'Employee', child: Text('Nhân viên')),
                    DropdownMenuItem(value: 'Admin', child: Text('Quản trị viên (Admin)')),
                  ],
                  onChanged: (val) {
                    if (val != null) {
                      setDialogState(() => role = val);
                    }
                  },
                ),
              ],
            ),
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Hủy'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(
                  backgroundColor: const Color(0xFF004CBF)),
              onPressed: () async {
                String fullName = fullNameController.text.trim();
                String username = usernameController.text.trim();
                String email = emailController.text.trim();

                if (fullName.isEmpty || username.isEmpty || email.isEmpty) {
                  ScaffoldMessenger.of(context).showSnackBar(
                    const SnackBar(content: Text('Vui lòng điền đủ thông tin bắt buộc')),
                  );
                  return;
                }

                final messenger = ScaffoldMessenger.of(context);
                Navigator.pop(ctx);

                if (employee == null) {
                  // CREATE
                  UserModel newEmp = UserModel(
                    id: DateTime.now().millisecondsSinceEpoch.toString(),
                    username: username,
                    email: email,
                    fullName: fullName,
                    employeeCode: "NV00${_employees.length + 1}",
                    role: role,
                    departmentId: 1,
                    departmentName: 'Phòng Công Nghệ',
                    phone: phoneController.text.trim(),
                  );
                  await _employeeService.addEmployee(newEmp);
                } else {
                  // UPDATE
                  UserModel updated = employee.copyWith(
                    fullName: fullName,
                    email: email,
                    role: role,
                    phone: phoneController.text.trim(),
                  );
                  await _employeeService.updateEmployee(updated);
                }

                _loadEmployees();
                messenger.showSnackBar(
                  SnackBar(
                    content: Text(employee == null
                        ? 'Đã thêm nhân viên thành công!'
                        : 'Đã cập nhật nhân viên!'),
                    backgroundColor: Colors.green,
                  ),
                );
              },
              child: Text(employee == null ? 'Thêm' : 'Lưu',
                  style: const TextStyle(color: Colors.white)),
            ),
          ],
        ),
      ),
    );
  }

  // --- XÓA NHÂN VIÊN ---
  void _confirmDeleteEmployee(UserModel emp) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xác nhận xóa'),
        content: Text('Bạn có chắc chắn muốn xóa nhân viên ${emp.fullName}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () async {
              Navigator.pop(ctx);
              await _employeeService.deleteEmployee(emp.id);
              _loadEmployees();
              if (mounted) {
                ScaffoldMessenger.of(context).showSnackBar(
                  const SnackBar(
                    content: Text('Đã xóa nhân viên!'),
                    backgroundColor: Colors.green,
                  ),
                );
              }
            },
            child: const Text('Xóa', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Quản Lý Nhân Viên'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: Column(
        children: [
          // THANH TÌM KIẾM
          Padding(
            padding: const EdgeInsets.all(16.0),
            child: TextField(
              decoration: InputDecoration(
                hintText: 'Tìm theo tên hoặc mã NV...',
                prefixIcon: const Icon(Icons.search, color: Color(0xFF004CBF)),
                filled: true,
                fillColor: Colors.white,
                border: OutlineInputBorder(
                  borderRadius: BorderRadius.circular(16),
                  borderSide: BorderSide.none,
                ),
              ),
              onChanged: (val) {
                setState(() => _searchQuery = val);
                _loadEmployees();
              },
            ),
          ),

          // DANH SÁCH NHÂN VIÊN
          Expanded(
            child: _isLoading
                ? const Center(child: CircularProgressIndicator())
                : _employees.isEmpty
                    ? const Center(child: Text('Không tìm thấy nhân viên nào'))
                    : RefreshIndicator(
                        onRefresh: _loadEmployees,
                        child: ListView.builder(
                          padding: const EdgeInsets.only(bottom: 80),
                          itemCount: _employees.length,
                          itemBuilder: (context, index) {
                            final emp = _employees[index];
                            return Container(
                              margin: const EdgeInsets.symmetric(
                                  horizontal: 16, vertical: 6),
                              decoration: BoxDecoration(
                                color: Colors.white,
                                borderRadius: BorderRadius.circular(16),
                                boxShadow: const [
                                  BoxShadow(
                                    color: Colors.black12,
                                    blurRadius: 6,
                                    offset: Offset(0, 2),
                                  ),
                                ],
                              ),
                              child: ListTile(
                                leading: CircleAvatar(
                                  backgroundColor: const Color(0xFF004CBF),
                                  child: Text(
                                    emp.fullName.isNotEmpty
                                        ? emp.fullName[0].toUpperCase()
                                        : 'N',
                                    style: const TextStyle(
                                      color: Colors.white,
                                      fontWeight: FontWeight.bold,
                                    ),
                                  ),
                                ),
                                title: Text(
                                  emp.fullName,
                                  style: const TextStyle(
                                    fontWeight: FontWeight.bold,
                                  ),
                                ),
                                subtitle: Text(
                                  "Mã: ${emp.employeeCode} | Chức vụ: ${emp.role}\nPhòng: ${emp.departmentName ?? 'Chưa phân'}",
                                  style: const TextStyle(fontSize: 12),
                                ),
                                isThreeLine: true,
                                trailing: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    IconButton(
                                      icon: const Icon(Icons.edit,
                                          color: Colors.blue),
                                      onPressed: () =>
                                          _showEmployeeDialog(employee: emp),
                                    ),
                                    IconButton(
                                      icon: const Icon(Icons.delete,
                                          color: Colors.red),
                                      onPressed: () =>
                                          _confirmDeleteEmployee(emp),
                                    ),
                                  ],
                                ),
                              ),
                            );
                          },
                        ),
                      ),
          ),
        ],
      ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showEmployeeDialog(),
        icon: const Icon(Icons.person_add),
        label: const Text('Thêm Nhân Viên'),
        backgroundColor: const Color(0xFF004CBF),
        foregroundColor: Colors.white,
      ),
    );
  }
}

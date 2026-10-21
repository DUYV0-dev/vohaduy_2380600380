import 'package:flutter/material.dart';
import '../../models/department_model.dart';
import '../../services/department_service.dart';

class DepartmentManagementScreen extends StatefulWidget {
  const DepartmentManagementScreen({super.key});

  @override
  State<DepartmentManagementScreen> createState() =>
      _DepartmentManagementScreenState();
}

class _DepartmentManagementScreenState
    extends State<DepartmentManagementScreen> {
  final DepartmentService _departmentService = DepartmentService();
  List<DepartmentModel> _departments = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadDepartments();
  }

  Future<void> _loadDepartments() async {
    setState(() => _isLoading = true);
    List<DepartmentModel> data = await _departmentService.getDepartments();
    setState(() {
      _departments = data;
      _isLoading = false;
    });
  }

  void _showDepartmentDialog({DepartmentModel? department}) {
    final nameController = TextEditingController(text: department?.name ?? '');
    final codeController = TextEditingController(text: department?.code ?? '');
    final descController =
        TextEditingController(text: department?.description ?? '');

    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(department == null ? 'Thêm Phòng Ban' : 'Sửa Phòng Ban'),
        content: SingleChildScrollView(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              TextField(
                controller: nameController,
                decoration: const InputDecoration(
                  labelText: 'Tên phòng ban (*)',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: codeController,
                decoration: const InputDecoration(
                  labelText: 'Mã phòng ban',
                  border: OutlineInputBorder(),
                ),
              ),
              const SizedBox(height: 12),
              TextField(
                controller: descController,
                maxLines: 2,
                decoration: const InputDecoration(
                  labelText: 'Mô tả nhiệm vụ',
                  border: OutlineInputBorder(),
                ),
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
              String name = nameController.text.trim();
              if (name.isEmpty) return;

              Navigator.pop(ctx);
              if (department == null) {
                DepartmentModel newDept = DepartmentModel(
                  id: DateTime.now().millisecondsSinceEpoch % 10000,
                  code: codeController.text.trim().isNotEmpty
                      ? codeController.text.trim()
                      : 'PB0${_departments.length + 1}',
                  name: name,
                  description: descController.text.trim(),
                  employeeCount: 0,
                );
                await _departmentService.addDepartment(newDept);
              } else {
                DepartmentModel updated = DepartmentModel(
                  id: department.id,
                  code: codeController.text.trim(),
                  name: name,
                  description: descController.text.trim(),
                  employeeCount: department.employeeCount,
                );
                await _departmentService.updateDepartment(updated);
              }

              _loadDepartments();
            },
            child: Text(department == null ? 'Thêm' : 'Lưu',
                style: const TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  void _confirmDeleteDepartment(DepartmentModel dept) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xác nhận xóa'),
        content: Text('Xóa phòng ban ${dept.name}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () async {
              Navigator.pop(ctx);
              await _departmentService.deleteDepartment(dept.id);
              _loadDepartments();
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
        title: const Text('Quản Lý Phòng Ban'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : ListView.builder(
              padding: const EdgeInsets.only(top: 12, bottom: 80),
              itemCount: _departments.length,
              itemBuilder: (context, index) {
                final dept = _departments[index];
                return Container(
                  margin:
                      const EdgeInsets.symmetric(horizontal: 16, vertical: 6),
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
                    leading: const CircleAvatar(
                      backgroundColor: Color(0xFFE8F1FF),
                      child: Icon(Icons.corporate_fare,
                          color: Color(0xFF004CBF)),
                    ),
                    title: Text(dept.name,
                        style: const TextStyle(fontWeight: FontWeight.bold)),
                    subtitle: Text(
                        "Mã: ${dept.code} | Số nhân viên: ${dept.employeeCount}\n${dept.description}"),
                    isThreeLine: true,
                    trailing: Row(
                      mainAxisSize: MainAxisSize.min,
                      children: [
                        IconButton(
                          icon: const Icon(Icons.edit, color: Colors.blue),
                          onPressed: () =>
                              _showDepartmentDialog(department: dept),
                        ),
                        IconButton(
                          icon: const Icon(Icons.delete, color: Colors.red),
                          onPressed: () => _confirmDeleteDepartment(dept),
                        ),
                      ],
                    ),
                  ),
                );
              },
            ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: () => _showDepartmentDialog(),
        icon: const Icon(Icons.add),
        label: const Text('Thêm Phòng Ban'),
        backgroundColor: const Color(0xFF004CBF),
        foregroundColor: Colors.white,
      ),
    );
  }
}

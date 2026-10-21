import 'package:flutter/material.dart';
import '../../models/attendance_model.dart';
import '../../services/attendance_service.dart';

class AllAttendanceScreen extends StatefulWidget {
  const AllAttendanceScreen({super.key});

  @override
  State<AllAttendanceScreen> createState() => _AllAttendanceScreenState();
}

class _AllAttendanceScreenState extends State<AllAttendanceScreen> {
  final AttendanceService _attendanceService = AttendanceService();
  List<AttendanceModel> _list = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAllAttendances();
  }

  Future<void> _loadAllAttendances() async {
    setState(() => _isLoading = true);
    List<AttendanceModel> data = await _attendanceService.getAttendances();
    setState(() {
      _list = data;
      _isLoading = false;
    });
  }

  void _confirmDeleteAttendance(AttendanceModel item) {
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Xóa lượt chấm công'),
        content: Text('Xóa lượt chấm công của ${item.employeeName}?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(ctx),
            child: const Text('Hủy'),
          ),
          ElevatedButton(
            style: ElevatedButton.styleFrom(backgroundColor: Colors.red),
            onPressed: () async {
              Navigator.pop(ctx);
              await _attendanceService.deleteAttendance(item.id);
              _loadAllAttendances();
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
        title: const Text('Bảng Chấm Công Công Ty'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _list.isEmpty
              ? const Center(child: Text('Chưa có lượt chấm công nào'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _list.length,
                  itemBuilder: (context, index) {
                    final item = _list[index];
                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16)),
                      child: ListTile(
                        leading: CircleAvatar(
                          backgroundColor: item.type == 'Khuôn mặt'
                              ? Colors.purple.shade50
                              : item.type == 'Vị trí GPS'
                                  ? Colors.green.shade50
                                  : Colors.orange.shade50,
                          child: Icon(
                            item.type == 'Khuôn mặt'
                                ? Icons.face
                                : item.type == 'Vị trí GPS'
                                    ? Icons.my_location
                                    : Icons.qr_code,
                            color: const Color(0xFF004CBF),
                          ),
                        ),
                        title: Text(item.employeeName,
                            style:
                                const TextStyle(fontWeight: FontWeight.bold)),
                        subtitle: Text(
                          "Loại: ${item.type} | Trạng thái: ${item.status}\nThời gian: ${item.checkInTime.toString().split('.')[0]}",
                        ),
                        isThreeLine: true,
                        trailing: IconButton(
                          icon: const Icon(Icons.delete_outline, color: Colors.red),
                          onPressed: () => _confirmDeleteAttendance(item),
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}

import 'package:flutter/material.dart';
import '../../models/leave_request_model.dart';
import '../../services/auth_service.dart';
import '../../services/leave_service.dart';

class LeaveRequestScreen extends StatefulWidget {
  const LeaveRequestScreen({super.key});

  @override
  State<LeaveRequestScreen> createState() => _LeaveRequestScreenState();
}

class _LeaveRequestScreenState extends State<LeaveRequestScreen> {
  final LeaveService _leaveService = LeaveService();
  List<LeaveRequestModel> _myRequests = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadMyLeaveRequests();
  }

  Future<void> _loadMyLeaveRequests() async {
    setState(() => _isLoading = true);
    AuthService authService = AuthService();
    var user = await authService.getCurrentUser();

    List<LeaveRequestModel> data =
        await _leaveService.getLeaveRequests(employeeId: user.id);
    setState(() {
      _myRequests = data;
      _isLoading = false;
    });
  }

  void _showCreateLeaveDialog() {
    final reasonController = TextEditingController();
    String leaveType = 'Nghỉ phép năm';

    showDialog(
      context: context,
      builder: (ctx) => StatefulBuilder(
        builder: (context, setDialogState) => AlertDialog(
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          title: const Text('Tạo Đơn Xin Nghỉ Phép'),
          content: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              DropdownButtonFormField<String>(
                initialValue: leaveType,
                decoration: const InputDecoration(
                  labelText: 'Loại nghỉ phép',
                  border: OutlineInputBorder(),
                ),
                items: const [
                  DropdownMenuItem(value: 'Nghỉ phép năm', child: Text('Nghỉ phép năm')),
                  DropdownMenuItem(value: 'Nghỉ bệnh', child: Text('Nghỉ bệnh')),
                  DropdownMenuItem(value: 'Nghỉ việc riêng', child: Text('Nghỉ việc riêng')),
                ],
                onChanged: (val) {
                  if (val != null) setDialogState(() => leaveType = val);
                },
              ),
              const SizedBox(height: 12),
              TextField(
                controller: reasonController,
                maxLines: 3,
                decoration: const InputDecoration(
                  labelText: 'Lý do xin nghỉ (*)',
                  border: OutlineInputBorder(),
                ),
              ),
            ],
          ),
          actions: [
            TextButton(
              onPressed: () => Navigator.pop(ctx),
              child: const Text('Hủy'),
            ),
            ElevatedButton(
              style: ElevatedButton.styleFrom(backgroundColor: const Color(0xFF004CBF)),
              onPressed: () async {
                String reason = reasonController.text.trim();
                if (reason.isEmpty) return;

                final messenger = ScaffoldMessenger.of(context);
                Navigator.pop(ctx);
                AuthService authService = AuthService();
                var user = await authService.getCurrentUser();

                LeaveRequestModel req = LeaveRequestModel(
                  id: 'leave_${DateTime.now().millisecondsSinceEpoch}',
                  employeeId: user.id,
                  employeeName: user.fullName,
                  employeeCode: user.employeeCode,
                  reason: reason,
                  startDate: DateTime.now().add(const Duration(days: 1)),
                  endDate: DateTime.now().add(const Duration(days: 1)),
                  leaveType: leaveType,
                  status: 'Chờ duyệt',
                  createdAt: DateTime.now(),
                );

                await _leaveService.createLeaveRequest(req);
                _loadMyLeaveRequests();
                messenger.showSnackBar(
                  const SnackBar(
                    content: Text('Đã nộp đơn xin nghỉ phép thành công!'),
                    backgroundColor: Colors.green,
                  ),
                );
              },
              child: const Text('Nộp Đơn', style: TextStyle(color: Colors.white)),
            ),
          ],
        ),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Đơn Xin Nghỉ Phép Cá Nhân'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _myRequests.isEmpty
              ? const Center(child: Text('Bạn chưa gửi đơn xin nghỉ phép nào'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _myRequests.length,
                  itemBuilder: (context, index) {
                    final req = _myRequests[index];
                    Color statusColor = Colors.orange;
                    if (req.status == 'Đã duyệt') statusColor = Colors.green;
                    if (req.status == 'Từ chối') statusColor = Colors.red;

                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  req.leaveType,
                                  style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: statusColor.withValues(alpha: 0.15),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    req.status,
                                    style: TextStyle(color: statusColor, fontWeight: FontWeight.bold, fontSize: 12),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 8),
                            Text("Lý do: ${req.reason}"),
                            if (req.adminComment != null) ...[
                              const SizedBox(height: 6),
                              Text("Phản hồi Admin: ${req.adminComment}", style: const TextStyle(color: Colors.blue, fontStyle: FontStyle.italic)),
                            ],
                          ],
                        ),
                      ),
                    );
                  },
                ),
      floatingActionButton: FloatingActionButton.extended(
        onPressed: _showCreateLeaveDialog,
        icon: const Icon(Icons.add_comment_outlined),
        label: const Text('Tạo Đơn Xin Nghỉ'),
        backgroundColor: const Color(0xFF004CBF),
        foregroundColor: Colors.white,
      ),
    );
  }
}

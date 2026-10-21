import 'package:flutter/material.dart';
import '../../models/leave_request_model.dart';
import '../../services/leave_service.dart';

class LeaveApprovalScreen extends StatefulWidget {
  const LeaveApprovalScreen({super.key});

  @override
  State<LeaveApprovalScreen> createState() => _LeaveApprovalScreenState();
}

class _LeaveApprovalScreenState extends State<LeaveApprovalScreen> {
  final LeaveService _leaveService = LeaveService();
  List<LeaveRequestModel> _requests = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadRequests();
  }

  Future<void> _loadRequests() async {
    setState(() => _isLoading = true);
    List<LeaveRequestModel> data = await _leaveService.getLeaveRequests();
    setState(() {
      _requests = data;
      _isLoading = false;
    });
  }

  void _handleApproval(LeaveRequestModel req, String newStatus) {
    TextEditingController commentController = TextEditingController();
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: Text(newStatus == 'Đã duyệt' ? 'Duyệt Đơn Nghỉ Phép' : 'Từ Chối Đơn Nghỉ'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text("Nhân viên: ${req.employeeName} (${req.employeeCode})"),
            Text("Lý do: ${req.reason}"),
            const SizedBox(height: 12),
            TextField(
              controller: commentController,
              decoration: const InputDecoration(
                labelText: 'Ghi chú phê duyệt',
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
            style: ElevatedButton.styleFrom(
              backgroundColor: newStatus == 'Đã duyệt' ? Colors.green : Colors.red,
            ),
            onPressed: () async {
              Navigator.pop(ctx);
              await _leaveService.updateLeaveStatus(
                req.id,
                newStatus,
                adminComment: commentController.text.trim(),
              );
              _loadRequests();
            },
            child: Text(newStatus == 'Đã duyệt' ? 'Chấp nhận' : 'Từ chối',
                style: const TextStyle(color: Colors.white)),
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
        title: const Text('Duyệt Đơn Nghỉ Phép'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _requests.isEmpty
              ? const Center(child: Text('Không có đơn xin nghỉ phép nào'))
              : ListView.builder(
                  padding: const EdgeInsets.all(16),
                  itemCount: _requests.length,
                  itemBuilder: (context, index) {
                    final req = _requests[index];
                    Color statusColor = Colors.orange;
                    String displayStatus = req.status;

                    if (req.status == 'Đã duyệt' || req.status == 'Approved') {
                      statusColor = Colors.green;
                      displayStatus = 'Đã duyệt';
                    }
                    if (req.status == 'Từ chối' || req.status == 'Rejected') {
                      statusColor = Colors.red;
                      displayStatus = 'Từ chối';
                    }
                    if (req.status == 'Pending') {
                      displayStatus = 'Chờ duyệt';
                    }

                    return Card(
                      margin: const EdgeInsets.only(bottom: 12),
                      shape: RoundedRectangleBorder(
                          borderRadius: BorderRadius.circular(16)),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Row(
                              mainAxisAlignment: MainAxisAlignment.spaceBetween,
                              children: [
                                Text(
                                  req.employeeName,
                                  style: const TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 16),
                                ),
                                Container(
                                  padding: const EdgeInsets.symmetric(
                                      horizontal: 10, vertical: 4),
                                  decoration: BoxDecoration(
                                    color: statusColor.withValues(alpha: 0.15),
                                    borderRadius: BorderRadius.circular(12),
                                  ),
                                  child: Text(
                                    displayStatus,
                                    style: TextStyle(
                                        color: statusColor,
                                        fontWeight: FontWeight.bold,
                                        fontSize: 12),
                                  ),
                                ),
                              ],
                            ),
                            const SizedBox(height: 4),
                            Text("Loại: ${req.leaveType} | Mã NV: ${req.employeeCode}"),
                            const SizedBox(height: 8),
                            Text("Lý do: ${req.reason}",
                                style: const TextStyle(
                                    fontStyle: FontStyle.italic)),
                            const SizedBox(height: 12),
                            if (req.status == 'Chờ duyệt' || req.status == 'Pending')
                              Row(
                                mainAxisAlignment: MainAxisAlignment.end,
                                children: [
                                  OutlinedButton(
                                    style: OutlinedButton.styleFrom(
                                        foregroundColor: Colors.red),
                                    onPressed: () =>
                                        _handleApproval(req, 'Từ chối'),
                                    child: const Text('Từ chối'),
                                  ),
                                  const SizedBox(width: 8),
                                  ElevatedButton(
                                    style: ElevatedButton.styleFrom(
                                        backgroundColor: Colors.green),
                                    onPressed: () =>
                                        _handleApproval(req, 'Đã duyệt'),
                                    child: const Text('Chấp nhận',
                                        style: TextStyle(color: Colors.white)),
                                  ),
                                ],
                              ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
    );
  }
}

import 'package:flutter/material.dart';
import '../../models/attendance_model.dart';
import '../../services/attendance_service.dart';
import '../../services/auth_service.dart';
import '../features/ai_assistant_screen.dart';
import '../features/face_attendance_screen.dart';
import '../features/gps_attendance_screen.dart';
import '../features/video_call_screen.dart';
import 'leave_request_screen.dart';
import 'profile_screen.dart';

class EmployeeDashboardScreen extends StatefulWidget {
  const EmployeeDashboardScreen({super.key});

  @override
  State<EmployeeDashboardScreen> createState() => _EmployeeDashboardScreenState();
}

class _EmployeeDashboardScreenState extends State<EmployeeDashboardScreen> {
  int _currentIndex = 0;

  @override
  Widget build(BuildContext context) {
    final List<Widget> screens = [
      const EmployeeHomeTab(),
      const EmployeeHistoryTab(),
      const LeaveRequestScreen(),
      const AiAssistantScreen(),
      const ProfileScreen(),
    ];

    return Scaffold(
      body: IndexedStack(
        index: _currentIndex,
        children: screens,
      ),
      bottomNavigationBar: BottomNavigationBar(
        currentIndex: _currentIndex,
        onTap: (index) {
          setState(() {
            _currentIndex = index;
          });
        },
        type: BottomNavigationBarType.fixed,
        selectedItemColor: const Color(0xFF004CBF),
        unselectedItemColor: Colors.grey,
        items: const [
          BottomNavigationBarItem(
            icon: Icon(Icons.touch_app_outlined),
            activeIcon: Icon(Icons.touch_app),
            label: 'Chấm công',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.history_outlined),
            activeIcon: Icon(Icons.history),
            label: 'Lịch sử',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.assignment_outlined),
            activeIcon: Icon(Icons.assignment),
            label: 'Đơn nghỉ',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.psychology_outlined),
            activeIcon: Icon(Icons.psychology),
            label: 'Trợ lý AI',
          ),
          BottomNavigationBarItem(
            icon: Icon(Icons.person_outline),
            activeIcon: Icon(Icons.person),
            label: 'Cá nhân',
          ),
        ],
      ),
    );
  }
}

// TAB TRANG CHỦ CHẤM CÔNG NHANH
class EmployeeHomeTab extends StatefulWidget {
  const EmployeeHomeTab({super.key});

  @override
  State<EmployeeHomeTab> createState() => _EmployeeHomeTabState();
}

class _EmployeeHomeTabState extends State<EmployeeHomeTab> {
  String _employeeName = "Võ Hà Duy";
  String _employeeCode = "NV002";

  @override
  void initState() {
    super.initState();
    _loadUser();
  }

  Future<void> _loadUser() async {
    AuthService authService = AuthService();
    var user = await authService.getCurrentUser();
    if (mounted) {
      setState(() {
        _employeeName = user.fullName;
        _employeeCode = user.employeeCode;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Dashboard Chấm Công'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: SingleChildScrollView(
        child: Column(
          children: [
            // HEADER WELCOME CARD
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(20),
              decoration: const BoxDecoration(
                color: Color(0xFF004CBF),
                borderRadius: BorderRadius.only(
                  bottomLeft: Radius.circular(30),
                  bottomRight: Radius.circular(30),
                ),
              ),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Row(
                    mainAxisAlignment: MainAxisAlignment.spaceBetween,
                    children: [
                      Text(
                        'Xin chào, $_employeeName 👋',
                        style: const TextStyle(
                          color: Colors.white,
                          fontSize: 22,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Container(
                        padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 4),
                        decoration: BoxDecoration(
                          color: AttendanceService.isWithinShiftHours(DateTime.now())
                              ? Colors.green.withValues(alpha: 0.25)
                              : Colors.orange.withValues(alpha: 0.25),
                          borderRadius: BorderRadius.circular(12),
                          border: Border.all(
                            color: AttendanceService.isWithinShiftHours(DateTime.now())
                                ? Colors.greenAccent
                                : Colors.orangeAccent,
                          ),
                        ),
                        child: Text(
                          AttendanceService.isWithinShiftHours(DateTime.now())
                              ? 'Trong ca làm'
                              : 'Ngoài ca làm',
                          style: TextStyle(
                            color: AttendanceService.isWithinShiftHours(DateTime.now())
                                ? Colors.greenAccent
                                : Colors.orangeAccent,
                            fontSize: 11,
                            fontWeight: FontWeight.bold,
                          ),
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 4),
                  Text(
                    'Mã NV: $_employeeCode | Ca quy định: 08:00 - 17:00',
                    style: const TextStyle(color: Colors.white70, fontSize: 13),
                  ),
                  const SizedBox(height: 16),
                  Container(
                    padding: const EdgeInsets.all(12),
                    decoration: BoxDecoration(
                      color: Colors.white.withValues(alpha: 0.15),
                      borderRadius: BorderRadius.circular(16),
                    ),
                    child: const Row(
                      mainAxisAlignment: MainAxisAlignment.spaceAround,
                      children: [
                        Column(
                          children: [
                            Text('22', style: TextStyle(color: Colors.white, fontSize: 20, fontWeight: FontWeight.bold)),
                            Text('Công tháng này', style: TextStyle(color: Colors.white70, fontSize: 11)),
                          ],
                        ),
                        Column(
                          children: [
                            Text('0', style: TextStyle(color: Colors.greenAccent, fontSize: 20, fontWeight: FontWeight.bold)),
                            Text('Đi trễ', style: TextStyle(color: Colors.white70, fontSize: 11)),
                          ],
                        ),
                        Column(
                          children: [
                            Text('1', style: TextStyle(color: Colors.orangeAccent, fontSize: 20, fontWeight: FontWeight.bold)),
                            Text('Nghỉ phép', style: TextStyle(color: Colors.white70, fontSize: 11)),
                          ],
                        ),
                      ],
                    ),
                  ),
                ],
              ),
            ),

            const SizedBox(height: 24),
            const Padding(
              padding: EdgeInsets.symmetric(horizontal: 20.0),
              child: Align(
                alignment: Alignment.centerLeft,
                child: Text(
                  'Lựa chọn hình thức chấm công:',
                  style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold, color: Color(0xFF004CBF)),
                ),
              ),
            ),
            const SizedBox(height: 16),

            // 3 THẺ CHẤM CÔNG
            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 20.0),
              child: Column(
                children: [
                  // 1. KHUÔN MẶT
                  Card(
                    elevation: 3,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    child: ListTile(
                      contentPadding: const EdgeInsets.all(16),
                      leading: const CircleAvatar(
                        radius: 26,
                        backgroundColor: Color(0xFFE8F1FF),
                        child: Icon(Icons.face, size: 32, color: Color(0xFF004CBF)),
                      ),
                      title: const Text('a. Chấm công Khuôn mặt AI', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                      subtitle: const Text('Nhận diện camera quét khuôn mặt chính xác'),
                      trailing: const Icon(Icons.arrow_forward_ios, size: 16, color: Color(0xFF004CBF)),
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(builder: (context) => const FaceAttendanceScreen()),
                        );
                      },
                    ),
                  ),
                  const SizedBox(height: 12),

                  // 2. VỊ TRÍ GPS
                  Card(
                    elevation: 3,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    child: ListTile(
                      contentPadding: const EdgeInsets.all(16),
                      leading: const CircleAvatar(
                        radius: 26,
                        backgroundColor: Color(0xFFE8F1FF),
                        child: Icon(Icons.my_location, size: 32, color: Color(0xFF004CBF)),
                      ),
                      title: const Text('b. Chấm công Vị trí GPS', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                      subtitle: const Text('Định vị Geofencing 70 Đ. D1, Tăng Nhơn Phú (100m)'),
                      trailing: const Icon(Icons.arrow_forward_ios, size: 16, color: Color(0xFF004CBF)),
                      onTap: () {
                        Navigator.push(
                          context,
                          MaterialPageRoute(builder: (context) => const GpsAttendanceScreen()),
                        );
                      },
                    ),
                  ),
                  const SizedBox(height: 12),

                  // 3. GỌI VIDEO NHÓM TRỰC TUYẾN
                  Card(
                    elevation: 3,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    child: ListTile(
                      contentPadding: const EdgeInsets.all(16),
                      leading: const CircleAvatar(
                        radius: 26,
                        backgroundColor: Color(0xFFE8F1FF),
                        child: Icon(Icons.video_call, size: 32, color: Color(0xFF004CBF)),
                      ),
                      title: const Text('c. Gọi Video Nhóm Trực Tuyến', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
                      subtitle: const Text('Tạo phòng hoặc nhập mã phòng họp video nhóm'),
                      trailing: const Icon(Icons.arrow_forward_ios, size: 16, color: Color(0xFF004CBF)),
                      onTap: () {
                        TextEditingController roomCodeController = TextEditingController(text: "CALL-P001");
                        showDialog(
                          context: context,
                          builder: (ctx) => AlertDialog(
                            title: const Text('Gọi Video Nhóm Online'),
                            content: Column(
                              mainAxisSize: MainAxisSize.min,
                              crossAxisAlignment: CrossAxisAlignment.start,
                              children: [
                                const Text('Nhập Mã Phòng họp video để khởi tạo / tham gia:', style: TextStyle(fontSize: 13)),
                                const SizedBox(height: 12),
                                TextField(
                                  controller: roomCodeController,
                                  decoration: const InputDecoration(
                                    labelText: 'Mã Phòng Họp Video (*)',
                                    hintText: 'Ví dụ: CALL-001',
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
                                onPressed: () {
                                  String code = roomCodeController.text.trim();
                                  if (code.isEmpty) code = "CALL-001";
                                  Navigator.pop(ctx);

                                  Navigator.push(
                                    context,
                                    MaterialPageRoute(
                                      builder: (context) => VideoCallScreen(roomCode: code),
                                    ),
                                  );
                                },
                                child: const Text('VÀO PHÒNG HỌP VIDEO', style: TextStyle(color: Colors.white)),
                              ),
                            ],
                          ),
                        );
                      },
                    ),
                  ),
                ],
              ),
            ),
            const SizedBox(height: 30),
          ],
        ),
      ),
    );
  }
}

// TAB LỊCH SỬ CHẤM CÔNG CÁ NHÂN
class EmployeeHistoryTab extends StatefulWidget {
  const EmployeeHistoryTab({super.key});

  @override
  State<EmployeeHistoryTab> createState() => _EmployeeHistoryTabState();
}

class _EmployeeHistoryTabState extends State<EmployeeHistoryTab> {
  final AttendanceService _attendanceService = AttendanceService();
  List<AttendanceModel> _myHistory = [];
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadHistory();
  }

  Future<void> _loadHistory() async {
    setState(() => _isLoading = true);
    AuthService authService = AuthService();
    var user = await authService.getCurrentUser();

    List<AttendanceModel> data =
        await _attendanceService.getAttendances(employeeId: user.id);
    setState(() {
      _myHistory = data;
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Lịch Sử Chấm Công'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.refresh),
            onPressed: _loadHistory,
          ),
        ],
      ),
      body: RefreshIndicator(
        onRefresh: _loadHistory,
        child: _isLoading
            ? const Center(child: CircularProgressIndicator())
            : _myHistory.isEmpty
                ? ListView(
                    physics: const AlwaysScrollableScrollPhysics(),
                    children: [
                      SizedBox(
                        height: MediaQuery.of(context).size.height * 0.7,
                        child: const Center(
                          child: Column(
                            mainAxisAlignment: MainAxisAlignment.center,
                            children: [
                              Icon(Icons.history_toggle_off, size: 60, color: Colors.grey),
                              SizedBox(height: 12),
                              Text('Bạn chưa có lượt chấm công nào', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16, color: Colors.grey)),
                              SizedBox(height: 4),
                              Text('Kéo xuống để tải lại dữ liệu từ Web API', style: TextStyle(fontSize: 12, color: Colors.grey)),
                            ],
                          ),
                        ),
                      ),
                    ],
                  )
                : ListView.builder(
                    padding: const EdgeInsets.all(16),
                    itemCount: _myHistory.length,
                    itemBuilder: (context, index) {
                      final item = _myHistory[index];
                      return Card(
                        margin: const EdgeInsets.only(bottom: 12),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                        child: ListTile(
                          leading: CircleAvatar(
                            backgroundColor: const Color(0xFFE8F1FF),
                            child: Icon(
                              item.type == 'Khuôn mặt'
                                  ? Icons.face
                                  : Icons.my_location,
                              color: const Color(0xFF004CBF),
                            ),
                          ),
                          title: Text("${item.type} • ${item.status}", style: const TextStyle(fontWeight: FontWeight.bold)),
                          subtitle: Text(
                              "Thời gian: ${item.checkInTime.toString().split('.')[0]}\nĐịa điểm: ${item.locationName}\nGhi chú: ${item.note.isNotEmpty ? item.note : 'Không có'}"),
                          isThreeLine: true,
                        ),
                      );
                    },
                  ),
      ),
    );
  }
}

import 'package:flutter/material.dart';
import '../login_screen.dart';
import '../../services/auth_service.dart';

class ProfileScreen extends StatelessWidget {
  const ProfileScreen({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Thông Tin Cá Nhân & Sinh Viên'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.logout),
            tooltip: 'Đăng xuất',
            onPressed: () async {
              AuthService auth = AuthService();
              await auth.logout();
              if (context.mounted) {
                Navigator.pushReplacement(
                  context,
                  MaterialPageRoute(builder: (context) => const LoginScreen()),
                );
              }
            },
          ),
        ],
      ),
      body: SingleChildScrollView(
        child: Column(
          children: [
            // Header cong màu xanh
            Container(
              width: double.infinity,
              padding: const EdgeInsets.only(top: 20, bottom: 40),
              decoration: const BoxDecoration(
                color: Color(0xFF004CBF),
                borderRadius: BorderRadius.only(
                  bottomLeft: Radius.circular(30),
                  bottomRight: Radius.circular(30),
                ),
              ),
              child: const Column(
                children: [
                  CircleAvatar(
                    radius: 50,
                    backgroundColor: Colors.white,
                    child: CircleAvatar(
                      radius: 46,
                      backgroundColor: Color(0xFF004CBF),
                      child: Icon(Icons.person, size: 54, color: Colors.white),
                    ),
                  ),
                  SizedBox(height: 12),
                  Text(
                    'Võ Hà Duy',
                    style: TextStyle(
                      fontSize: 24,
                      fontWeight: FontWeight.bold,
                      color: Colors.white,
                    ),
                  ),
                  SizedBox(height: 4),
                  Text(
                    'Chức vụ: Nhân viên / Sinh viên thực hiện',
                    style: TextStyle(color: Colors.white70, fontSize: 13),
                  ),
                ],
              ),
            ),

            Padding(
              padding: const EdgeInsets.all(16.0),
              child: Card(
                elevation: 4,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Column(
                  children: [
                    const ListTile(
                      leading: Icon(Icons.badge, color: Color(0xFF004CBF)),
                      title: Text('Họ và tên tác giả'),
                      subtitle: Text('Võ Hà Duy', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                    ),
                    const Divider(height: 1),
                    const ListTile(
                      leading: Icon(Icons.numbers, color: Color(0xFF004CBF)),
                      title: Text('Mã số sinh viên (MSSV)'),
                      subtitle: Text('2380600380', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 15)),
                    ),
                    const Divider(height: 1),
                    const ListTile(
                      leading: Icon(Icons.school, color: Color(0xFF004CBF)),
                      title: Text('Lớp học'),
                      subtitle: Text('23DTHC4 (Chiều thứ 4)'),
                    ),
                    const Divider(height: 1),
                    const ListTile(
                      leading: Icon(Icons.computer, color: Color(0xFF004CBF)),
                      title: Text('Khoa'),
                      subtitle: Text('Công nghệ thông tin'),
                    ),
                    const Divider(height: 1),
                    const ListTile(
                      leading: Icon(Icons.book, color: Color(0xFF004CBF)),
                      title: Text('Môn học'),
                      subtitle: Text('Lập trình Thiết bị Di động'),
                    ),
                    const Divider(height: 1),
                    const ListTile(
                      leading: Icon(Icons.app_shortcut, color: Color(0xFF004CBF)),
                      title: Text('Tên dự án ứng dụng'),
                      subtitle: Text('vohaduy_ltdd_ui (App Chấm Công Nhân Viên)'),
                    ),
                  ],
                ),
              ),
            ),

            Padding(
              padding: const EdgeInsets.symmetric(horizontal: 16.0),
              child: SizedBox(
                width: double.infinity,
                height: 48,
                child: OutlinedButton.icon(
                  icon: const Icon(Icons.logout, color: Colors.red),
                  label: const Text('ĐĂNG XUẤT TÀI KHOẢN', style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold)),
                  style: OutlinedButton.styleFrom(
                    side: const BorderSide(color: Colors.red),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                  ),
                  onPressed: () async {
                    AuthService auth = AuthService();
                    await auth.logout();
                    if (context.mounted) {
                      Navigator.pushReplacement(
                        context,
                        MaterialPageRoute(builder: (context) => const LoginScreen()),
                      );
                    }
                  },
                ),
              ),
            ),
            const SizedBox(height: 30),
          ],
        ),
      ),
    );
  }
}

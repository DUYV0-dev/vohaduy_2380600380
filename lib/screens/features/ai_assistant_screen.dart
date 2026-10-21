import 'package:flutter/material.dart';
import '../../models/attendance_model.dart';
import '../../services/ai_assistant_service.dart';
import '../../services/attendance_service.dart';

class AiAssistantScreen extends StatefulWidget {
  const AiAssistantScreen({super.key});

  @override
  State<AiAssistantScreen> createState() => _AiAssistantScreenState();
}

class _AiAssistantScreenState extends State<AiAssistantScreen> {
  Map<String, dynamic>? _aiReport;
  bool _isLoading = true;

  @override
  void initState() {
    super.initState();
    _loadAiAnalysis();
  }

  Future<void> _loadAiAnalysis() async {
    setState(() => _isLoading = true);
    AttendanceService attService = AttendanceService();
    List<AttendanceModel> history = await attService.getAttendances();

    setState(() {
      _aiReport = AiAssistantService.analyzeAttendanceRisk(history);
      _isLoading = false;
    });
  }

  @override
  Widget build(BuildContext context) {
    if (_isLoading || _aiReport == null) {
      return const Scaffold(
        body: Center(child: CircularProgressIndicator()),
      );
    }

    double punctuality = _aiReport!['punctualityRate'] ?? 100.0;
    double riskScore = _aiReport!['riskScore'] ?? 0.0;
    String warningLevel = _aiReport!['warningLevel'] ?? 'An toàn';
    String adviceText = _aiReport!['adviceText'] ?? '';

    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Trợ Lý AI Cảnh Báo Chấm Công'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // BANNER BÁO CÁO AI
              Container(
                padding: const EdgeInsets.all(20),
                decoration: BoxDecoration(
                  gradient: const LinearGradient(
                    colors: [Color(0xFF004CBF), Color(0xFF0D6EFD)],
                    begin: Alignment.topLeft,
                    end: Alignment.bottomRight,
                  ),
                  borderRadius: BorderRadius.circular(20),
                  boxShadow: const [
                    BoxShadow(color: Colors.black12, blurRadius: 10, offset: Offset(0, 4)),
                  ],
                ),
                child: Column(
                  children: [
                    const CircleAvatar(
                      radius: 36,
                      backgroundColor: Colors.white24,
                      child: Icon(Icons.psychology, size: 44, color: Colors.white),
                    ),
                    const SizedBox(height: 12),
                    const Text(
                      'AI Attendance Smart Assistant',
                      style: TextStyle(
                        color: Colors.white,
                        fontSize: 20,
                        fontWeight: FontWeight.bold,
                      ),
                    ),
                    const SizedBox(height: 4),
                    const Text(
                      'Tự động dự báo kỷ luật & Tỷ lệ phạt lương',
                      style: TextStyle(color: Colors.white70, fontSize: 13),
                    ),
                  ],
                ),
              ),

              const SizedBox(height: 20),

              // CHỈ SỐ KỶ LUẬT
              Row(
                children: [
                  Expanded(
                    child: Card(
                      elevation: 3,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          children: [
                            const Text('Điểm Đúng Giờ', style: TextStyle(color: Colors.grey, fontSize: 12)),
                            const SizedBox(height: 8),
                            Text(
                              "${punctuality.toStringAsFixed(0)}%",
                              style: const TextStyle(
                                fontSize: 28,
                                fontWeight: FontWeight.bold,
                                color: Colors.green,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                  const SizedBox(width: 12),
                  Expanded(
                    child: Card(
                      elevation: 3,
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                      child: Padding(
                        padding: const EdgeInsets.all(16.0),
                        child: Column(
                          children: [
                            const Text('Tỷ lệ Rủi ro Trừ Lương', style: TextStyle(color: Colors.grey, fontSize: 12)),
                            const SizedBox(height: 8),
                            Text(
                              "${riskScore.toStringAsFixed(0)}%",
                              style: TextStyle(
                                fontSize: 28,
                                fontWeight: FontWeight.bold,
                                color: riskScore > 20 ? Colors.red : Colors.orange,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),

              const SizedBox(height: 20),

              // MỨC CẢNH BÁO & LỜI KHUYÊN AI
              Card(
                elevation: 3,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Row(
                        children: [
                          Icon(
                            Icons.warning_amber_rounded,
                            color: riskScore > 20 ? Colors.red : Colors.green,
                            size: 28,
                          ),
                          const SizedBox(width: 8),
                          Text(
                            "Trạng thái: $warningLevel",
                            style: TextStyle(
                              fontSize: 16,
                              fontWeight: FontWeight.bold,
                              color: riskScore > 20 ? Colors.red : Colors.green[800],
                            ),
                          ),
                        ],
                      ),
                      const Divider(height: 24),
                      const Text(
                        'Lời khuyên từ Trợ lý AI:',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14),
                      ),
                      const SizedBox(height: 8),
                      Text(
                        adviceText,
                        style: const TextStyle(fontSize: 14, height: 1.4),
                      ),
                    ],
                  ),
                ),
              ),

              const SizedBox(height: 20),

              // NHẮC BÁO THỨC CA LÀM
              Card(
                elevation: 2,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                child: ListTile(
                  leading: const CircleAvatar(
                    backgroundColor: Color(0xFFE8F1FF),
                    child: Icon(Icons.alarm, color: Color(0xFF004CBF)),
                  ),
                  title: const Text('Gợi ý đặt báo thức ca làm tiếp theo'),
                  subtitle: Text("Ca làm: ${_aiReport!['nextShiftTime']} -> Báo thức khuyên dùng: ${_aiReport!['recommendedAlarm']}"),
                  trailing: Switch(
                    value: true,
                    activeThumbColor: const Color(0xFF004CBF),
                    onChanged: (v) {},
                  ),
                ),
              ),
            ],
          ),
        ),
      ),
    );
  }
}

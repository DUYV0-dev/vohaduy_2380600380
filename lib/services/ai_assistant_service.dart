import '../models/attendance_model.dart';

class AiAssistantService {
  // AI Phân tích xu hướng & Cảnh báo Chấm công
  static Map<String, dynamic> analyzeAttendanceRisk(List<AttendanceModel> history) {
    int totalCheckIns = history.length;
    int lateCount = history.where((a) => a.status.contains('Đi trễ')).length;
    int onTimeCount = history.where((a) => a.status.contains('Đúng giờ')).length;

    double punctualityRate = totalCheckIns > 0 ? (onTimeCount / totalCheckIns) * 100 : 100.0;
    double riskScore = 100.0 - punctualityRate; // Tỷ lệ rủi ro %

    String warningLevel = "An toàn";
    String adviceText = "Bạn có lịch sử chấm công rất chuẩn xác! Tiếp tục duy trì phong độ nhé.";

    if (lateCount >= 3) {
      warningLevel = "Nguy hiểm (Cảnh báo Trừ lương)";
      adviceText = "Bạn đã đi trễ $lateCount lần trong tháng! Nguy cơ bị trừ 10% phụ cấp đi lại. AI khuyên bạn nên đặt báo thức sớm hơn 20 phút.";
    } else if (lateCount >= 1) {
      warningLevel = "Cảnh báo Nhẹ";
      adviceText = "Bạn đã đi trễ $lateCount lần. Hãy chú ý thời gian di chuyển vào các giờ cao điểm để không vi phạm quy định.";
    }

    return {
      "punctualityRate": punctualityRate,
      "riskScore": riskScore,
      "totalCheckIns": totalCheckIns,
      "lateCount": lateCount,
      "warningLevel": warningLevel,
      "adviceText": adviceText,
      "nextShiftTime": "08:30 AM (Sáng mai)",
      "recommendedAlarm": "07:15 AM",
    };
  }
}

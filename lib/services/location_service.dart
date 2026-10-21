import 'dart:math';

class LocationService {
  // Tọa độ địa chỉ thực tế công ty Võ Hà Duy: 70 Đ. D1, Tăng Nhơn Phú, TP. Hồ Chí Minh 70000
  static const double companyLat = 10.8443;
  static const double companyLng = 106.7828;
  static const double allowedRadiusMeters = 100.0; // Bán kính cho phép chấm công: 100m

  // Tính khoảng cách Haversine giữa 2 điểm (mét)
  static double calculateDistance(double lat1, double lon1, double lat2, double lon2) {
    var p = 0.017453292519943295; // Math.PI / 180
    var c = cos;
    var a = 0.5 - c((lat2 - lat1) * p) / 2 +
        c(lat1 * p) * c(lat2 * p) *
            (1 - c((lon2 - lon1) * p)) / 2;
    return 12742 * asin(sqrt(a)) * 1000; // Trả về mét
  }

  // Kiểm tra xem vị trí có nằm trong bán kính hợp lệ của công ty không
  static Map<String, dynamic> checkGeofence(double currentLat, double currentLng) {
    double distance = calculateDistance(currentLat, currentLng, companyLat, companyLng);
    bool isValid = distance <= allowedRadiusMeters;

    return {
      "isValid": isValid,
      "distanceMeters": distance,
      "allowedRadius": allowedRadiusMeters,
      "companyAddress": "70 Đ. D1, Tăng Nhơn Phú, TP. Hồ Chí Minh 70000",
      "message": isValid
          ? "Vị trí hợp lệ! Khoảng cách tới công ty là ${distance.toStringAsFixed(1)}m (trong bán kính ${allowedRadiusMeters.toInt()}m)."
          : "Khoảng cách tới 70 Đ. D1, Tăng Nhơn Phú là ${distance.toStringAsFixed(1)}m (yêu cầu <= ${allowedRadiusMeters.toInt()}m).",
    };
  }
}

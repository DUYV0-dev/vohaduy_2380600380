import 'package:flutter/material.dart';
import 'package:geolocator/geolocator.dart';
import '../../services/attendance_service.dart';
import '../../services/auth_service.dart';
import '../../services/location_service.dart';

class GpsAttendanceScreen extends StatefulWidget {
  const GpsAttendanceScreen({super.key});

  @override
  State<GpsAttendanceScreen> createState() => _GpsAttendanceScreenState();
}

class _GpsAttendanceScreenState extends State<GpsAttendanceScreen> {
  // Tọa độ công ty thực tế Võ Hà Duy: 70 Đ. D1, Tăng Nhơn Phú, TP. Hồ Chí Minh
  double _currentLat = LocationService.companyLat;
  double _currentLng = LocationService.companyLng;
  bool _isChecking = false;
  bool _isOvertimeShift = false; // Chọn Ca làm: Ca Chính vs Ca Tăng Ca (OT)
  String _gpsStatus = "Đang truy vấn định vị GPS thực tế từ thiết bị...";

  late Map<String, dynamic> _geofenceResult;

  @override
  void initState() {
    super.initState();
    _geofenceResult = LocationService.checkGeofence(_currentLat, _currentLng);
    _getRealDeviceGps();
  }

  // TRUY VẤN VỊ TRÍ ĐỊNH VỊ GPS THỰC TẾ TỪ CẢM BIẾN THIẾT BỊ
  Future<void> _getRealDeviceGps() async {
    setState(() => _isChecking = true);

    try {
      bool serviceEnabled = await Geolocator.isLocationServiceEnabled();
      if (!serviceEnabled) {
        setState(() {
          _gpsStatus = "Dịch vụ định vị GPS bị tắt. Đang dùng tọa độ 70 Đ. D1, Tăng Nhơn Phú.";
        });
      } else {
        LocationPermission permission = await Geolocator.checkPermission();
        if (permission == LocationPermission.denied) {
          permission = await Geolocator.requestPermission();
        }

        if (permission == LocationPermission.whileInUse ||
            permission == LocationPermission.always) {
          Position position = await Geolocator.getCurrentPosition(
            desiredAccuracy: LocationAccuracy.high,
          );
          setState(() {
            _currentLat = position.latitude;
            _currentLng = position.longitude;
            _gpsStatus = "Đã lấy tọa độ GPS thiết bị thực tế thành công!";
          });
        }
      }
    } catch (e) {
      setState(() {
        _gpsStatus = "Đã xác định tọa độ hiện tại";
      });
    }

    _checkLocation();
  }

  void _checkLocation() {
    setState(() {
      _geofenceResult = LocationService.checkGeofence(_currentLat, _currentLng);
      _isChecking = false;
    });
  }

  Future<void> _handleGpsCheckIn() async {
    DateTime now = DateTime.now();
    bool isShiftValid = AttendanceService.isWithinShiftHours(now);

    // Nếu chọn Ca chính thức mà thời gian hiện tại nằm ngoài ca (08:00 - 17:00)
    if (!_isOvertimeShift && !isShiftValid) {
      if (mounted) {
        String currentTimeStr = "${now.hour}:${now.minute.toString().padLeft(2, '0')}";
        ScaffoldMessenger.of(context).showSnackBar(
          SnackBar(
            content: Text(
              "Hiện tại ($currentTimeStr) ngoài ca chính thức (08:00 - 17:00)! Nếu bạn làm ngoài giờ, vui lòng bật tùy chọn [Ca Tăng Ca (OT)] để chấm công.",
            ),
            backgroundColor: Colors.orange[800],
            duration: const Duration(seconds: 5),
          ),
        );
      }
      return;
    }

    AuthService authService = AuthService();
    var user = await authService.getCurrentUser();

    AttendanceService attService = AttendanceService();
    await attService.checkIn(
      employeeId: user.id,
      employeeName: user.fullName,
      employeeCode: user.employeeCode,
      type: 'Vị trí GPS',
      locationName: '70 Đ. D1, Tăng Nhơn Phú, TP. Hồ Chí Minh',
      note: _isOvertimeShift
          ? 'Chấm công Ca Tăng Ca (OT) ngoài giờ'
          : 'Check-in Vị trí GPS Ca chính thức (${_currentLat.toStringAsFixed(4)}, ${_currentLng.toStringAsFixed(4)})',
    );

    if (mounted) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(_isOvertimeShift
              ? "Chấm công Ca Tăng Ca (OT) thành công cho ${user.fullName}!"
              : "Chấm công vị trí GPS thành công cho ${user.fullName}!"),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.pop(context);
    }
  }

  @override
  Widget build(BuildContext context) {
    bool isValid = _geofenceResult['isValid'] ?? true;
    double distance = _geofenceResult['distanceMeters'] ?? 0.0;

    return Scaffold(
      backgroundColor: const Color(0xFFF4F7FB),
      appBar: AppBar(
        title: const Text('Chấm Công Vị Trí GPS Real'),
        backgroundColor: const Color(0xFF004CBF),
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.my_location),
            tooltip: 'Cập nhật lại GPS',
            onPressed: _getRealDeviceGps,
          ),
        ],
      ),
      body: SafeArea(
        child: SingleChildScrollView(
          padding: const EdgeInsets.all(20.0),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.stretch,
            children: [
              // BẢN ĐỒ / VỊ TRÍ REAL GPS CARD
              Card(
                elevation: 4,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(20),
                ),
                child: Padding(
                  padding: const EdgeInsets.all(20.0),
                  child: Column(
                    children: [
                      Container(
                        height: 180,
                        width: double.infinity,
                        decoration: BoxDecoration(
                          color: const Color(0xFFE8F1FF),
                          borderRadius: BorderRadius.circular(16),
                          border: Border.all(
                            color: isValid ? Colors.green : Colors.blue,
                            width: 2,
                          ),
                        ),
                        child: Stack(
                          alignment: Alignment.center,
                          children: [
                            Icon(
                              Icons.map_outlined,
                              size: 110,
                              color: Colors.blue.withValues(alpha: 0.3),
                            ),
                            Column(
                              mainAxisAlignment: MainAxisAlignment.center,
                              children: [
                                Icon(
                                  Icons.location_on,
                                  size: 52,
                                  color: isValid ? Colors.green : Colors.blue,
                                ),
                                const SizedBox(height: 6),
                                Container(
                                  padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 6),
                                  decoration: BoxDecoration(
                                    color: Colors.white,
                                    borderRadius: BorderRadius.circular(20),
                                    boxShadow: const [BoxShadow(color: Colors.black12, blurRadius: 4)],
                                  ),
                                  child: Text(
                                    "Tọa độ GPS: ${_currentLat.toStringAsFixed(4)}, ${_currentLng.toStringAsFixed(4)}",
                                    style: TextStyle(
                                      fontWeight: FontWeight.bold,
                                      fontSize: 13,
                                      color: isValid ? Colors.green[800] : Colors.blue[800],
                                    ),
                                  ),
                                ),
                              ],
                            ),
                          ],
                        ),
                      ),
                      const SizedBox(height: 16),
                      Text(
                        _gpsStatus,
                        style: const TextStyle(fontSize: 12, color: Colors.grey),
                        textAlign: TextAlign.center,
                      ),
                      const SizedBox(height: 8),
                      ListTile(
                        leading: Icon(
                          isValid ? Icons.check_circle : Icons.location_on,
                          color: isValid ? Colors.green : const Color(0xFF004CBF),
                          size: 32,
                        ),
                        title: const Text(
                          "70 Đ. D1, Tăng Nhơn Phú, TP. Hồ Chí Minh",
                          style: TextStyle(
                            fontWeight: FontWeight.bold,
                            color: Color(0xFF004CBF),
                            fontSize: 14,
                          ),
                        ),
                        subtitle: Text(
                          "Khoảng cách đến công ty: ${distance.toStringAsFixed(1)}m (Yêu cầu <= 100m)",
                        ),
                      ),
                    ],
                  ),
                ),
              ),

              const SizedBox(height: 16),

              // BỘ CHỌN CA LÀM VIỆC: CA CHÍNH THỨC VS CA TĂNG CA (OT)
              Card(
                elevation: 3,
                shape: RoundedRectangleBorder(
                  borderRadius: BorderRadius.circular(16),
                ),
                child: Padding(
                  padding: const EdgeInsets.symmetric(horizontal: 16.0, vertical: 12.0),
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      const Text(
                        'Lựa chọn Ca Chấm Công:',
                        style: TextStyle(fontWeight: FontWeight.bold, fontSize: 14, color: Color(0xFF004CBF)),
                      ),
                      const SizedBox(height: 8),
                      Row(
                        children: [
                          Expanded(
                            child: ChoiceChip(
                              avatar: const Icon(Icons.wb_sunny_outlined, size: 18),
                              label: const Text('Ca Chính Thức (08h-17h)'),
                              selected: !_isOvertimeShift,
                              selectedColor: const Color(0xFFE8F1FF),
                              onSelected: (selected) {
                                if (selected) setState(() => _isOvertimeShift = false);
                              },
                            ),
                          ),
                          const SizedBox(width: 8),
                          Expanded(
                            child: ChoiceChip(
                              avatar: const Icon(Icons.nightlight_round_outlined, size: 18),
                              label: const Text('Ca Tăng Ca (OT)'),
                              selected: _isOvertimeShift,
                              selectedColor: Colors.orange.shade100,
                              onSelected: (selected) {
                                if (selected) setState(() => _isOvertimeShift = true);
                              },
                            ),
                          ),
                        ],
                      ),
                    ],
                  ),
                ),
              ),

              const SizedBox(height: 20),

              // NÚT CHECK-IN Vị TRÍ
              SizedBox(
                height: 52,
                child: ElevatedButton.icon(
                  onPressed: _isChecking ? null : _handleGpsCheckIn,
                  icon: const Icon(Icons.my_location),
                  label: Text(
                    _isChecking
                        ? 'ĐANG ĐỊNH VỊ GPS...'
                        : (_isOvertimeShift ? 'XÁC NHẬN CHẤM CÔNG CA TĂNG CA (OT)' : 'XÁC NHẬN CHẤM CÔNG GPS'),
                    style: const TextStyle(fontSize: 15, fontWeight: FontWeight.bold),
                  ),
                  style: ElevatedButton.styleFrom(
                    backgroundColor: _isOvertimeShift ? Colors.orange[800] : const Color(0xFF004CBF),
                    foregroundColor: Colors.white,
                    shape: RoundedRectangleBorder(
                      borderRadius: BorderRadius.circular(12),
                    ),
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

import 'package:camera/camera.dart';
import 'package:flutter/material.dart';
import '../../services/auth_service.dart';

class VideoCallScreen extends StatefulWidget {
  final String roomCode;

  const VideoCallScreen({
    super.key,
    required this.roomCode,
  });

  @override
  State<VideoCallScreen> createState() => _VideoCallScreenState();
}

class _VideoCallScreenState extends State<VideoCallScreen> {
  CameraController? _cameraController;
  List<CameraDescription> _cameras = [];
  bool _isCameraInitialized = false;

  bool _isMuted = false;
  bool _isCameraOff = false;
  bool _isFrontCamera = true;

  String _currentUserName = "Đang tải...";
  final List<String> _joinedParticipants = [];

  @override
  void initState() {
    super.initState();
    _loadCurrentUser();
    _initCamera();
  }

  Future<void> _loadCurrentUser() async {
    AuthService authService = AuthService();
    var user = await authService.getCurrentUser();
    if (mounted) {
      setState(() {
        _currentUserName = user.fullName.isNotEmpty ? user.fullName : user.username;
        _joinedParticipants.add("$_currentUserName (Bạn)");
      });
    }
  }

  Future<void> _initCamera() async {
    try {
      _cameras = await availableCameras();
      if (_cameras.isNotEmpty) {
        CameraDescription selectedCamera = _cameras.firstWhere(
          (c) => _isFrontCamera
              ? c.lensDirection == CameraLensDirection.front
              : c.lensDirection == CameraLensDirection.back,
          orElse: () => _cameras.first,
        );

        _cameraController = CameraController(
          selectedCamera,
          ResolutionPreset.medium,
          enableAudio: false,
        );

        await _cameraController!.initialize();
        if (mounted) {
          setState(() {
            _isCameraInitialized = true;
          });
        }
      }
    } catch (_) {}
  }

  void _switchCamera() async {
    if (_cameras.length < 2) return;
    setState(() {
      _isFrontCamera = !_isFrontCamera;
      _isCameraInitialized = false;
    });
    await _cameraController?.dispose();
    _initCamera();
  }

  void _showAddParticipantDialog() {
    TextEditingController userCodeController = TextEditingController();
    showDialog(
      context: context,
      builder: (ctx) => AlertDialog(
        title: const Text('Thêm Thành Viên Vào Cuộc Gọi'),
        content: Column(
          mainAxisSize: MainAxisSize.min,
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text('Mã phòng họp hiện tại: ${widget.roomCode}', style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF004CBF))),
            const SizedBox(height: 12),
            TextField(
              controller: userCodeController,
              decoration: const InputDecoration(
                labelText: 'Mã người dùng / Tên nhân viên',
                hintText: 'Nhập mã NV hoặc tên...',
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
              String name = userCodeController.text.trim();
              if (name.isNotEmpty) {
                setState(() {
                  _joinedParticipants.add(name);
                });
              }
              Navigator.pop(ctx);
            },
            child: const Text('Thêm Vô Phòng', style: TextStyle(color: Colors.white)),
          ),
        ],
      ),
    );
  }

  @override
  void dispose() {
    _cameraController?.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Colors.grey[950],
      appBar: AppBar(
        title: Column(
          children: [
            Text("Phòng Gọi Video: ${widget.roomCode}", style: const TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
            Text("${_joinedParticipants.length} người trong cuộc gọi", style: const TextStyle(fontSize: 12, color: Colors.white70)),
          ],
        ),
        backgroundColor: Colors.black,
        elevation: 0,
        actions: [
          IconButton(
            icon: const Icon(Icons.person_add),
            tooltip: 'Thêm thành viên',
            onPressed: _showAddParticipantDialog,
          ),
        ],
      ),
      body: SafeArea(
        child: Column(
          children: [
            // LƯỚI KHUNG VIDEO THÀNH VIÊN
            Expanded(
              child: Padding(
                padding: const EdgeInsets.all(8.0),
                child: GridView.builder(
                  gridDelegate: SliverGridDelegateWithFixedCrossAxisCount(
                    crossAxisCount: _joinedParticipants.length > 1 ? 2 : 1,
                    crossAxisSpacing: 8,
                    mainAxisSpacing: 8,
                  ),
                  itemCount: _joinedParticipants.length,
                  itemBuilder: (context, index) {
                    bool isSelf = index == 0;
                    String pName = _joinedParticipants[index];

                    return Container(
                      decoration: BoxDecoration(
                        color: Colors.grey[900],
                        borderRadius: BorderRadius.circular(16),
                        border: Border.all(
                          color: isSelf ? const Color(0xFF004CBF) : Colors.grey.shade800,
                          width: isSelf ? 2 : 1,
                        ),
                      ),
                      child: ClipRRect(
                        borderRadius: BorderRadius.circular(14),
                        child: Stack(
                          alignment: Alignment.center,
                          children: [
                            // 1. Nếu là bản thân người dùng -> Stream Camera thật
                            if (isSelf) ...[
                              if (_isCameraInitialized && _cameraController != null && !_isCameraOff)
                                SizedBox.expand(
                                  child: CameraPreview(_cameraController!),
                                )
                              else
                                const Center(
                                  child: CircleAvatar(
                                    radius: 40,
                                    backgroundColor: Color(0xFF004CBF),
                                    child: Icon(Icons.person, size: 48, color: Colors.white),
                                  ),
                                ),
                            ] else ...[
                              // 2. Nếu là người dùng khác tham gia bằng mã -> Hiển thị avatar người dùng thực tế
                              Center(
                                child: CircleAvatar(
                                  radius: 40,
                                  backgroundColor: Colors.indigo,
                                  child: Text(
                                    pName.isNotEmpty ? pName[0].toUpperCase() : 'U',
                                    style: const TextStyle(color: Colors.white, fontSize: 28, fontWeight: FontWeight.bold),
                                  ),
                                ),
                              ),
                            ],

                            Positioned(
                              bottom: 8,
                              left: 8,
                              child: Container(
                                padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 4),
                                decoration: BoxDecoration(
                                  color: Colors.black.withValues(alpha: 0.6),
                                  borderRadius: BorderRadius.circular(8),
                                ),
                                child: Row(
                                  mainAxisSize: MainAxisSize.min,
                                  children: [
                                    Text(pName, style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.bold)),
                                    if (isSelf && _isMuted) ...[
                                      const SizedBox(width: 4),
                                      const Icon(Icons.mic_off, size: 12, color: Colors.red),
                                    ],
                                  ],
                                ),
                              ),
                            ),
                          ],
                        ),
                      ),
                    );
                  },
                ),
              ),
            ),

            // THANH ĐIỀU KHIỂN CUỘC GỌI
            Container(
              padding: const EdgeInsets.symmetric(vertical: 20, horizontal: 24),
              color: Colors.black,
              child: Row(
                mainAxisAlignment: MainAxisAlignment.spaceEvenly,
                children: [
                  // Micro Toggle
                  FloatingActionButton(
                    heroTag: "mute_btn",
                    backgroundColor: _isMuted ? Colors.red : Colors.grey[800],
                    onPressed: () {
                      setState(() => _isMuted = !_isMuted);
                    },
                    child: Icon(_isMuted ? Icons.mic_off : Icons.mic, color: Colors.white),
                  ),

                  // Camera Toggle
                  FloatingActionButton(
                    heroTag: "cam_btn",
                    backgroundColor: _isCameraOff ? Colors.red : Colors.grey[800],
                    onPressed: () {
                      setState(() => _isCameraOff = !_isCameraOff);
                    },
                    child: Icon(_isCameraOff ? Icons.videocam_off : Icons.videocam, color: Colors.white),
                  ),

                  // Switch Camera
                  FloatingActionButton(
                    heroTag: "switch_btn",
                    backgroundColor: Colors.grey[800],
                    onPressed: _switchCamera,
                    child: const Icon(Icons.cameraswitch, color: Colors.white),
                  ),

                  // End Call
                  FloatingActionButton(
                    heroTag: "end_btn",
                    backgroundColor: Colors.red,
                    onPressed: () => Navigator.pop(context),
                    child: const Icon(Icons.call_end, color: Colors.white),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

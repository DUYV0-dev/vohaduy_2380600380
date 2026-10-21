import 'package:flutter_dotenv/flutter_dotenv.dart';
import 'package:shared_preferences/shared_preferences.dart';

class Config_URL {
  static String _customBaseUrl = "";

  static Future<void> init() async {
    SharedPreferences prefs = await SharedPreferences.getInstance();
    _customBaseUrl = prefs.getString('custom_api_url') ?? '';
  }

  static String get baseUrl {
    if (_customBaseUrl.isNotEmpty) {
      if (_customBaseUrl.startsWith('https://vohaduyapi.somee.com')) {
        return _customBaseUrl.replaceFirst('https://', 'http://');
      }
      return _customBaseUrl;
    }
    final url = dotenv.env['BASE_URL'];
    if (url != null && url.isNotEmpty) {
      if (url.startsWith('https://vohaduyapi.somee.com')) {
        return url.replaceFirst('https://', 'http://');
      }
      return url;
    }
    // Web API Hosted Online 24/7 trên Somee (Võ Hà Duy)
    return "http://vohaduyapi.somee.com/api/";
  }

  static Future<void> setBaseUrl(String newUrl) async {
    String formatted = newUrl.trim();
    if (formatted.startsWith('https://vohaduyapi.somee.com')) {
      formatted = formatted.replaceFirst('https://', 'http://');
    }
    if (formatted.isNotEmpty && !formatted.endsWith('/')) {
      formatted = "$formatted/";
    }
    _customBaseUrl = formatted;
    SharedPreferences prefs = await SharedPreferences.getInstance();
    await prefs.setString('custom_api_url', formatted);
  }
}

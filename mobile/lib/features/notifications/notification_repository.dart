import '../../core/api/api_client.dart';
import '../../core/api/api_envelope.dart';
import 'notification_models.dart';

class NotificationRepository {
  NotificationRepository(this._api);

  final ApiClient _api;

  Future<NotificationFeed> getMine({int take = 20}) async {
    final result = await _api.get(
      '/api/Notifications/mine',
      query: {'take': take},
    );
    if (result is! Map) {
      throw const ApiException('Unexpected notifications response.');
    }
    return NotificationFeed.fromJson(Map<String, dynamic>.from(result));
  }

  /// Marks all of the caller's notifications read; returns how many changed.
  Future<int> markAllRead() async {
    final result = await _api.post('/api/Notifications/mark-all-read');
    return (result as num?)?.toInt() ?? 0;
  }
}

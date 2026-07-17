import 'package:flutter_test/flutter_test.dart';
import 'package:hr_app/core/api/api_envelope.dart';

/// The envelope parser is the load-bearing piece: `result` is untyped on the server and
/// the API reports failure four different ways. These pin each shape.
void main() {
  group('parseEnvelope', () {
    test('2xx with isSuccess:true exposes result', () {
      final e = parseEnvelope(200, {
        'result': {'token': 'abc'},
        'isSuccess': true,
        'message': '',
      });
      expect(e.isSuccess, isTrue);
      expect((e.result as Map)['token'], 'abc');
    });

    test('200 with isSuccess:false is a failure, not a success', () {
      // Most controllers catch, set IsSuccess=false, and still return 200.
      final e = parseEnvelope(200, {
        'result': null,
        'isSuccess': false,
        'message': 'Username or password is incorrect',
      });
      expect(e.isSuccess, isFalse);
      expect(e.message, 'Username or password is incorrect');
    });

    test('envelope arriving as a JSON string is decoded', () {
      final e = parseEnvelope(
        400,
        '{"isSuccess":false,"message":"bad","result":null}',
      );
      expect(e.isSuccess, isFalse);
      expect(e.message, 'bad');
    });

    test('non-2xx bare string body becomes the message', () {
      // AuthController.Register returns BadRequest(ex.Message) — no envelope.
      final e = parseEnvelope(400, 'Email already registered');
      expect(e.isSuccess, isFalse);
      expect(e.message, 'Email already registered');
    });

    test(
      '2xx bare value (e.g. raw bool from PaySalary) is a success carrying result',
      () {
        final e = parseEnvelope(200, true);
        expect(e.isSuccess, isTrue);
        expect(e.result, true);
      },
    );

    test('non-2xx with no usable body falls back to a status message', () {
      final e = parseEnvelope(500, null);
      expect(e.isSuccess, isFalse);
      expect(e.message, contains('500'));
    });
  });
}

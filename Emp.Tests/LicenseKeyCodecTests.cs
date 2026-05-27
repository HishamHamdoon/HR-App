using System;
using System.Text;
using Emp.Models.Licensing;
using Xunit;

namespace Emp.Tests
{
    public class LicenseKeyCodecTests
    {
        private static readonly byte[] Secret = Encoding.UTF8.GetBytes("unit-test-secret-0123456789");

        [Fact]
        public void Generate_Then_Verify_RoundTrips()
        {
            var expiry = new DateTime(2027, 5, 26);
            var key = LicenseKeyCodec.Generate("Yearly", expiry, Secret);

            var ok = LicenseKeyCodec.TryVerify(key, Secret, out var type, out var parsedExpiry);

            Assert.True(ok);
            Assert.Equal("Yearly", type);
            Assert.Equal(expiry.Date, parsedExpiry.Date);
        }

        [Fact]
        public void Verify_Fails_For_Tampered_Payload()
        {
            var key = LicenseKeyCodec.Generate("Yearly", new DateTime(2027, 1, 1), Secret);
            // Flip the payload but keep the original signature.
            var parts = key.Split('.');
            var forged = LicenseKeyCodec.Generate("Yearly", new DateTime(2099, 1, 1), Secret).Split('.')[0]
                         + "." + parts[1];

            Assert.False(LicenseKeyCodec.TryVerify(forged, Secret, out _, out _));
        }

        [Fact]
        public void Verify_Fails_With_Different_Secret()
        {
            var key = LicenseKeyCodec.Generate("Yearly", new DateTime(2027, 1, 1), Secret);
            var otherSecret = Encoding.UTF8.GetBytes("a-completely-different-secret");

            Assert.False(LicenseKeyCodec.TryVerify(key, otherSecret, out _, out _));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-key")]
        [InlineData("only.one")]
        [InlineData("a.b.c")]
        public void Verify_Fails_For_Malformed_Keys(string key)
        {
            Assert.False(LicenseKeyCodec.TryVerify(key, Secret, out _, out _));
        }
    }
}

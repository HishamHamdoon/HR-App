using Emp.Api.Dtos;
using Xunit;

namespace Emp.Tests
{
    public class ResponseDtoTests
    {
        [Fact]
        public void Defaults_Are_Success_With_Empty_Message()
        {
            var dto = new ResponseDto();
            Assert.True(dto.IsSuccess);
            Assert.Equal(string.Empty, dto.Message);
            Assert.Null(dto.Result);
        }

        [Fact]
        public void Can_Carry_A_Result_Payload()
        {
            var dto = new ResponseDto { Result = new { Id = 7 }, IsSuccess = false, Message = "nope" };
            Assert.NotNull(dto.Result);
            Assert.False(dto.IsSuccess);
            Assert.Equal("nope", dto.Message);
        }
    }
}

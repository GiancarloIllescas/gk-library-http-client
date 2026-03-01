using System.Net;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace GK.Library.Http.Client.Test;

public class BasicApiCustomErrorTest : IntegrationTestBase
{
    public BasicApiCustomErrorTest(CustomWebApplicationFactory<Program> factory) : base(factory)
    {
    }

    [Fact]
    public async Task Put_ShouldFail_HandlerExceptionWithErrorMapper()
    {
        // Arrange
        _mockServer
            .Given(Request.Create().WithPath("/basic-api/data").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.InternalServerError));

        // Act
        var response = await _basicApiCustomErrorService.GetDataAsync();

        // Assertions
        Assert.Null(response);
    }





}
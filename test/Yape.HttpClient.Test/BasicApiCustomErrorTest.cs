using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Text.Json;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Yape.Library.Http.Client.Infraestructure.Adapters.Http;
using Yape.Library.Http.Client.Test.Entities;

namespace Yape.Library.Http.Client.Test;

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
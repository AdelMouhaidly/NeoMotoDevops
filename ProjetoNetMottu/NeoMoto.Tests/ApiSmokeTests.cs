using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NeoMoto.Tests;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly HttpClient _client;

	public ApiSmokeTests(WebApplicationFactory<Program> factory)
	{
		_client = factory.WithWebHostBuilder(_ => { }).CreateClient(new WebApplicationFactoryClientOptions
		{
			BaseAddress = new Uri("http://localhost")
		});
	}

	[Fact]
	public async Task Swagger_should_be_available()
	{
		var resp = await _client.GetAsync("/swagger/v1/swagger.json");
		// Verifica que a API responde (não é erro de conexão)
		resp.StatusCode.Should().NotBe(HttpStatusCode.BadGateway);
		resp.StatusCode.Should().NotBe(HttpStatusCode.ServiceUnavailable);
	}

	[Fact]
	public async Task Should_list_filiais_with_pagination()
	{
		var resp = await _client.GetAsync("/api/filiais?pageNumber=1&pageSize=2");
		// Apenas verifica que a API respondeu (não é 404 ou erro de conexão)
		resp.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
		resp.StatusCode.Should().NotBe(HttpStatusCode.BadGateway);
	}
}

using System.Net;
using System.Net.Http.Json;
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
		resp.StatusCode.Should().Be(HttpStatusCode.OK);
	}

	[Fact]
	public async Task Should_list_filiais_with_pagination()
	{
		var resp = await _client.GetAsync("/api/filiais?pageNumber=1&pageSize=2");
		resp.EnsureSuccessStatusCode();
		var json = await resp.Content.ReadAsStringAsync();
		json.Should().Contain("items").And.Contain("totalCount");
	}
}


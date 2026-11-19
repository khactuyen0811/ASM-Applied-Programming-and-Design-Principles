using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SIMS.Tests.Integration;

[Collection("SimsIntegration")]
public sealed class SimsIntegrationTests
{
    private readonly SimsWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public SimsIntegrationTests(SimsWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private HttpClient NewClient() => _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    private async Task<HttpResponseMessage> LoginAsync(HttpClient client, string username, string password)
    {
        var page = await client.GetAsync("/Login");
        var html = await page.Content.ReadAsStringAsync();
        var token = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value;

        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Username"] = username,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        });

        return await client.PostAsync("/Login", form);
    }

    private static string LocationPath(HttpResponseMessage response)
    {
        var location = response.Headers.Location!;
        return location.IsAbsoluteUri ? location.PathAndQuery : location.OriginalString;
    }

    [Fact]
    public async Task GetLogin_ReturnsLoginPage()
    {
        var response = await _client.GetAsync("/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Sign In to SIMS", body);
    }

    [Fact]
    public async Task GetDashboard_Unauthenticated_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/Dashboard");

        Assert.True(
            response.StatusCode == HttpStatusCode.Redirect || response.StatusCode == HttpStatusCode.Unauthorized,
            $"Expected redirect or unauthorized, got {(int)response.StatusCode}.");
        if (response.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.NotNull(response.Headers.Location);
            Assert.StartsWith("/Login", LocationPath(response));
        }
    }

    [Fact]
    public async Task PostLogin_ValidAdminCredentials_RedirectsToDashboard_AndDashboardIsAccessible()
    {
        var loginResponse = await LoginAsync(_client, "trieunt", "12345");

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.NotNull(loginResponse.Headers.Location);
        Assert.StartsWith("/Dashboard", LocationPath(loginResponse));

        var dashboard = await _client.GetAsync("/Dashboard");
        Assert.Equal(HttpStatusCode.OK, dashboard.StatusCode);
    }

    [Fact]
    public async Task PostLogin_WrongPassword_ShowsErrorMessage()
    {
        var response = await LoginAsync(_client, "trieunt", "wrong-password");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Your account invalid", body);
    }

    [Fact]
    public async Task StudentUser_CannotAccessTeacher_ButCanAccessStudentIndex()
    {
        var loginResponse = await LoginAsync(_client, "student", "12345");
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        var teacher = await _client.GetAsync("/Teacher");
        Assert.True(
            teacher.StatusCode == HttpStatusCode.Forbidden || teacher.StatusCode == HttpStatusCode.Redirect,
            $"Expected forbidden or redirect, got {(int)teacher.StatusCode}.");
        if (teacher.StatusCode == HttpStatusCode.Redirect)
        {
            Assert.NotNull(teacher.Headers.Location);
            Assert.StartsWith("/Home/Denied", LocationPath(teacher));
        }

        var student = await _client.GetAsync("/Student");
        Assert.Equal(HttpStatusCode.OK, student.StatusCode);
    }

    [Fact]
    public async Task AdminUser_DashboardStats_ReturnsSeededTotals()
    {
        var loginResponse = await LoginAsync(_client, "trieunt", "12345");
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        var response = await _client.GetAsync("/Dashboard/Stats");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"totalstudents\":10000", body.ToLowerInvariant());
        Assert.Contains("\"totalcourses\":60", body.ToLowerInvariant());
    }

    [Fact]
    public async Task AdminUser_StudentIndex_ReturnsStudentPage()
    {
        var loginResponse = await LoginAsync(_client, "trieunt", "12345");
        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);

        var response = await _client.GetAsync("/Student");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Student Directory", body);
    }
}

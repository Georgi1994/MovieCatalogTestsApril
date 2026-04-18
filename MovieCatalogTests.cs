using System.Net;
using MovieCatalogApiTests.Dtos;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;

namespace MovieCatalogApiTests.Tests;

[TestFixture]
public class MovieCatalogTests
{
    private RestClient client = null!;

    private const string BaseUrl = "http://144.91.123.158:5000/api";
    private const string Email = "georgi94@abv.bg";
    private const string Password = "123321";

    private static string createdMovieId = string.Empty;

    [OneTimeSetUp]
    public void Setup()
    {
        client = new RestClient("http://144.91.123.158:5000");

        var loginRequest = new RestRequest("/api/User/Authentication", Method.Post);

        loginRequest.AddHeader("Content-Type", "application/json");

        loginRequest.AddStringBody($@"{{
        ""email"": ""{Email}"",
        ""password"": ""{Password}""
    }}", DataFormat.Json);

        var loginResponse = client.Execute(loginRequest);

        TestContext.WriteLine($"STATUS: {loginResponse.StatusCode}");
        TestContext.WriteLine($"CONTENT: {loginResponse.Content}");

        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var loginData = System.Text.Json.JsonSerializer.Deserialize<LoginResponseDto>(
            loginResponse.Content!,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        Assert.That(loginData, Is.Not.Null);
        Assert.That(loginData!.AccessToken, Is.Not.Null.And.Not.Empty);

        client = new RestClient(new RestClientOptions("http://144.91.123.158:5000/api")
        {
            Authenticator = new JwtAuthenticator(loginData.AccessToken)
        });
    }

    [Test, Order(1)]
    public void CreateMovie_WithRequiredFields_ShouldSucceed()
    {
        var request = new RestRequest("/Movie/Create", Method.Post);

        request.AddJsonBody(new
        {
            title = "Exam Movie",
            description = "Created via RestSharp tests",
            posterUrl = "",
            trailerLink = "",
            isWatched = false
        });

        var response = client.Execute<ApiResponseDto>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!.Movie, Is.Not.Null);
        Assert.That(response.Data.Movie.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(response.Data.Msg, Is.EqualTo("Movie created successfully!"));

        createdMovieId = response.Data.Movie.Id;
    }

    [Test, Order(2)]
    public void EditCreatedMovie_ShouldSucceed()
    {
        var request = new RestRequest("/Movie/Edit", Method.Put);
        request.AddQueryParameter("movieId", createdMovieId);

        request.AddJsonBody(new
        {
            title = "Edited Exam Movie",
            description = "Edited via RestSharp tests",
            posterUrl = "",
            trailerLink = "",
            isWatched = true
        });

        var response = client.Execute<ApiResponseDto>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!.Msg, Is.EqualTo("Movie edited successfully!"));
    }

    [Test, Order(3)]
    public void GetAllMovies_ShouldReturnNonEmptyArray()
    {
        var request = new RestRequest("/Catalog/All", Method.Get);

        var response = client.Execute<List<MovieDto>>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!, Is.Not.Empty);
    }

    [Test, Order(4)]
    public void DeleteCreatedMovie_ShouldSucceed()
    {
        var request = new RestRequest("/Movie/Delete", Method.Delete);
        request.AddQueryParameter("movieId", createdMovieId);

        var response = client.Execute<ApiResponseDto>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!.Msg, Is.EqualTo("Movie deleted successfully!"));
    }

    [Test, Order(5)]
    public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
    {
        var request = new RestRequest("/Movie/Create", Method.Post);

        request.AddJsonBody(new
        {
            title = "",
            description = "",
            posterUrl = "",
            trailerLink = "",
            isWatched = false
        });

        var response = client.Execute(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test, Order(6)]
    public void EditNonExistingMovie_ShouldReturnBadRequest()
    {
        var request = new RestRequest("/Movie/Edit", Method.Put);
        request.AddQueryParameter("movieId", "non-existing-id");

        request.AddJsonBody(new
        {
            title = "Invalid Edit",
            description = "This should fail",
            posterUrl = "",
            trailerLink = "",
            isWatched = false
        });

        var response = client.Execute<ApiResponseDto>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!.Msg,
            Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
    }

    [Test, Order(7)]
    public void DeleteNonExistingMovie_ShouldReturnBadRequest()
    {
        var request = new RestRequest("/Movie/Delete", Method.Delete);
        request.AddQueryParameter("movieId", "non-existing-id");

        var response = client.Execute<ApiResponseDto>(request);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(response.Data, Is.Not.Null);
        Assert.That(response.Data!.Msg,
            Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
    }
}

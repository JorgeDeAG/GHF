using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using WebAPI;
using WebAPI.Controllers;
using Xunit;


/*using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;*/


namespace UnitTests
{
    public class GitHubUser2FreshDeskControllerTests
    {
        private string stringTestGitHubUserName()
        {
            return "octocat";
        }
        private int intTestGitHubUserID()
        {
            return 583231;
        }
        private string stringTestFreshDeskSubdomain()
        {
            return "deaguinaga";
        }
        private Dictionary<string, HttpResponseMessage> responseMessage = new Dictionary<string, HttpResponseMessage>
        {
            {
                "freshDeskResponseDuplicateValue", new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Conflict,
                    Content = new StringContent("{\"description\":\"Validation failed\",\"errors\":[{\"field\":\"email\",\"additional_info\":{\"user_id\":202001218298},\"message\":\"It should be a unique value\",\"code\":\"duplicate_value\"}]}",
                    Encoding.UTF8, "application/json")
                }
            },
            {
                "freshDeskResponseUpdatedUser", new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"active\":true,\"address\":\"Portland, OR\",\"deleted\":false,\"description\":null,\"email\":\"torvalds@github.com\",\"id\":202001218298,\"job_title\":\"GitHub User (20231022 09:37)\",\"language\":\"es\",\"mobile\":null,\"name\":\"torvalds\",\"phone\":null,\"time_zone\":\"Eastern Time (US & Canada)\",\"twitter_id\":null,\"custom_fields\":{},\"tags\":[],\"other_emails\":[],\"facebook_id\":null,\"created_at\":\"2023-10-22T06:49:00Z\",\"updated_at\":\"2023-10-22T07:37:35Z\",\"csat_rating\":null,\"preferred_source\":null,\"company_id\":null,\"view_all_tickets\":null,\"other_companies\":[],\"unique_external_id\":\"1024025\",\"avatar\":null,\"first_name\":\"torvalds\",\"last_name\":\"\",\"visitor_id\":\"eac435d9-6933-41ca-ad64-f4c8ecd0422d\",\"org_contact_id\":1715983576342937600,\"other_phone_numbers\":[]}",
                    Encoding.UTF8, "application/json")
                }
            },
            {
                "freshDeskResponseNewUser", new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"active\": true,\"address\": \"Portland, OR\",\"view_all_tickets\": false,\"email\": \"torvalds@github.com\",\"job_title\": \"GitHub User (20231022 08:48)\",\"name\": \"torvalds\",\"unique_external_id\": \"1024025\"}",
                    Encoding.UTF8, "application/json")
                }
            },
            {
                "gitHubUser", new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonTestGitHubUser(), Encoding.UTF8, "application/json")
                }
            }
        };
        static string jsonTestGitHubUser()
        {
            return "{\"login\":\"octocat\",\"id\":583231,\"node_id\":\"MDQ6VXNlcjU4MzIzMQ==\",\"avatar_url\":\"https://avatars.githubusercontent.com/u/583231?v=4\",\"gravatar_id\":\"\",\"url\":\"https://api.github.com/users/octocat\",\"html_url\":\"https://github.com/octocat\",\"followers_url\":\"https://api.github.com/users/octocat/followers\",\"following_url\":\"https://api.github.com/users/octocat/following{/other_user}\",\"gists_url\":\"https://api.github.com/users/octocat/gists{/gist_id}\",\"starred_url\":\"https://api.github.com/users/octocat/starred{/owner}{/repo}\",\"subscriptions_url\":\"https://api.github.com/users/octocat/subscriptions\",\"organizations_url\":\"https://api.github.com/users/octocat/orgs\",\"repos_url\":\"https://api.github.com/users/octocat/repos\",\"events_url\":\"https://api.github.com/users/octocat/events{/privacy}\",\"received_events_url\":\"https://api.github.com/users/octocat/received_events\",\"type\":\"User\",\"site_admin\":false,\"name\":\"The Octocat\",\"company\":\"@github\",\"blog\":\"https://github.blog\",\"location\":\"San Francisco\",\"email\":\"octocat@github.com\",\"hireable\":null,\"bio\":null,\"twitter_username\":null,\"public_repos\":8,\"public_gists\":8,\"followers\":10796,\"following\":9,\"created_at\":\"2011-01-25T18:44:36Z\",\"updated_at\":\"2023-09-22T11:25:21Z\"}";
        }
        [Fact]
        public async Task GetGitHubUser_returns_GitHubUser()
        {
            var logger = new Mock<ILogger<GitHubUser2FreshDeskController>>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpClientFactory = new Mock<IHttpClientFactory>();

            var gitHubUserMessage = responseMessage["gitHubUser"];
            //Console.WriteLine("JWT");
            //Console.WriteLine(GenerateJwtToken());
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(gitHubUserMessage);

            var httpClient = new HttpClient(mockHandler.Object);
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new GitHubUser2FreshDeskController(logger.Object, httpClientFactory.Object);

            GitHubUser user = await controller.GetGitHubUser(stringTestGitHubUserName());

            Assert.IsType<GitHubUser>(user);
            Assert.Equal(stringTestGitHubUserName(), user.login);
            Assert.Equal(intTestGitHubUserID(), user.id);

        }


        [Fact]
        public async Task freshDesk_createInexistentUser_returns_OK_New()
        {
            var logger = new Mock<ILogger<GitHubUser2FreshDeskController>>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var freshDesdkResponseNewUserMessage = responseMessage["freshDeskResponseNewUser"];
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(freshDesdkResponseNewUserMessage);

            var httpClient = new HttpClient(mockHandler.Object);
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new GitHubUser2FreshDeskController(logger.Object, httpClientFactory.Object);

            GitHubUser user = JsonSerializer.Deserialize<GitHubUser>(jsonTestGitHubUser())!;
            String returnValue = await controller.setFreshDeskUser(stringTestFreshDeskSubdomain(), user);

            Assert.Equal("OK_New", returnValue);
        }

        [Fact]
        public async Task freshDesk_updateExistentUser_returns_OK_Update()
        {
            var logger = new Mock<ILogger<GitHubUser2FreshDeskController>>();
            var mockHandler = new Mock<HttpMessageHandler>();
            var httpClientFactory = new Mock<IHttpClientFactory>();
            var freshDesdkResponseDuplicateValueMessage = responseMessage["freshDeskResponseDuplicateValue"];
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(freshDesdkResponseDuplicateValueMessage);

            var freshDesdkResponseUpdatedUserMessage = responseMessage["freshDeskResponseUpdatedUser"];
            mockHandler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Put
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(freshDesdkResponseUpdatedUserMessage);

            var httpClient = new HttpClient(mockHandler.Object);
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new GitHubUser2FreshDeskController(logger.Object, httpClientFactory.Object);

            GitHubUser user = JsonSerializer.Deserialize<GitHubUser>(jsonTestGitHubUser())!;
            String returnValue = await controller.setFreshDeskUser(stringTestFreshDeskSubdomain(), user);

            Assert.Equal("OK_Update", returnValue);
        }

        [Fact]
        public async Task main_functionality_OK_New()
        {
            var logger = new Mock<ILogger<GitHubUser2FreshDeskController>>();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage["gitHubUser"]);
            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage["freshDeskResponseNewUser"]);
            mockHandler.Verify();

            var httpClient = new HttpClient(mockHandler.Object, false);
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new GitHubUser2FreshDeskController(logger.Object, httpClientFactory.Object);
            string returnValue=await controller.GitHubUser2FreshDesk(stringTestGitHubUserName(),stringTestFreshDeskSubdomain());

            Assert.Equal("OK_New", returnValue);
        }

        [Fact]
        public async Task main_functionality_OK_Update()
        {
            var logger = new Mock<ILogger<GitHubUser2FreshDeskController>>();
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            var httpClientFactory = new Mock<IHttpClientFactory>();
            mockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage["gitHubUser"]);
            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Post
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage["freshDeskResponseDuplicateValue"]);
            mockHandler.Protected().SetupSequence<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(request =>
                request.Method == HttpMethod.Put
                ), ItExpr.IsAny<CancellationToken>()).ReturnsAsync(responseMessage["freshDeskResponseUpdatedUser"]);
            mockHandler.Verify();

            var httpClient = new HttpClient(mockHandler.Object, false);
            httpClientFactory.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var controller = new GitHubUser2FreshDeskController(logger.Object, httpClientFactory.Object);
            string returnValue=await controller.GitHubUser2FreshDesk(stringTestGitHubUserName(),stringTestFreshDeskSubdomain());

            Assert.Equal("OK_Update", returnValue);
        }


        /*public string GenerateJwtToken()
        {

            var keyBytes = new byte[32];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(keyBytes);
            }

            string base64Key = Convert.ToBase64String(keyBytes);


            //var secretKey = base64Key;
            var secretKey = "MB//YoCUlrXkW0rbAgeXv7Pru/bPS+Q9LzBBPd2xkrY=";
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, "taskUser"),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            var token = new JwtSecurityToken(
                issuer: "deaguinaga",
                audience: "SIT",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1080),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );
            string tkn= new JwtSecurityTokenHandler().WriteToken(token);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }*/
    }
}
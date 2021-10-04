using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PasswordHashTests
{
    [TestClass]
    public class PasswordHashTests
    {
        //ToDo: embed the exe in the project instead of reading from file path
        [TestInitialize]
        public void Setup()
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\test\JumpCloudAssignment\broken-hashserve_win.exe";
            using (Process proc = Process.Start(start)) { };
        }

        [TestCleanup]
        public async Task TearDownAsync()
        {
            await StopServer();
        }

        /// <summary>
        /// Test is "happy path" post and should recieve 200/ok response
        /// </summary>
        [TestMethod]
        public async Task PostToHashEndpoint_UsingHttp_IsSuccessfulAsync()
        {
            var post1 = await UploadToHashEndpoint("tacojustice");
            Assert.AreEqual(HttpStatusCode.OK, post1);
        }

        /// <summary>
        /// Test POSTs several times and makes sure the correct hash is returned
        /// ToDo:Are there performance requirements > should Jmeter test(s) be employed for that?
        /// </summary>
        [TestMethod]
        public async Task MultiPostToHashEndpoint_UsingHttp_IsSuccessful()
        {
            await UploadToHashEndpointAsync("tacojustice");
            await UploadToHashEndpointAsync("potatodream");
            await UploadToHashEndpointAsync("lemonglass");

            string expectedHash1 = "OjYlLNz1JaMJFJwdDuKPFcHjB865KQuUGOO86u3oauCH+rSoV54DpX0PtO0Ve2kik+OHPlfTEtww2NKaQVN9zg=="; //tacojustice hash
            string expectedHash2 = "yBsDBsMk38EvY+FMGtRFsJWEnBho6+r1tmMWchZW3s4H1V5TJTkBQSzGDsODsbosNE/4+IqG3imlW1Mq9UnpCw=="; //potatodream hash
            string expectedHash3 = "4JAQxTpFK0R+Yf14qYj9zGMrJWUMWmLu+4ghx9XNdBFgNwlW21+rroDz0gaMB+MTZT2JbguU/5NSBSOfbTxtMQ=="; //lemonglass hash

            var getHash1 = await ReturnPasswordHashAsync(1);
            var getHash2 = await ReturnPasswordHashAsync(2);
            var getHash3 = await ReturnPasswordHashAsync(3);

            Assert.AreEqual(expectedHash1, getHash1);
            Assert.AreEqual(expectedHash2, getHash2);
            Assert.AreEqual(expectedHash3, getHash3);
        }

        /// <summary>
        /// Starts a shutdown of the server while making a request
        /// </summary>
        [TestMethod]
        public async Task HashRequestProcessed_DuringShutdown_IsSuccessful()
        {
            var upload1 = await UploadToHashEndpoint("tacojustice");
            var upload2 = await UploadToHashEndpoint("potatodream");
            Assert.AreEqual(HttpStatusCode.OK, upload1);
            Assert.AreEqual(HttpStatusCode.OK, upload2);
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://127.0.0.1:8088/hash"))
                {
                    request.Content = new StringContent("shutdown");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                }
            }
            var upload3 = await UploadToHashEndpoint("lemonglass");
            Assert.AreEqual(HttpStatusCode.OK, upload3);
        }

        /// <summary>
        /// Helper method to make POST request to hash endpoint and wait for response
        /// </summary>
        private async Task<HttpStatusCode> UploadToHashEndpoint(string password)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://127.0.0.1:8088/hash"))
                {
                    request.Content = new StringContent($"{{\"password\":\"{password}\"}}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);

                    return response.StatusCode;
                }
            }
        }

        /// <summary>
        /// Return hashed password
        /// </summary>
        /// <param name="jobId"></param>
        private async Task<string> ReturnPasswordHashAsync(int jobId)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"http://127.0.0.1:8088/hash/{jobId}"))
                {
                    var response = await httpClient.SendAsync(request);
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        /// <summary>
        /// Helper method to make POST request to hash endpoint
        /// ToDo: Expand to allow the "password" json to also be passed in > will depend on expected behavior
        /// </summary>
        private async Task UploadToHashEndpointAsync(string password)
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://127.0.0.1:8088/hash"))
                {
                    request.Content = new StringContent($"{{\"password\":\"{password}\"}}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");
                    var response = await httpClient.SendAsync(request);
                }
            }
        }

        /// <summary>
        /// StopServer
        /// </summary>
        /// <returns></returns>
        public async Task StopServer()
        {
            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), "http://127.0.0.1:8088/hash"))
                {
                    request.Content = new StringContent("shutdown");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await httpClient.SendAsync(request);
                }
            }
        }
    }
}
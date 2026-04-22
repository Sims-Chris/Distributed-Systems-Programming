using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DistSysAcwClient
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        private static string currentUsername = "";
        private static string currentApiKey = "";
        private static string _storedServerPublicKeyXML = null;

        // Ensure this ends with a forward slash for easy concatenation
        //private const string BaseUrl = "http://distsysacwserver.net.dcs.hull.ac.uk/4710625/Api/";
        private const string BaseUrl = "http://localhost:53415/api/";

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello. What would you like to do?");

            while (true)
            {
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input)) continue;
                if (input.Equals("Exit", StringComparison.OrdinalIgnoreCase)) break;

                Console.Clear();
                await ProcessCommand(input);

                Console.WriteLine("\nWhat would you like to do next?");
            }
        }

        private static async Task ProcessCommand(string input)
        {
            try
            {
                // We create a lowercase version for command checking
                string lowerInput = input.ToLower();

                // 1. TalkBack Hello
                if (lowerInput.StartsWith("talkback hello"))
                {
                    // Use the original 'input' for the message to preserve casing (e.g. "Hello" vs "hello")
                    string message = input.Substring(14).Trim();
                    await GetRequest($"talkback/hello?message={Uri.EscapeDataString(message)}");
                }
                // 2. TalkBack Sort
                else if (lowerInput.StartsWith("talkback sort"))
                {
                    string arrayPart = input.Substring(13).Trim().Trim('[', ']');
                    string query = string.Join("&", arrayPart.Split(',').Select(i => $"integers={i.Trim()}"));
                    await GetRequest($"talkback/sort?{query}");
                }
                // 3. User Get
                else if (lowerInput.StartsWith("user get"))
                {
                    string name = input.Substring(8).Trim();
                    await GetRequest($"user/new?username={name}");
                }
                // 4. User Post
                else if (lowerInput.StartsWith("user post"))
                {
                    string name = input.Substring(9).Trim();
                    await UserPost(name);
                }
                // 5. User Set
                else if (lowerInput.StartsWith("user set"))
                {
                    var parts = input.Substring(8).Trim().Split(' ');
                    if (parts.Length >= 2)
                    {
                        currentUsername = parts[0];
                        currentApiKey = parts[1];
                        Console.WriteLine("Stored");
                    }
                }
                // 6. User Delete
                else if (lowerInput.Equals("user delete"))
                {
                    if (CheckAuth()) await UserDelete();
                }
                // 7. User Role
                else if (lowerInput.StartsWith("user role"))
                {
                    if (CheckAuth()) await UserRole(input.Substring(9).Trim());
                }
                // 8. Protected Hello
                else if (lowerInput.StartsWith("protected hello"))
                {
                    if (CheckAuth()) await GetRequest("protected/hello", true);
                }
                // 9. Protected SHA1
                else if (lowerInput.StartsWith("protected sha1"))
                {
                    string msg = input.Substring(14).Trim();
                    if (CheckAuth()) await GetRequest($"protected/sha1?message={Uri.EscapeDataString(msg)}", true);
                }
                // 10. Protected SHA256
                else if (lowerInput.StartsWith("protected sha256"))
                {
                    string msg = input.Substring(16).Trim();
                    if (CheckAuth()) await GetRequest($"protected/sha256?message={Uri.EscapeDataString(msg)}", true);
                }

                // 11. Protected Get PublicKey
                else if (lowerInput.StartsWith("protected get publickey"))
                {
                    if (CheckAuth()) await GetPublicKey("protected/getpublickey", true);
                }

                else if (lowerInput.StartsWith("protected sign"))
                {
                    string msg = input.Substring(14).Trim();
                    if (CheckAuth()) await ProtectedSign(msg);
                }
                else
                {
                    Console.WriteLine("Unknown command.");
                }
            }
            catch (Exception ex)
            {
                // The instructions say the console must never crash and output errors
                Console.WriteLine(ex.Message);
            }
        }

        #region Request Helpers

        private static async Task GetRequest(string endpoint, bool useAuth = false)
        {
            Console.WriteLine("...please wait...");
            using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + endpoint);
            if (useAuth) request.Headers.Add("ApiKey", currentApiKey);

            var response = await client.SendAsync(request);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static async Task UserPost(string username)
        {
            Console.WriteLine("...please wait...");
            // Requirement says "with 'UserOne' in the request body" 
            // Usually, this means a raw JSON string: "UserOne"
            var content = new StringContent(JsonSerializer.Serialize(username), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(BaseUrl + "user/new", content);
            string responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                currentApiKey = responseBody;
                currentUsername = username;
                Console.WriteLine("Got API Key");
                Console.WriteLine(currentApiKey);
            }
            else
            {
                Console.WriteLine(responseBody);
            }
        }

        private static async Task UserDelete()
        {
            Console.WriteLine("...please wait...");
            string url = $"{BaseUrl}user/removeuser?username={currentUsername}";
            using var request = new HttpRequestMessage(HttpMethod.Delete, url);
            request.Headers.Add("ApiKey", currentApiKey);

            var response = await client.SendAsync(request);
            string result = await response.Content.ReadAsStringAsync();

            // Standardizing output to "True" or "False" as per instructions
            if (bool.TryParse(result, out bool deleted))
                Console.WriteLine(deleted ? "True" : "False");
            else
                Console.WriteLine(result);
        }

        private static async Task UserRole(string param)
        {
            Console.WriteLine("...please wait...");
            var parts = param.Split(' ');
            if (parts.Length < 2) return;

            var body = new { username = parts[0], role = parts[1] };
            using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "user/changerole");
            request.Headers.Add("ApiKey", currentApiKey);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);
            Console.WriteLine(await response.Content.ReadAsStringAsync());
        }

        private static bool CheckAuth()
        {
            if (string.IsNullOrEmpty(currentApiKey) || string.IsNullOrEmpty(currentUsername))
            {
                Console.WriteLine("You need to do a User Post or User Set first");
                return false;
            }
            return true;
        }

        private static string _serverPublicKey; // Variable to store the key

        private static async Task GetPublicKey(string param, bool useAuth = false)
        {
            try
            {
                Console.WriteLine("...please wait...");

                using var request = new HttpRequestMessage(HttpMethod.Get, BaseUrl + param);

                if (useAuth)
                {
                    // Ensure currentApiKey is accessible in this scope
                    request.Headers.Add("ApiKey", currentApiKey);
                }

                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Store the XML string for later encryption use
                    _storedServerPublicKeyXML = await response.Content.ReadAsStringAsync();

                    if (!string.IsNullOrEmpty(_storedServerPublicKeyXML))
                    {
                        Console.WriteLine("Got Public Key");
                    }
                    else
                    {
                        Console.WriteLine("Couldn’t Get the Public Key");
                    }
                }
                else
                {
                    // Optional: for debugging, you could log the status code here
                    Console.WriteLine("Couldn’t Get the Public Key");
                }
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn’t Get the Public Key");
            }
        }
        private static async Task ProtectedSign(string message)
        {
            if (string.IsNullOrEmpty(_storedServerPublicKeyXML))
            {
                Console.WriteLine("Client doesn't yet have the public key");
                return;
            }

            Console.WriteLine("...please wait...");
            using var request = new HttpRequestMessage(HttpMethod.Get, $"{BaseUrl}protected/sign?message={Uri.EscapeDataString(message)}");
            request.Headers.Add("ApiKey", currentApiKey);

            var response = await client.SendAsync(request);
            string hexSignature = await response.Content.ReadAsStringAsync();
            hexSignature = hexSignature.Trim('"'); // Remove JSON quotes if present

            try
            {
                // Convert hex with dashes back to byte array
                byte[] signatureBytes = hexSignature.Split('-').Select(x => Convert.ToByte(x, 16)).ToArray();
                byte[] dataToVerify = Encoding.ASCII.GetBytes(message);

                using var rsa = new RSACryptoServiceProvider();
                rsa.FromXmlString(_storedServerPublicKeyXML);

                bool isValid = rsa.VerifyData(dataToVerify, signatureBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

                Console.WriteLine(isValid ? "Message was successfully signed" : "Message was not successfully signed");
            }
            catch (Exception)
            {
                Console.WriteLine("Message was not successfully signed");
            }
        }
        #endregion
    }
}
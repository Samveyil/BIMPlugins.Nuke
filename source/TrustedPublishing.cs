using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace BIMPlugins.Nuke
{
    /// <summary>
    /// Exchanges a GitHub Actions OIDC token for a short-lived nuget.org API key
    /// (same protocol as the NuGet/login action).
    /// </summary>
    internal static class TrustedPublishing
    {
        private const string Audience = "https://www.nuget.org";
        private const string TokenServiceUrl = "https://www.nuget.org/api/v2/token";

        public static bool IsAvailable =>
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_URL")) &&
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_TOKEN"));

        public static string GetApiKey(string nugetUser)
        {
            if (!IsAvailable)
                throw new Exception("GitHub OIDC is not available. Run in GitHub Actions with 'permissions: id-token: write'.");

            var requestUrl = Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_URL");
            var requestToken = Environment.GetEnvironmentVariable("ACTIONS_ID_TOKEN_REQUEST_TOKEN");

            using var http = new HttpClient();
            http.DefaultRequestHeaders.UserAgent.ParseAdd("BIMPlugins.Nuke");

            // 1. GitHub OIDC token
            var oidcRequest = new HttpRequestMessage(HttpMethod.Get, $"{requestUrl}&audience={Uri.EscapeDataString(Audience)}");
            oidcRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestToken);
            var oidcResponse = http.Send(oidcRequest);
            var oidcBody = oidcResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!oidcResponse.IsSuccessStatusCode)
                throw new Exception($"Failed to get GitHub OIDC token (HTTP {(int)oidcResponse.StatusCode}).");

            var oidcToken = ReadString(oidcBody, "value")
                ?? throw new Exception("GitHub OIDC response did not contain 'value'.");
            Mask(oidcToken);

            // 2. Exchange it for a nuget.org API key
            var body = JsonSerializer.Serialize(new { username = nugetUser, tokenType = "ApiKey" });
            var exchangeRequest = new HttpRequestMessage(HttpMethod.Post, TokenServiceUrl)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };
            exchangeRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", oidcToken);
            var exchangeResponse = http.Send(exchangeRequest);
            var exchangeBody = exchangeResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!exchangeResponse.IsSuccessStatusCode)
                throw new Exception(
                    $"Token exchange failed (HTTP {(int)exchangeResponse.StatusCode}). " +
                    $"Check the Trusted Publishing policy and that '{nugetUser}' is the policy creator: {exchangeBody}");

            var apiKey = ReadString(exchangeBody, "apiKey")
                ?? throw new Exception("Token service response did not contain 'apiKey'.");
            Mask(apiKey);
            return apiKey;
        }

        private static string ReadString(string json, string property)
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty(property, out var value) ? value.GetString() : null;
        }

        // Hide the value in GitHub Actions logs.
        private static void Mask(string value) => Console.WriteLine($"::add-mask::{value}");
    }
}

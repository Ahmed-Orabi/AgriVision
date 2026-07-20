using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AgriVision.Infrastructure.Integrations.Gradio
{
    public class GradioProvider
    {
        private const int MaxRetryAttempts = 3;
        private readonly IHttpClientFactory _httpClientFactory;

        public GradioProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> UploadImageAsync(IFormFile imageFile, string baseUrl, CancellationToken ct)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                throw new ArgumentException("Image file is empty", nameof(imageFile));
            }

            var client = _httpClientFactory.CreateClient();
            for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                using var content = new MultipartFormDataContent();
                await using var fileStream = imageFile.OpenReadStream();
                using var fileContent = new StreamContent(fileStream);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);
                content.Add(fileContent, "files", imageFile.FileName);

                using var response = await client.PostAsync(
                    $"{baseUrl}/gradio_api/upload",
                    content,
                    ct);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync(ct);
                    var paths = JsonSerializer.Deserialize<string[]>(json);

                    if (paths == null || paths.Length == 0)
                    {
                        throw new InvalidOperationException("No file path returned by Gradio");
                    }

                    return paths[0];
                }

                if (!IsTransientStatusCode(response.StatusCode) || attempt == MaxRetryAttempts)
                {
                    var error = await response.Content.ReadAsStringAsync(ct);
                    throw new HttpRequestException($"Gradio upload failed ({response.StatusCode}): {error}");
                }

                await Task.Delay(TimeSpan.FromMilliseconds(600 * attempt), ct);
            }

            throw new InvalidOperationException("Upload retries exhausted with no valid response.");
        }

        public async Task<string> SubmitPredictionAsync(string imagePath, string baseUrl, CancellationToken ct)
        {
            var client = _httpClientFactory.CreateClient();

            var requestBody = new
            {
                data = new object[]
                {
                    new
                    {
                        path = imagePath,
                        meta = new { _type = "gradio.FileData" }
                    }
                }
            };

            using var response = await SendWithRetryAsync(
                () => client.PostAsync(
                    $"{baseUrl}/gradio_api/call/show_predictions_image",
                    new StringContent(
                        JsonSerializer.Serialize(requestBody),
                        Encoding.UTF8,
                        "application/json"),
                    ct),
                ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Gradio prediction submit failed ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            return doc.RootElement
                .GetProperty("event_id")
                .GetString()
                ?? throw new InvalidOperationException("Missing event_id");
        }

        public async Task<string?> GetPredictionResultAsync(string baseUrl, string eventId, CancellationToken ct)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                using var response = await client.GetAsync(
                    $"{baseUrl}/gradio_api/call/show_predictions_image/{eventId}",
                    ct
                );

                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStringAsync(ct);

                var lines = responseContent.Split('\n');
                foreach (var line in lines)
                {
                    if (!line.StartsWith("data:"))
                    {
                        continue;
                    }

                    try
                    {
                        var jsonData = line.Substring(5).Trim();
                        if (string.IsNullOrWhiteSpace(jsonData))
                        {
                            continue;
                        }

                        using var jsonDoc = JsonDocument.Parse(jsonData);
                        if (jsonDoc.RootElement.ValueKind != JsonValueKind.Array ||
                            jsonDoc.RootElement.GetArrayLength() == 0)
                        {
                            continue;
                        }

                        var firstElement = jsonDoc.RootElement[0];

                        if (firstElement.TryGetProperty("url", out var urlProp))
                        {
                            var url = urlProp.GetString();
                            if (!string.IsNullOrEmpty(url))
                            {
                                return url;
                            }
                        }

                        if (firstElement.TryGetProperty("path", out var pathProp))
                        {
                            var path = pathProp.GetString();
                            if (!string.IsNullOrEmpty(path))
                            {
                                if (path.StartsWith("/tmp/gradio", StringComparison.Ordinal))
                                {
                                    return $"{baseUrl}/gradio_api/file={path}";
                                }

                                return path;
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON parse error on line: {line}, Error: {ex.Message}");
                    }
                }

                Console.WriteLine($"Could not extract result URL from response. Raw response: {responseContent}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Get result error: {ex.Message}");
                return null;
            }
        }

        private static async Task<HttpResponseMessage> SendWithRetryAsync(
            Func<Task<HttpResponseMessage>> send,
            CancellationToken ct)
        {
            HttpResponseMessage? lastResponse = null;

            for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                lastResponse?.Dispose();
                lastResponse = await send();

                if (!IsTransientStatusCode(lastResponse.StatusCode) || attempt == MaxRetryAttempts)
                {
                    return lastResponse;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(600 * attempt), ct);
            }

            return lastResponse ?? throw new InvalidOperationException("No response returned from Gradio request.");
        }

        private static bool IsTransientStatusCode(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.ServiceUnavailable ||
                   statusCode == HttpStatusCode.BadGateway ||
                   statusCode == HttpStatusCode.GatewayTimeout ||
                   (int)statusCode == 429;
        }
    }
}

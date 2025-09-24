using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Spoken.Core
{
    public class OpenAIThoughtGrouper : IThoughtGrouper
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string OPENAI_API_URL = "https://api.openai.com/v1/chat/completions";
        
        public OpenAIThoughtGrouper(string apiKey)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        
        public bool IsAvailable => !string.IsNullOrEmpty(_apiKey);
        public string ServiceName => "OpenAI GPT-4";
        
        public async Task<List<Verse>> AnalyzeThoughtUnitsAsync(List<Verse> verses, string bookName, int chapterNumber)
        {
            if (!verses.Any()) return verses;
            
            try
            {
                var prompt = BuildAnalysisPrompt(verses, bookName, chapterNumber);
                var response = await CallOpenAIAsync(prompt);
                return ParseThoughtGroupResponse(verses, response);
            }
            catch (Exception ex)
            {
                // Log error and return original verses with default grouping
                Console.WriteLine($"AI thought grouping failed: {ex.Message}");
                return ApplyFallbackGrouping(verses);
            }
        }
        
        private string BuildAnalysisPrompt(List<Verse> verses, string bookName, int chapterNumber)
        {
            var versesText = string.Join(" ", verses.Select(v => $"[{v.Number}] {v.Text}"));
            
            return $@"Analyze the following biblical passage from {bookName} chapter {chapterNumber} and identify coherent thought units for smooth reading flow.

Biblical Text:
{versesText}

Instructions:
1. Group verses into coherent thought units based on:
   - Thematic continuity (same topic, character, or narrative thread)
   - Logical flow of ideas or arguments
   - Natural reading rhythm and comprehension
   - Speaker changes or narrative shifts
   
2. Each thought unit should:
   - Contain 2-8 verses typically
   - Have internal coherence
   - Create natural pause points for reflection
   - Enhance readability while preserving meaning

3. Respond with ONLY a JSON array of thought group start verse numbers:
   Example: [1, 4, 7, 12] means groups are: verses 1-3, 4-6, 7-11, 12-end

Consider biblical literary structure, Hebrew parallelism, and natural English reading flow.";
        }
        
        private async Task<string> CallOpenAIAsync(string prompt)
        {
            var request = new
            {
                model = "gpt-4o-mini", // Cost-effective model good for text analysis
                messages = new[]
                {
                    new { role = "system", content = "You are a biblical scholar and reading comprehension expert. Respond only with the requested JSON format." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 150,
                temperature = 0.1 // Low temperature for consistent analysis
            };
            
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(OPENAI_API_URL, content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);
            
            return responseObj.GetProperty("choices")[0]
                             .GetProperty("message")
                             .GetProperty("content")
                             .GetString() ?? string.Empty;
        }
        
        private List<Verse> ParseThoughtGroupResponse(List<Verse> verses, string response)
        {
            try
            {
                // Clean up response and parse JSON
                var cleanResponse = response.Trim().TrimStart('[').TrimEnd(']');
                var startVerses = cleanResponse.Split(',')
                    .Select(s => int.Parse(s.Trim()))
                    .OrderBy(n => n)
                    .ToList();
                
                // Apply thought group information
                var result = verses.ToList();
                int currentGroupId = 1;
                
                foreach (var verse in result)
                {
                    verse.ThoughtGroupId = currentGroupId;
                    verse.IsThoughtStart = startVerses.Contains(verse.Number);
                    
                    // Move to next group if this verse starts a new group
                    if (verse.IsThoughtStart && verse.Number > startVerses.First())
                    {
                        currentGroupId++;
                    }
                }
                
                return result;
            }
            catch
            {
                // If parsing fails, use fallback grouping
                return ApplyFallbackGrouping(verses);
            }
        }
        
        private List<Verse> ApplyFallbackGrouping(List<Verse> verses)
        {
            // Fallback: group every 4-5 verses as a reasonable default
            var result = verses.ToList();
            int currentGroupId = 1;
            
            for (int i = 0; i < result.Count; i++)
            {
                result[i].ThoughtGroupId = currentGroupId;
                result[i].IsThoughtStart = (i % 4 == 0); // Start new group every 4 verses
                
                if (result[i].IsThoughtStart && i > 0)
                {
                    currentGroupId++;
                }
            }
            
            return result;
        }
        
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
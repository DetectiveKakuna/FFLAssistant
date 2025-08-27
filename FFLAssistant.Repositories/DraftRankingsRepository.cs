using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FFLAssistant.Repositories;

public class DraftRankingsRepository(
    IOptions<DraftRankingsConfiguration> draftRankingsOptions,
    ILogger<DraftRankingsRepository> logger) : IDraftRankingsRepository
{
    private readonly ILogger<DraftRankingsRepository> _logger = logger;
    private readonly string _filePath = draftRankingsOptions.Value.RankingsJsonPath;

    public async Task<IList<DraftRanking>?> GetDraftRankingsAsync()
    {
        try
        {
            if (!await FileExistsAsync())
            {
                _logger.LogInformation("Draft rankings file does not exist: {FilePath}", _filePath);
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(_filePath);
            var rankings = JsonSerializer.Deserialize<IList<DraftRanking>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully loaded draft rankings from file");
            return rankings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading draft rankings file: {FilePath}", _filePath);
            return null;
        }
    }

    public async Task SaveDraftRankingsAsync(IList<DraftRanking> rankings)
    {
        try
        {
            var jsonContent = JsonSerializer.Serialize(rankings, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_filePath, jsonContent);
            _logger.LogInformation("Successfully saved draft rankings to file: {FilePath}", _filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving draft rankings file: {FilePath}", _filePath);
            throw;
        }
    }

    public Task<bool> FileExistsAsync()
    {
        return Task.FromResult(File.Exists(_filePath));
    }

    public Task<DateTime?> GetFileLastModifiedAsync()
    {
        try
        {
            if (!File.Exists(_filePath))
                return Task.FromResult<DateTime?>(null);

            var lastModified = File.GetLastWriteTime(_filePath);
            return Task.FromResult<DateTime?>(lastModified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file last modified time: {FilePath}", _filePath);
            return Task.FromResult<DateTime?>(null);
        }
    }
}
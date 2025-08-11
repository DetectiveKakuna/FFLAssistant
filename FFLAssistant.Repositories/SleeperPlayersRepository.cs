using FFLAssistant.Models;
using FFLAssistant.Models.Configurations;
using FFLAssistant.Repositories.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace FFLAssistant.Repositories;

public class SleeperPlayersRepository(
    IOptions<SleeperConfiguration> config,
    ILogger<SleeperPlayersRepository> logger) : ISleeperPlayersRepository
{
    private readonly SleeperConfiguration _config = config.Value;
    private readonly ILogger<SleeperPlayersRepository> _logger = logger;

    public async Task<IList<Player>?> GetPlayersAsync()
    {
        try
        {
            if (!await FileExistsAsync())
            {
                _logger.LogInformation("Sleeper players file does not exist: {FilePath}", _config.SaveFilePath);
                return null;
            }

            var jsonContent = await File.ReadAllTextAsync(_config.SaveFilePath);
            var players = JsonSerializer.Deserialize<List<Player>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Successfully loaded {Count} players from file", players?.Count ?? 0);
            return players;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading sleeper players file: {_config.SaveFilePath}", _config.SaveFilePath);
            return null;
        }
    }

    public async Task SavePlayersAsync(IList<Player> players)
    {
        try
        {
            // Ensure directory exists
            var directory = Path.GetDirectoryName(_config.SaveFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var jsonContent = JsonSerializer.Serialize(players, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await File.WriteAllTextAsync(_config.SaveFilePath, jsonContent);
            _logger.LogInformation("Successfully saved {Count} players to file: {_config.SaveFilePath}", players.Count, _config.SaveFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving sleeper players file: {_config.SaveFilePath}", _config.SaveFilePath);
            throw;
        }
    }

    public Task<bool> FileExistsAsync()
    {
        return Task.FromResult(File.Exists(_config.SaveFilePath));
    }

    public Task<DateTime?> GetFileLastModifiedAsync()
    {
        try
        {
            if (!File.Exists(_config.SaveFilePath))
                return Task.FromResult<DateTime?>(null);

            var lastModified = File.GetLastWriteTime(_config.SaveFilePath);
            return Task.FromResult<DateTime?>(lastModified);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file last modified time: {_config.SaveFilePath}", _config.SaveFilePath);
            return Task.FromResult<DateTime?>(null);
        }
    }
}
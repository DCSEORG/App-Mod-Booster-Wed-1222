using Microsoft.Data.SqlClient;
using Azure.Identity;

namespace ExpenseManagementApp.Services;

public class DatabaseService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseService> _logger;

    public DatabaseService(IConfiguration configuration, ILogger<DatabaseService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SqlConnection> GetConnectionAsync()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found in configuration");
        }

        var connection = new SqlConnection(connectionString);
        
        // If using Managed Identity authentication, acquire and set the access token
        if (connectionString.Contains("Active Directory Managed Identity", StringComparison.OrdinalIgnoreCase))
        {
            try
            {
                var credential = new DefaultAzureCredential();
                var tokenRequestContext = new Azure.Core.TokenRequestContext(new[] { "https://database.windows.net/.default" });
                var token = await credential.GetTokenAsync(tokenRequestContext);
                connection.AccessToken = token.Token;
                _logger.LogInformation("Successfully acquired access token for Managed Identity");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to acquire Managed Identity token. Connection will use alternative authentication.");
            }
        }

        return connection;
    }
    
    public SqlConnection GetConnection()
    {
        return GetConnectionAsync().GetAwaiter().GetResult();
    }
}

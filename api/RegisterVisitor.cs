using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace VisitorRegistration;

public class RegisterVisitor
{
    private readonly ILogger<RegisterVisitor> logger;

    public RegisterVisitor(ILogger<RegisterVisitor> logger)
    {
        this.logger = logger;
    }

    [Function("RegisterVisitor")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            logger.LogInformation($"Body: '{requestBody}' (Length: {requestBody.Length})");

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                logger.LogWarning("FirstName is null or missing in request.");
                return new BadRequestObjectResult(new { error = "Request body cannot be empty." });
            }

            JsonNode? node = JsonNode.Parse(requestBody);
            string? firstName = node?["firstName"]?.ToString();

            logger.LogInformation("Retrieving connection string...");
            var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("SqlConnectionString variable is invalid or not configured.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            logger.LogInformation("Successfully retrieved connection string, connecting to database...");

            VisitorModel? newVisitor = null;

            await using (var connection = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Visitors (FirstName) 
                                 OUTPUT INSERTED.Id, INSERTED.FirstName, INSERTED.Timestamp 
                                 VALUES (@firstName)";

                logger.LogInformation("Opening connection...");
                await connection.OpenAsync();
                logger.LogInformation("Connection opened.");

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@firstName", firstName);

                logger.LogInformation("Attempting to read and execute SQL query.");
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        newVisitor = new VisitorModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                        };
                    }
                }
                logger.LogInformation("Successfully performed SQL query.");
                return new OkObjectResult(newVisitor);
            }
        }
        catch (JsonException)
        {
            logger.LogError("Failed parsing of JSON.");
            return new BadRequestResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An unexpected error occurred.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
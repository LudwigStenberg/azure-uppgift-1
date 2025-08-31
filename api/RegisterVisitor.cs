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
        logger.LogInformation("Function started...");

        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(requestBody))
            {
                logger.LogWarning("The body is null or missing in the request.");
                return new BadRequestObjectResult(new { error = "Request body cannot be empty." });
            }
            logger.LogInformation($"Request Body: {requestBody}");

            JsonNode? node = JsonNode.Parse(requestBody);
            string? firstName = node?["firstName"]?.ToString();
            string? lastName = node?["lastName"]?.ToString();
            string? emailAddress = node?["emailAddress"]?.ToString();

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
                string query = @"INSERT INTO Visitors (FirstName, LastName, EmailAddress) 
                                 OUTPUT INSERTED.Id, INSERTED.FirstName, INSERTED.LastName, INSERTED.EmailAddress, INSERTED.Timestamp 
                                 VALUES (@firstName, @lastName, @emailAddress)";

                logger.LogInformation("Opening connection...");
                await connection.OpenAsync();
                logger.LogInformation("Connection opened.");

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@emailAddress", emailAddress);

                logger.LogInformation("Attempting to read and execute SQL query.");
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        newVisitor = new VisitorModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            EmailAddress = reader.GetString(reader.GetOrdinal("EmailAddress")),
                            Timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"))
                        };
                    }
                }
                logger.LogInformation("Successfully created visitor with ID: '{Id}', FirstName: '{FirstName}', LastName: {LatName}, EmailAddress: '{EmailAddress}' and Timestamp: {Timestamp}",
                     newVisitor!.Id, newVisitor.FirstName, newVisitor.LastName, newVisitor.EmailAddress, newVisitor.Timestamp);
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
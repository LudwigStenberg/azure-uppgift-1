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
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        JsonNode? node = JsonNode.Parse(requestBody);
        string? firstName = node?["firstName"]?.ToString();

        VisitorModel? newVisitor = null;
        await using var connection = new SqlConnection(Environment.GetEnvironmentVariable("SqlConnectionString"));
        {

            var query = @"INSERT INTO Visitors (FirstName) 
                        OUTPUT INSERTED.Id, INSERTED.FirstName, INSERTED.Timestamp 
                        VALUES (@firstName)";

            await connection.OpenAsync();

            var command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@firstName", firstName);

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
        }
        logger.LogInformation("Request Body: {RequestBody}", requestBody);
        logger.LogInformation("Visit made by: {Name}", firstName);
        logger.LogInformation("newVisitor retrieved from query: {NewVisitor}", newVisitor);

        if (newVisitor == null)
        {
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }

        return new OkObjectResult(newVisitor);




    }
}
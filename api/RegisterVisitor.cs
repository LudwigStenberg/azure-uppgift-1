using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace VisitorRegistration;

public class RegisterVisitor
{
    private readonly ILogger<RegisterVisitor> logger;

    public RegisterVisitor(ILogger<RegisterVisitor> logger)
    {
        this.logger = logger;
    }

    [Function("RegisterVisitor")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req, [FromBody] VisitorRequest visitor)
    {
        logger.LogInformation("Function started...");
        logger.LogInformation("HttpRequest received... Host: {Host}", req.Host);

        var context = new ValidationContext(visitor);
        var results = new List<ValidationResult>();

        if (!Validator.TryValidateObject(visitor, context, results, true))
        {
            logger.LogInformation("ValidationResults: {Results}", results);
            return new BadRequestObjectResult(results);
        }

        try
        {
            string firstName = visitor.FirstName.ToLower();
            string lastName = visitor.LastName.ToLower();
            string emailAddress = visitor.EmailAddress.ToLower();
            DateTime checkInTime = DateTime.UtcNow;

            logger.LogInformation("Retrieving connection string...");
            var connectionString = Environment.GetEnvironmentVariable("SqlConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                logger.LogError("SqlConnectionString variable is invalid or not configured.");
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
            logger.LogInformation("Successfully retrieved connection string, connecting to database...");

            VisitorModel? visitorResponse = null;

            await using (var connection = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO Visitors (FirstName, LastName, EmailAddress, CheckInTime) 
                                 OUTPUT INSERTED.Id, INSERTED.FirstName, INSERTED.LastName, INSERTED.EmailAddress, INSERTED.CheckInTime 
                                 VALUES (@firstName, @lastName, @emailAddress, @checkInTime)";

                logger.LogInformation("Opening connection...");
                await connection.OpenAsync();
                logger.LogInformation("Connection opened.");

                var command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@firstName", firstName);
                command.Parameters.AddWithValue("@lastName", lastName);
                command.Parameters.AddWithValue("@emailAddress", emailAddress);
                command.Parameters.AddWithValue("@checkInTime", checkInTime);

                logger.LogInformation("Attempting to read and execute SQL query.");
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        visitorResponse = new VisitorModel
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                            LastName = reader.GetString(reader.GetOrdinal("LastName")),
                            EmailAddress = reader.GetString(reader.GetOrdinal("EmailAddress")),
                            CheckInTime = reader.GetDateTime(reader.GetOrdinal("CheckInTime"))
                        };
                    }
                }
                logger.LogInformation("Successfully created visitor:\nID: '{Id}'\nFirstName: '{FirstName}'\nLastName: {LatName}\nEmailAddress: '{EmailAddress}'\nCheckInTime: {CheckInTime}",
                     visitorResponse!.Id, visitorResponse.FirstName, visitorResponse.LastName, visitorResponse.EmailAddress, visitorResponse.CheckInTime);
                return new OkObjectResult(visitorResponse);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An unexpected error occurred.");
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
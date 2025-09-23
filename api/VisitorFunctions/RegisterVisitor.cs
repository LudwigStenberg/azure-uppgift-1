using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
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
    private readonly ServiceBusClient serviceBusClient;

    public RegisterVisitor(ILogger<RegisterVisitor> logger, ServiceBusClient serviceBusClient)
    {
        this.logger = logger;
        this.serviceBusClient = serviceBusClient;
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

                logger.LogInformation("Attempting to read and execute SQL query...");
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

                // Service Bus -- this is just a test implementation.
                try
                {

                    // Create notification model -- this represents our data message
                    var notification = new BookingNotification
                    {
                        BookingId = visitorResponse.Id,  // Using visitor ID as booking ID for now
                        CustomerEmail = visitorResponse.EmailAddress,
                        FirstName = visitorResponse.FirstName,
                        LastName = visitorResponse.LastName,
                        EventType = "CheckedIn",
                        Timestamp = DateTime.UtcNow,
                        FacilityName = "Main Office", // Just a Placeholder
                        NumberOfParticipants = 1,
                        StartDate = visitorResponse.CheckInTime,
                        EndDate = visitorResponse.CheckInTime
                        // TotalPrice and BookingNotes are default/null
                    };

                    var sender = serviceBusClient.CreateSender("booking-notifications");
                    var message = new ServiceBusMessage(JsonSerializer.Serialize(notification));
                    await sender.SendMessageAsync(message);

                    logger.LogInformation("Check-in notification sent for visitor {Id}", visitorResponse.Id);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(
                        ex, "Failed to send check-in notification for visitor {Id}. Error: {Message}", visitorResponse.Id, ex);
                }

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
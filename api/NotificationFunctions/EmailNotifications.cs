// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Extensions.Logging;


// public class EmailNotifications
// {
//     private readonly ILogger<EmailNotifications> logger;

//     public EmailNotifications(ILogger<EmailNotifications> logger)
//     {
//         this.logger = logger;
//     }

//     [Function("EmailNotifications")]
//     public async Task Run([ServiceBusTrigger("booking-notifications")] string message)
//     {
//     }
// }
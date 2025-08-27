using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
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

        logger.LogInformation(requestBody);
        logger.LogInformation("Visit made by: {Name}", firstName);

        return firstName != null ? new OkResult() : new BadRequestObjectResult("Error during request.");
    }
}
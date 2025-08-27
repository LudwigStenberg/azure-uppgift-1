using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("C# HTTP trigger function processed a request.");

        string? name = req.Query["name"];


        return !string.IsNullOrEmpty(name)
            ? new OkObjectResult($"Hello {name}") :
           new BadRequestObjectResult("Please pass a name on the query string!");
    }
}
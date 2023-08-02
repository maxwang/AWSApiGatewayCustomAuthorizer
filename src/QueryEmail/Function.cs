using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace QueryEmail;

public class Function
{
    private readonly ILogger<Function> _logger;
    public Function()
    {
        var services = ConfigureServices();
        _logger = services.GetRequiredService<ILogger<Function>>();
    }

    public IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var environmentName =
            Environment.GetEnvironmentVariable("CORE_ENVIRONMENT");

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
            .Build();

        var loggerOptions = new LambdaLoggerOptions(configuration);

        services.AddLogging(logging => logging.AddLambdaLogger(loggerOptions));

        return services.BuildServiceProvider();
    }

    public async Task<string> MailQueryHandler(APIGatewayProxyRequest request, ILambdaContext context)
    {
        var isAdmin = request.RequestContext.Authorizer.TryGetValue("IsAdmin", out var isAdminValue) && ((JsonElement)isAdminValue).GetBoolean();

        //var  isAdmin = ((JsonElement)request.RequestContext.Authorizer["IsAdmin"]).GetBoolean();
        _logger.LogInformation("MailQueryHandler IsAdmin: {isAdmin} : {type}", isAdmin, isAdmin.GetType());
        _logger.LogInformation("MailQueryHandler Identity: {identity}", JsonSerializer.Serialize(request));
        _logger.LogInformation("MailQueryHandler Context: {context}", JsonSerializer.Serialize(context));
        return await Task.FromResult(request?.Body ?? "test").ConfigureAwait(false);
    }

    /// <summary>
    /// Mails the type of the query handler with strong.
    /// </summary>
    /// <param name="input">The input.</param>
    /// <param name="context">The context.</param>
    /// <returns></returns>
    public async Task<string> MailQueryHandler_WithStrongType(Entities.QueryEmail input,
        ILambdaContext context)
    {
        _logger.LogInformation("MailQueryHandler Identity: {identity}", JsonSerializer.Serialize(input));
        _logger.LogInformation("MailQueryHandler Identity: {context}", JsonSerializer.Serialize(context));
        return await Task.FromResult(input.ApplicationEnvironment).ConfigureAwait(false);
    }
}

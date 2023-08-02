using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace TestAwsAuthLambda;

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
    public APIGatewayCustomAuthorizerResponse ValidateToken(APIGatewayCustomAuthorizerRequest request,
        ILambdaContext context)
    {
        using (_logger.BeginScope($"{context.AwsRequestId}"))
        {
            _logger.LogInformation("Api Gateway Custom Authorize Processing");
            var effect = "Allow";
            var principalId = "max-test";
            var nameIdentifier = "max-test";

            APIGatewayCustomAuthorizerContextOutput contextOutput = new APIGatewayCustomAuthorizerContextOutput()
            {
                ["User"] = "User",
                ["IsAdmin"] = false
            };

            return new APIGatewayCustomAuthorizerResponse()
            {
                PrincipalID = principalId,
                PolicyDocument = new APIGatewayCustomAuthorizerPolicy()
                {
                    Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
                    {
                        new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement()
                        {
                            Effect = effect,
                            Resource = new HashSet<string> { request.MethodArn },
                            Action = new HashSet<string> { "execute-api:Invoke" },
                        }
                    }
                },
                Context = contextOutput
            };
        }
    }
}

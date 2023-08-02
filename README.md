
 # AWS Api Gateway Custom Authorizer Lambda example

This is a demo project to show hot to create an Api Gateway custom authorizer lambda and how to pass custom data from authorizer to lambda. 


## Technologies

* [ASP.NET Core 6](https://docs.microsoft.com/en-us/aspnet/core/introduction-to-aspnet-core)
* [Aws Lambda](https://docs.aws.amazon.com/lambda/index.html) 


## how to pass custom data from authorizer to lambd

First create `APIGatewayCustomAuthorizerContextOutput` and pass it to `APIGatewayCustomAuthorizerResponse` as below

```csharp
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
```

Then setup lambda with `Use Lambda Proxy integration`, or config `Mapping Templates`

Then use 

```csharp
public async Task<string> MailQueryHandler(APIGatewayProxyRequest request, ILambdaContext context)
{
    var isAdmin = request.RequestContext.Authorizer.TryGetValue("IsAdmin", out var isAdminValue) && ((JsonElement)isAdminValue).GetBoolean();

    //var  isAdmin = ((JsonElement)request.RequestContext.Authorizer["IsAdmin"]).GetBoolean();
    _logger.LogInformation("MailQueryHandler IsAdmin: {isAdmin} : {type}", isAdmin, isAdmin.GetType());
    _logger.LogInformation("MailQueryHandler Identity: {identity}", JsonSerializer.Serialize(request));
    _logger.LogInformation("MailQueryHandler Context: {context}", JsonSerializer.Serialize(context));
    return await Task.FromResult(request?.Body ?? "test").ConfigureAwait(false);
}
```
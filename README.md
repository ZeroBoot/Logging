# Zero Logging

Zero logging extensions for Microsoft.Extensions.Logging.

## Console

Extensions `Microsoft.Extensions.Logging.Console`, grouping scope property for display.

### Install

Install the _Zero.Logging.Console_ [NuGet package](https://www.nuget.org/packages/Zero.Logging.Console) into your app:

```powershell
dotnet add package Zero.Logging.Console --version 5.0.2
```

### Configure

**First**, Configure in `appsettings.json`：

```json
"Logging": {
    "LogLevel": {
        "Default": "Information",
        "Microsoft": "Warning",
        "Microsoft.Hosting.Lifetime": "Information"
    },
    "Console": {
        "FormatterName": "json",
        "IncludeScopes": true,
        "FormatterOptions": {
            "IncludeScopes": true
        }
    }
}
```

**Next**, Edit your web application's _Startup.cs_ file, add zero logging service in ConfigureServices:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddZeroJsonConsoleLogging();

    services.AddControllers();
}
```

### Demonstrate

**Finally**, Add `LoggingController` to test logging:

```csharp
[ApiController]
[Route("[controller]")]
public class LoggingController : ControllerBase
{
    private readonly ILogger<LoggingController> _logger;

    public LoggingController(ILogger<LoggingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public string Get()
    {
        using (_logger.BeginScope("myScope"))
        {
            _logger.LogInformation("test1");
        }

        using (_logger.BeginScope("OrderId : {orderId}, UserId : {userId}", 1, 2))
        {
            _logger.LogInformation("test2");
        }

        using (_logger.BeginScope(new OrderLogScope(3, 4)))
        {
            _logger.LogInformation("test3");
        }

        return "ok";
    }
}
```

That's it! Access the [http://localhost:5000/logging](http://localhost:5000/logging)，you will see log output like:

```json
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "Test",
  "Message": "test1",
  "Scope": {
    "Activity": {
      "Message": "SpanId:fc1e2bb7dd8dba4f, TraceId:f62fde06a229b14e8ba9e8a51512814b, ParentId:0000000000000000",
      "SpanId": "fc1e2bb7dd8dba4f",
      "TraceId": "f62fde06a229b14e8ba9e8a51512814b",
      "ParentId": "0000000000000000"
    },
    "Connection": {
      "Message": "ConnectionId:0HM8AVNE070U2",
      "ConnectionId": "0HM8AVNE070U2"
    },
    "Hosting": {
      "Message": "RequestPath:/ RequestId:0HM8AVNE070U2:00000002",
      "RequestId": "0HM8AVNE070U2:00000002",
      "RequestPath": "/"
    },
    "Plain": [
      "myScope"
    ]
  }
}
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "Test",
  "Message": "test2",
  "Scope": {
    "Activity": {
      "Message": "SpanId:fc1e2bb7dd8dba4f, TraceId:f62fde06a229b14e8ba9e8a51512814b, ParentId:0000000000000000",
      "SpanId": "fc1e2bb7dd8dba4f",
      "TraceId": "f62fde06a229b14e8ba9e8a51512814b",
      "ParentId": "0000000000000000"
    },
    "Connection": {
      "Message": "ConnectionId:0HM8AVNE070U2",
      "ConnectionId": "0HM8AVNE070U2"
    },
    "Hosting": {
      "Message": "RequestPath:/ RequestId:0HM8AVNE070U2:00000002",
      "RequestId": "0HM8AVNE070U2:00000002",
      "RequestPath": "/"
    },
    "Custom": {
      "Message": "OrderId : 1, UserId : 2",
      "orderId": 1,
      "userId": 2
    }
  }
}
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "Test",
  "Message": "test3",
  "Scope": {
    "Activity": {
      "Message": "SpanId:fc1e2bb7dd8dba4f, TraceId:f62fde06a229b14e8ba9e8a51512814b, ParentId:0000000000000000",
      "SpanId": "fc1e2bb7dd8dba4f",
      "TraceId": "f62fde06a229b14e8ba9e8a51512814b",
      "ParentId": "0000000000000000"
    },
    "Connection": {
      "Message": "ConnectionId:0HM8AVNE070U2",
      "ConnectionId": "0HM8AVNE070U2"
    },
    "Hosting": {
      "Message": "RequestPath:/ RequestId:0HM8AVNE070U2:00000002",
      "RequestId": "0HM8AVNE070U2:00000002",
      "RequestPath": "/"
    },
    "Order": {
      "Message": "OrderId:3 UserId:4",
      "OrderId": 3,
      "UserId": 4
    }
  }
}
```

As above, The `Scope` is group by type.

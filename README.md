# Zero Logging

Zero logging extensions for Microsoft.Extensions.Logging.

## Console

Extensions `Microsoft.Extensions.Logging.Console`, grouping scope property for display.

### Install

Install the _Zero.Logging.Console_ [NuGet package](https://www.nuget.org/packages/Zero.Logging.Console) into your app:

```powershell
dotnet add package Zero.Logging.Console --version 5.0.0
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
        _logger.LogInformation("test");

        using (_logger.BeginScope(new Dictionary<string, string> { ["k1"] = "v1", ["k2"] = "v2" }))
        {
            _logger.LogInformation("test2");
        }

        using (_logger.BeginScope("myScope"))
        {
            _logger.LogInformation("test3");
        }

        return "ok";
    }
}
```

That's it! Access the `/logging`，you will see log output like:

```json
{
    "EventId": 0,
    "LogLevel": "Information",
    "Category": "WebApiDemo.Controllers.LoggingController",
    "Message": "test",
    "State": {
        "Message": "test",
        "{OriginalFormat}": "test"
    },
    "Scope": {
        "Activity": {
            "SpanId": "e868c0ea2c961510",
            "TraceId": "fb74e90e8c61ff51931fd4ba8e4e71bf",
            "ParentId": "0000000000000000"
        },
        "Connection": {
            "ConnectionId": "0HM7DRIKFU2Q3"
        },
        "Hosting": {
            "RequestId": "0HM7DRIKFU2Q3:00000003",
            "RequestPath": "/logging"
        },
        "Action": {
            "ActionId": "c4d35f4e-f7d9-49f1-933d-a4d8566beb05",
            "ActionName": "WebApiDemo.Controllers.LoggingController.Get (WebApiDemo)"
        }
    }
}                                        

{
    "EventId": 0,
    "LogLevel": "Information",
    "Category": "WebApiDemo.Controllers.LoggingController",
    "Message": "test2",
    "State": {
        "Message": "test2",
        "{OriginalFormat}": "test2"
    },
    "Scope": {
        "Activity": {
            "SpanId": "e868c0ea2c961510",
            "TraceId": "fb74e90e8c61ff51931fd4ba8e4e71bf",
            "ParentId": "0000000000000000"
        },
        "Connection": {
            "ConnectionId": "0HM7DRIKFU2Q3"
        },
        "Hosting": {
            "RequestId": "0HM7DRIKFU2Q3:00000003",
            "RequestPath": "/logging"
        },
        "Action": {
            "ActionId": "c4d35f4e-f7d9-49f1-933d-a4d8566beb05",
            "ActionName": "WebApiDemo.Controllers.LoggingController.Get (WebApiDemo)"
        },
        "Custom": {
            "k1": "v1",
            "k2": "v2"
        }
    }
}                      

{
    "EventId": 0,
    "LogLevel": "Information",
    "Category": "WebApiDemo.Controllers.LoggingController",
    "Message": "test3",
    "State": {
        "Message": "test3",
        "{OriginalFormat}": "test3"
    },
    "Scope": {
        "Activity": {
            "SpanId": "e868c0ea2c961510",
            "TraceId": "fb74e90e8c61ff51931fd4ba8e4e71bf",
            "ParentId": "0000000000000000"
        },
        "Connection": {
            "ConnectionId": "0HM7DRIKFU2Q3"
        },
        "Hosting": {
            "RequestId": "0HM7DRIKFU2Q3:00000003",
            "RequestPath": "/logging"
        },
        "Action": {
            "ActionId": "c4d35f4e-f7d9-49f1-933d-a4d8566beb05",
            "ActionName": "WebApiDemo.Controllers.LoggingController.Get (WebApiDemo)"
        },
        "Plain": [
            "myScope"
        ]
    }
}
```

As above, The `Scope` is group by type.

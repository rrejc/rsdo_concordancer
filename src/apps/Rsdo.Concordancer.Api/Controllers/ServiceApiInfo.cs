using System;
using Microsoft.OpenApi;

namespace Rsdo.Concordancer.Api.Controllers;

public static class ServiceApiInfo
{
    public const string ServiceName = "RSDO";
    public const string ApiGroupConcordancer = "concordancer";
    public const string ApiGroupDashboard = "dashboard";

    public static Version ServiceVersion => typeof(ServiceApiInfo).Assembly.GetName().Version;

    public static OpenApiInfo GetOpenApiInfo()
    {
        return new OpenApiInfo()
        {
            Title = $"{ServiceName} API",
            Version = ServiceVersion.ToString(3),
            Contact = new OpenApiContact()
            {
                Name = "Amebis, d. o. o., Kamnik",
                Email = "info@amebis.si",
                Url = new Uri("https://amebis.si"),
            },
            Description = $"API which is used in a concordancer developed in the project RSDO (Razvoj slovenščine v digitalnem okolju).",
        };
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Rsdo.Concordancer.Api.Controllers;
using Rsdo.Concordancer.Api.Framework;
using Rsdo.Concordancer.Core.Constants;
using Rsdo.Concordancer.Data.CompositionRoot;
using Rsdo.Concordancer.Infrastructure.CompositionRoot;
using Rsdo.Concordancer.ServiceModel.Shared;
using Rsdo.Concordancer.Services.CompositionRoot;
using Rsdo.Concordancer.Services.Framework.Cache;
using Serilog;

var builder = WebApplication.CreateBuilder(
    new WebApplicationOptions()
    {
        Args = args,
        WebRootPath = "./WebApp/build",
    });

builder.Host.UseSerilog(
    (context, _, loggerConfiguration) =>
    {
        loggerConfiguration.ReadFrom.Configuration(context.Configuration);
    });

builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(
    containerBuilder =>
    {
        containerBuilder.RegisterModule(new ServicesModule());
        containerBuilder.RegisterModule(new InfrastructureModule());
        containerBuilder.RegisterModule(new DataModule());
        containerBuilder.RegisterBuildCallback(
            (c) =>
            {
                var warmUps = c.Resolve<IEnumerable<ICacheWarmUp>>();
                foreach (var warmUp in warmUps)
                {
                    warmUp.WarmUp();
                }
            });
    });

builder.Services.AddCors(
    opt =>
    {
        opt.AddPolicy(
            "CorsPolicy",
            policy =>
            {
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
    });

builder.Services.AddControllers()
    .AddJsonOptions(
        opts =>
        {
            // Bind strings to enums
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

builder.Services.AddSwaggerGen(
    c =>
    {
        c.SwaggerDoc(
            ServiceApiInfo.ApiGroupConcordancer,
            info: new OpenApiInfo()
            {
                Title = $"{ServiceApiInfo.ServiceName} Concordancer API",
                Version = $"v{ServiceApiInfo.ServiceVersion.ToString(3)}",
            });
        c.SwaggerDoc(
            ServiceApiInfo.ApiGroupDashboard,
            info: new OpenApiInfo()
            {
                Title = $"{ServiceApiInfo.ServiceName} Dashboard API",
                Version = $"v{ServiceApiInfo.ServiceVersion.ToString(3)}",
            });

        // enable attribute annotations
        c.EnableAnnotations();

        // include code documentation to the swagger doc
        foreach (var assembly in new[] { Assembly.GetExecutingAssembly(), typeof(ExecutionResult).Assembly })
        {
            var xmlFile = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
        }
    });

builder.Services.AddHangfire(
    x =>
    {
        // It would be better to use ConnectionStringProvider, but in this case
        // we would have to build the container which would (at this point) double singletons.
        // So we are duplicated code from ConnectionStringProvider
        var connectionString = builder.Configuration[ConfigurationKey.Database.MasterConnectionString];
        x.UsePostgreSqlStorage(
            options =>
            {
                options.UseNpgsqlConnection(connectionString);
            });
        x.UseMediator();
    });

builder.Services.AddHangfireServer();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(
    c =>
    {
        c.SwaggerEndpoint($"{ServiceApiInfo.ApiGroupConcordancer}/swagger.json", $"{ServiceApiInfo.ServiceName} Concordancer API");
        c.SwaggerEndpoint($"{ServiceApiInfo.ApiGroupDashboard}/swagger.json", $"{ServiceApiInfo.ServiceName} Dashboard API");
    });

app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthorization();

app.MapControllers();
app.MapHangfireDashboard("/hangfire");
app.MapFallbackToFile("index.html");

app.Run();
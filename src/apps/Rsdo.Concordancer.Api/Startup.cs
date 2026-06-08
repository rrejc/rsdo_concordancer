using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using Autofac;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
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
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rsdo.Concordancer.Api;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddCors(
            opt =>
            {
                opt.AddPolicy(
                    "CorsPolicy",
                    policy =>
                    {
                        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                    });
            });
        services.AddControllers()
            .AddJsonOptions(
                opts =>
                {
                    // Bind strings to enums
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });
        services.AddSwaggerGen(
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
                ConfigureSwaggerApiDoc(c, Assembly.GetExecutingAssembly(), typeof(ExecutionResult).Assembly);
            });
        services.AddHangfire(
            x =>
            {
                // It would be better to use ConnectionStringProvider, but in this case
                // we would have to build the container which would (at this point) double singletons.
                // So we are duplicated code from ConnectionStringProvider
                var connectionString = Configuration[ConfigurationKey.Database.MasterConnectionString];
                x.UsePostgreSqlStorage(
                    options =>
                    {
                        options.UseNpgsqlConnection(connectionString);
                    });
                x.UseMediator();
            });
        services.AddHangfireServer();
        services.AddHttpClient();
    }

    public void ConfigureSwaggerApiDoc(SwaggerGenOptions options, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var xmlFile = $"{assembly.GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath);
        }
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterModule(new ServicesModule());
        builder.RegisterModule(new InfrastructureModule());
        builder.RegisterModule(new DataModule());
        builder.RegisterBuildCallback(
            (c) =>
            {
                var warmUps = c.Resolve<IEnumerable<ICacheWarmUp>>();
                foreach (var warmUp in warmUps)
                {
                    warmUp.WarmUp();
                }
            });
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
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

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard("/hangfire");
                endpoints.MapFallbackToFile("index.html");
            });
    }
}
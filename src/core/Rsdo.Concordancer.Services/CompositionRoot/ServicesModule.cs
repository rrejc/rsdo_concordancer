using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using FluentValidation;
using Microsoft.Extensions.Caching.Memory;
using Rsdo.Concordancer.Core.Interfaces;
using Rsdo.Concordancer.Core.Search.Queries.Concordances;
using Rsdo.Concordancer.Core.Search.Queries.TermLists;
using Rsdo.Concordancer.ServiceModel.Requests.Concordances;
using Rsdo.Concordancer.ServiceModel.Requests.TermLists;
using Rsdo.Concordancer.ServiceModel.Types;
using Rsdo.Concordancer.Services.Framework;
using Rsdo.Concordancer.Services.Framework.BulkLoaders;
using Rsdo.Concordancer.Services.Framework.Cache;
using Rsdo.Concordancer.Services.Framework.DatabaseManager;
using Rsdo.Concordancer.Services.Framework.DbContext;
using Rsdo.Concordancer.Services.Framework.Decorators;
using Rsdo.Concordancer.Services.Search.Aggregations;
using Rsdo.Concordancer.Services.Search.AlternateSearches;
using Rsdo.Concordancer.Services.Search.QueryFactories;
using Rsdo.Concordancer.Services.Search.QueryFactories.Concordances;
using Rsdo.Concordancer.Services.Search.QueryFactories.TermLists;
using Rsdo.Concordancer.Services.Services.InputQueryParser;
using Rsdo.Concordancer.Services.Services.LemmatizationService;
using Rsdo.Concordancer.Services.Services.ParagraphService;
using Rsdo.Concordancer.Services.Services.PartOfSpeechService;
using Rsdo.Concordancer.Services.Services.TokenizerService;
using Module = Autofac.Module;

namespace Rsdo.Concordancer.Services.CompositionRoot;

public class ServicesModule : Module
{
    private Assembly ServicesAssembly => GetType().Assembly;

    protected override void Load(ContainerBuilder builder)
    {
        base.Load(builder);

        RegisterCache(builder);
        RegisterDatabase(builder);
        RegisterMediator(builder);
        RegisterServiceBus(builder);
        RegisterRequestHandlers(builder);
        RegisterSearch(builder);
        RegisterServices(builder);
    }

    private static void RegisterCache(ContainerBuilder builder)
    {
        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        builder.RegisterInstance(memoryCache).As<IMemoryCache>().SingleInstance();

        builder.RegisterType<PartOfSpeechCacheWarmUp>().As<ICacheWarmUp>().InstancePerDependency();
    }

    private static void RegisterDatabase(ContainerBuilder builder)
    {
        // Temporary switch
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        // Database manager
        builder.RegisterType<PostgreSqlDatabaseManager>().As<IDatabaseManager>().SingleInstance();

        // Db contexts
        builder.RegisterType<MasterDbContext>().InstancePerLifetimeScope();
        builder.RegisterType<CorpusDbContext>().InstancePerLifetimeScope();

        // Bulk loaders
        builder.RegisterType<PostgreSqlBulkLoader>().As<IBulkLoader>().InstancePerLifetimeScope();
    }

    private static void RegisterMediator(ContainerBuilder builder)
    {
        // Mediator
        builder.RegisterType<Mediator>().As<IMediator>().InstancePerLifetimeScope();
    }

    private static void RegisterServiceBus(ContainerBuilder builder)
    {
        // Service bus
        builder.RegisterType<ServiceBus>().As<IServiceBus>().InstancePerLifetimeScope();
    }

    private static void RegisterServices(ContainerBuilder builder)
    {
        builder.RegisterType<ClasslaTokenizerService>().Keyed<ITokenizerService>(TokenizerType.Classla).SingleInstance();
        builder.RegisterType<DefaultTokenizerService>().Keyed<ITokenizerService>(TokenizerType.Default).SingleInstance();
        builder.RegisterType<InputQueryParser>().As<IInputQueryParser>().SingleInstance();
        builder.RegisterType<ParagraphService>().As<IParagraphService>().InstancePerLifetimeScope();
        builder.RegisterType<SimpleLemmatizationService>().Keyed<ILemmatizationService>(LemmatizationType.Simple).InstancePerLifetimeScope();
        builder.RegisterType<WildcardLemmatizationService>().Keyed<ILemmatizationService>(LemmatizationType.Wildcard).InstancePerLifetimeScope();
        builder.RegisterType<PartOfSpeechService>().As<IPartOfSpeechService>().InstancePerLifetimeScope();
    }

    private void RegisterRequestHandlers(ContainerBuilder builder)
    {
        // Validators
        builder.RegisterAssemblyTypes(ServicesAssembly).AsClosedTypesOf(typeof(IValidator<>)).AsImplementedInterfaces().InstancePerLifetimeScope();

        // Request handlers
        builder.RegisterAssemblyTypes(ServicesAssembly).AsClosedTypesOf(typeof(IRequestHandler<,>)).AsImplementedInterfaces();
        builder.RegisterGenericDecorator(typeof(SearchConcordancesDecorator<,>), typeof(IRequestHandler<,>));
        builder.RegisterGenericDecorator(typeof(CurrentContextInitializationDecorator<,>), typeof(IRequestHandler<,>));
        builder.RegisterGenericDecorator(typeof(LoggingDecorator<,>), typeof(IRequestHandler<,>));
        builder.RegisterGenericDecorator(typeof(RequestValidationDecorator<,>), typeof(IRequestHandler<,>));
    }

    private void RegisterSearch(ContainerBuilder builder)
    {
        // Query factories
        var wildcardLemmatizationServiceParameter = new ResolvedParameter(
            (p, ctx) => p.ParameterType == typeof(ILemmatizationService),
            (p, ctx) => ctx.ResolveKeyed<ILemmatizationService>(LemmatizationType.Wildcard));
        var simpleLemmatizationServiceParameter = new ResolvedParameter(
            (p, ctx) => p.ParameterType == typeof(ILemmatizationService),
            (p, ctx) => ctx.ResolveKeyed<ILemmatizationService>(LemmatizationType.Simple));

        builder.RegisterType<ExportConcordancesQueryFactory>()
            .As<IQueryFactory<ExportConcordances, ConcordancesQuery>>()
            .WithParameter(simpleLemmatizationServiceParameter)
            .InstancePerLifetimeScope();
        builder.RegisterType<SearchConcordancesQueryFactory>()
            .As<IQueryFactory<SearchConcordances, ConcordancesQuery>>()
            .WithParameter(simpleLemmatizationServiceParameter)
            .InstancePerLifetimeScope();
        builder.RegisterType<ConcordanceDetailsQueryFactory>()
            .As<IQueryFactory<ConcordanceDetails, ConcordancesQuery>>()
            .WithParameter(simpleLemmatizationServiceParameter)
            .InstancePerLifetimeScope();
        builder.RegisterType<ExportTermListQueryFactory>()
            .As<IQueryFactory<ExportTermList, TermListQuery>>()
            .WithParameter(wildcardLemmatizationServiceParameter)
            .InstancePerLifetimeScope();
        builder.RegisterType<SearchTermListQueryFactory>()
            .As<IQueryFactory<SearchTermList, TermListQuery>>()
            .WithParameter(wildcardLemmatizationServiceParameter)
            .InstancePerLifetimeScope();

        // Aggregations
        builder.RegisterType<AggregationProviderFactory>().As<IAggregationProviderFactory>().InstancePerLifetimeScope();
        builder.RegisterType<TextAggregationProvider>().Keyed<IAggregationProvider>(AggregationType.Text).InstancePerLifetimeScope();

        // Alternate searches
        builder.RegisterType<LemmasAlternateSearchProvider>()
            .As<IAlternateSearchProvider<SearchConcordances, SearchConcordancesResponse>>()
            .WithParameter(simpleLemmatizationServiceParameter)
            .InstancePerLifetimeScope();
    }
}
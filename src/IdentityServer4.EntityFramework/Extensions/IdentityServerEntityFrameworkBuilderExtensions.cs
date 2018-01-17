// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Services;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using System;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods to add EF database support to IdentityServer.
    /// </summary>
    public static class IdentityServerEntityFrameworkBuilderExtensions
    {
        /// <summary>
        /// Configures EF implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStore(
            this IIdentityServerBuilder builder,
            Action<ConfigurationStoreOptions> storeOptionsAction = null)
        {
            return builder.AddConfigurationStore<ConfigurationDbContext, Client, IdentityResource, ApiResource>(storeOptionsAction);
        }

        /// <summary>
        /// Configures EF implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <typeparam name="TContext">The IConfigurationDbContext to use.</typeparam>
        /// <typeparam name="TClient">The Client Entity Type to use.</typeparam>
        /// <typeparam name="TIdentityResource">The IdentityResource Entity Type to use</typeparam>
        /// <typeparam name="TApiResource">The ApiResource Entity Type to use</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStore<TContext, TClient, TIdentityResource, TApiResource>(
            this IIdentityServerBuilder builder,
            Action<ConfigurationStoreOptions> storeOptionsAction = null)
            where TContext : DbContext, IConfigurationDbContext<TClient, TIdentityResource, TApiResource>
            where TClient : Client
            where TIdentityResource : IdentityResource 
            where TApiResource : ApiResource
        {
            var options = new ConfigurationStoreOptions();
            builder.Services.AddSingleton(options);
            storeOptionsAction?.Invoke(options);

            if (options.ResolveDbContextOptions != null)
            {
                builder.Services.AddDbContext<TContext>(options.ResolveDbContextOptions);
            }
            else
            {
                builder.Services.AddDbContext<TContext>(dbCtxBuilder =>
                {
                    options.ConfigureDbContext?.Invoke(dbCtxBuilder);
                });
            }
            builder.Services.AddScoped<IConfigurationDbContext<TClient, TIdentityResource, TApiResource>, TContext>();
            builder.Services.AddScoped<IClientDbContext<TClient>, TContext>();
            builder.Services.AddScoped<IResourceDbContext<TIdentityResource, TApiResource>, TContext>();

            builder.Services.AddTransient<IClientStore, ClientStore<TClient>>();
            builder.Services.AddTransient<IResourceStore, ResourceStore<TIdentityResource, TApiResource>>();
            builder.Services.AddTransient<ICorsPolicyService, CorsPolicyService<TClient>>();

            return builder;
        }

        /// <summary>
        /// Configures caching for Default IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStoreCache(
            this IIdentityServerBuilder builder)
        {
            builder.AddInMemoryCaching();

            // add the caching decorators
            builder.AddClientStoreCache<ClientStore<Client>>();
            builder.AddResourceStoreCache<ResourceStore<IdentityResource, ApiResource>>();
            builder.AddCorsPolicyCache<CorsPolicyService<Client>>();

            return builder;
        }

        /// <summary>
        /// Configures caching for Generic IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
        /// </summary>
        /// <typeparam name="TClient">The Client Entity Type to use.</typeparam>
        /// <typeparam name="TIdentityResource">The IdentityResource Entity Type to use</typeparam>
        /// <typeparam name="TApiResource">The ApiResource Entity Type to use</typeparam>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddConfigurationStoreCache<TClient, TIdentityResource, TApiResource>(
            this IIdentityServerBuilder builder) 
            where TClient : Client 
            where TIdentityResource : IdentityResource 
            where TApiResource : ApiResource
        {
            builder.AddInMemoryCaching();

            // add the caching decorators
            builder.AddClientStoreCache<ClientStore<TClient>>();
            builder.AddResourceStoreCache<ResourceStore<TIdentityResource, TApiResource>>();
            builder.AddCorsPolicyCache<CorsPolicyService<TClient>>();

            return builder;
        }

        /// <summary>
        /// Configures EF implementation of IPersistedGrantStore with IdentityServer.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore(
            this IIdentityServerBuilder builder,
            Action<OperationalStoreOptions> storeOptionsAction = null)
        {
            return builder.AddOperationalStore<PersistedGrantDbContext>(storeOptionsAction);
        }

        /// <summary>
        /// Configures EF implementation of IPersistedGrantStore with IdentityServer.
        /// </summary>
        /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="storeOptionsAction">The store options action.</param>
        /// <returns></returns>
        public static IIdentityServerBuilder AddOperationalStore<TContext>(
            this IIdentityServerBuilder builder,
            Action<OperationalStoreOptions> storeOptionsAction = null)
            where TContext : DbContext, IPersistedGrantDbContext
        {
            builder.Services.AddSingleton<TokenCleanup>();
            builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

            var storeOptions = new OperationalStoreOptions();
            builder.Services.AddSingleton(storeOptions);
            storeOptionsAction?.Invoke(storeOptions);

            if (storeOptions.ResolveDbContextOptions != null)
            {
                builder.Services.AddDbContext<TContext>(storeOptions.ResolveDbContextOptions);
            }
            else
            {
                builder.Services.AddDbContext<TContext>(dbCtxBuilder =>
                {
                    storeOptions.ConfigureDbContext?.Invoke(dbCtxBuilder);
                });
            }

            builder.Services.AddScoped<IPersistedGrantDbContext, TContext>();
            builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();

            return builder;
        }

        class TokenCleanupHost : IHostedService
        {
            private readonly TokenCleanup _tokenCleanup;
            private readonly OperationalStoreOptions _options;

            public TokenCleanupHost(TokenCleanup tokenCleanup, OperationalStoreOptions options)
            {
                _tokenCleanup = tokenCleanup;
                _options = options;
            }

            public Task StartAsync(CancellationToken cancellationToken)
            {
                if (_options.EnableTokenCleanup)
                {
                    _tokenCleanup.Start(cancellationToken);
                }
                return Task.CompletedTask;
            }

            public Task StopAsync(CancellationToken cancellationToken)
            {
                if (_options.EnableTokenCleanup)
                {
                    _tokenCleanup.Stop();
                }
                return Task.CompletedTask;
            }
        }
    }
}

﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using FluentAssertions;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.EntityFramework.Options;
using IdentityServer4.EntityFramework.Stores;
using IdentityServer4.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace IdentityServer4.EntityFramework.IntegrationTests.Stores
{
    public class ClientStoreTests : IClassFixture<DatabaseProviderFixture<ConfigurationDbContext>>
    {
        private static readonly ConfigurationStoreOptions StoreOptions = new ConfigurationStoreOptions();
        public static readonly TheoryData<DbContextOptions<ConfigurationDbContext>> TestDatabaseProviders = new TheoryData<DbContextOptions<ConfigurationDbContext>>
        {
            DatabaseProviderBuilder.BuildInMemory<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlite<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions),
            DatabaseProviderBuilder.BuildSqlServer<ConfigurationDbContext>(nameof(ClientStoreTests), StoreOptions)
        };

        public ClientStoreTests(DatabaseProviderFixture<ConfigurationDbContext> fixture)
        {
            fixture.Options = TestDatabaseProviders.SelectMany(x => x.Select(y => (DbContextOptions<ConfigurationDbContext>)y)).ToList();
            fixture.StoreOptions = StoreOptions;
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void FindClientByIdAsync_WhenClientExists_ExpectClientRetured(DbContextOptions<ConfigurationDbContext> options)
        {
            var testClient = new Client
            {
                ClientId = "test_client",
                ClientName = "Test Client"
            };

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.Clients.Add(testClient.ToEntity());
                context.SaveChanges();
            }

            Client client;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ClientStore<Entities.Client>(context, FakeLogger<ClientStore<Entities.Client>>.Create());
                client = store.FindClientByIdAsync(testClient.ClientId).Result;
            }

            Assert.NotNull(client);
        }

        [Theory, MemberData(nameof(TestDatabaseProviders))]
        public void FindClientByIdAsync_WhenClientExists_ExpectClientPropertiesRetured(DbContextOptions<ConfigurationDbContext> options)
        {
            var testClient = new Client
            {
                ClientId = "properties_test_client",
                ClientName = "Properties Test Client",
                Properties =
                {
                    { "foo1", "bar1" },
                    { "foo2", "bar2" },
                }
            };

            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                context.Clients.Add(testClient.ToEntity());
                context.SaveChanges();
            }

            Client client;
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = new ClientStore<Entities.Client>(context, FakeLogger<ClientStore<Entities.Client>>.Create());
                client = store.FindClientByIdAsync(testClient.ClientId).Result;
            }

            client.Properties.Should().NotBeNull();
            client.Properties.Count.Should().Be(2);
            client.Properties.ContainsKey("foo1").Should().BeTrue();
            client.Properties.ContainsKey("foo2").Should().BeTrue();
            client.Properties["foo1"].Should().Be("bar1");
            client.Properties["foo2"].Should().Be("bar2");
        }
    }
}
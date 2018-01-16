// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.Interfaces
{
    /// <summary>
    /// Abstraction for the configuration context.
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    /// <typeparam name="TIdentityResource"></typeparam>
    /// <typeparam name="TApiResource"></typeparam>
    /// <seealso cref="System.IDisposable" />
    public interface IConfigurationDbContext<TClient, TIdentityResource, TApiResource> : 
        IClientDbContext<TClient>,
        IResourceDbContext<TIdentityResource, TApiResource>
        where TClient : Client 
        where TIdentityResource : IdentityResource 
        where TApiResource : ApiResource
    {
        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        int SaveChanges();
        
        /// <summary>
        /// Saves the changes.
        /// </summary>
        /// <returns></returns>
        Task<int> SaveChangesAsync();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TClient"></typeparam>
    public interface IClientDbContext<TClient> : IDisposable
        where TClient : Client

    {
        /// <summary>
        /// Gets or sets the clients.
        /// </summary>
        /// <value>
        /// The clients.
        /// </value>
        DbSet<TClient> Clients { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TIdentityResource"></typeparam>
    public interface IResourceDbContext<TIdentityResource, TApiResource> : IDisposable
        where TIdentityResource : IdentityResource
        where TApiResource : ApiResource
    {

        /// <summary>
        /// Gets or sets the identity resources.
        /// </summary>
        /// <value>
        /// The identity resources.
        /// </value>
        DbSet<TIdentityResource> IdentityResources { get; set; }

        /// <summary>
        /// Gets or sets the API resources.
        /// </summary>
        /// <value>
        /// The API resources.
        /// </value>
        DbSet<TApiResource> ApiResources { get; set; }
    }
}
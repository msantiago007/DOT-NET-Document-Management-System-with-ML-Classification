// -----------------------------------------------------------------------------
// <copyright file="RoleBasedAuthorizationExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for role-based authorization
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Builder;
using DocumentManagementML.API.Middleware;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for role-based authorization
    /// </summary>
    public static class RoleBasedAuthorizationExtensions
    {
        /// <summary>
        /// Adds role-based authorization middleware to the application
        /// </summary>
        /// <param name="app">Application builder</param>
        /// <returns>Application builder</returns>
        public static IApplicationBuilder UseRoleBasedAuthorization(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RoleBasedAuthorizationMiddleware>();
        }
    }
}
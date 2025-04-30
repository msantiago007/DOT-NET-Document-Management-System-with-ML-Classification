// -----------------------------------------------------------------------------
// <copyright file="ClaimsPrincipalExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extensions for ClaimsPrincipal
// -----------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extensions for ClaimsPrincipal
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the user ID from claims
        /// </summary>
        /// <param name="principal">Claims principal</param>
        /// <returns>User ID or null if not found</returns>
        public static string? GetUserId(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }
        
        /// <summary>
        /// Gets the username from claims
        /// </summary>
        /// <param name="principal">Claims principal</param>
        /// <returns>Username or null if not found</returns>
        public static string? GetUsername(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Name);
        }
        
        /// <summary>
        /// Gets the email from claims
        /// </summary>
        /// <param name="principal">Claims principal</param>
        /// <returns>Email or null if not found</returns>
        public static string? GetEmail(this ClaimsPrincipal principal)
        {
            return principal.FindFirstValue(ClaimTypes.Email);
        }
        
        /// <summary>
        /// Checks if the user is in a specific role
        /// </summary>
        /// <param name="principal">Claims principal</param>
        /// <param name="role">Role to check</param>
        /// <returns>True if the user is in the role, false otherwise</returns>
        public static bool IsInRole(this ClaimsPrincipal principal, string role)
        {
            return principal.HasClaim(ClaimTypes.Role, role);
        }
        
        /// <summary>
        /// Gets all roles for the user
        /// </summary>
        /// <param name="principal">Claims principal</param>
        /// <returns>Collection of roles</returns>
        public static IEnumerable<string> GetRoles(this ClaimsPrincipal principal)
        {
            return principal.FindAll(ClaimTypes.Role).Select(c => c.Value);
        }
    }
}
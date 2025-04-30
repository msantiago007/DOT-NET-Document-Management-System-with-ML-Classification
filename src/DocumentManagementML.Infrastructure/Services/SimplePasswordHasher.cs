// -----------------------------------------------------------------------------
// <copyright file="SimplePasswordHasher.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Simple implementation of IPasswordHasher for development
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.Services;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DocumentManagementML.Infrastructure.Services
{
    /// <summary>
    /// Simple implementation of IPasswordHasher for development
    /// </summary>
    public class SimplePasswordHasher : IPasswordHasher
    {
        /// <summary>
        /// Hashes a password using SHA256
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The password hash</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentNullException(nameof(password));
            }
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="hashedPassword">The hashed password</param>
        /// <param name="providedPassword">The password to verify</param>
        /// <returns>True if the password is valid, false otherwise</returns>
        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(hashedPassword))
            {
                throw new ArgumentNullException(nameof(hashedPassword));
            }
            
            if (string.IsNullOrEmpty(providedPassword))
            {
                throw new ArgumentNullException(nameof(providedPassword));
            }
            
            var providedPasswordHash = HashPassword(providedPassword);
            return hashedPassword == providedPasswordHash;
        }
    }
}
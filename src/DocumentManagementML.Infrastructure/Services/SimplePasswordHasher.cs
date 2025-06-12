// -----------------------------------------------------------------------------
// <copyright file="SimplePasswordHasher.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 29, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Service for hashing and verifying passwords
// -----------------------------------------------------------------------------
using DocumentManagementML.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Cryptography;
using System.Text;

namespace DocumentManagementML.Infrastructure.Services
{
    /// <summary>
    /// Implementation of password hashing service
    /// </summary>
    public class SimplePasswordHasher : IPasswordHasher
    {
        private readonly ILogger<SimplePasswordHasher>? _logger;
        private const int SaltSize = 16; // 128 bits
        private const int KeySize = 32; // 256 bits
        private const int Iterations = 100000;
        private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
        private const char Delimiter = ':';
        
        /// <summary>
        /// Initializes a new instance of the SimplePasswordHasher class
        /// </summary>
        public SimplePasswordHasher()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SimplePasswordHasher class
        /// </summary>
        /// <param name="logger">Logger</param>
        public SimplePasswordHasher(ILogger<SimplePasswordHasher> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Hashes a password
        /// </summary>
        /// <param name="password">The password to hash</param>
        /// <returns>The hashed password</returns>
        public string HashPassword(string password)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password));
                }
                
                var salt = RandomNumberGenerator.GetBytes(SaltSize);
                var hash = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    Iterations,
                    HashAlgorithm,
                    KeySize);
                
                return string.Join(
                    Delimiter, 
                    Convert.ToBase64String(hash),
                    Convert.ToBase64String(salt),
                    Iterations,
                    HashAlgorithm);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error hashing password");
                throw new InvalidOperationException("Error hashing password", ex);
            }
        }
        
        /// <summary>
        /// Verifies a password against a hash
        /// </summary>
        /// <param name="password">The password to verify</param>
        /// <param name="hashedPassword">The hash to verify against</param>
        /// <returns>True if the password matches the hash, false otherwise</returns>
        public bool VerifyPassword(string password, string hashedPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    throw new ArgumentNullException(nameof(password));
                }
                
                if (string.IsNullOrEmpty(hashedPassword))
                {
                    throw new ArgumentNullException(nameof(hashedPassword));
                }
                
                // For backward compatibility with simple SHA256 hashing
                if (!hashedPassword.Contains(Delimiter))
                {
                    using (var sha256 = SHA256.Create())
                    {
                        var bytes = Encoding.UTF8.GetBytes(password);
                        var hashValue = sha256.ComputeHash(bytes);
                        var providedPasswordHash = Convert.ToBase64String(hashValue);
                        return hashedPassword == providedPasswordHash;
                    }
                }
                
                var parts = hashedPassword.Split(Delimiter);
                if (parts.Length != 4)
                {
                    _logger?.LogWarning("Invalid password hash format");
                    return false;
                }
                
                var hash = Convert.FromBase64String(parts[0]);
                var salt = Convert.FromBase64String(parts[1]);
                var iterations = int.Parse(parts[2]);
                var algorithm = new HashAlgorithmName(parts[3]);
                
                var hashToCheck = Rfc2898DeriveBytes.Pbkdf2(
                    password,
                    salt,
                    iterations,
                    algorithm,
                    hash.Length);
                
                return CryptographicOperations.FixedTimeEquals(hash, hashToCheck);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error verifying password");
                return false;
            }
        }
    }
}
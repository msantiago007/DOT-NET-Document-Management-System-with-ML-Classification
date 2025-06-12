// -----------------------------------------------------------------------------
// <copyright file="ApiTestFixture.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            May 1, 2025
// Last Modified:      May 1, 2025
// Version:            0.9.0
// Description:        Test fixture for API integration tests
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DocumentManagementML.IntegrationTests.TestHelpers;

namespace DocumentManagementML.IntegrationTests.TestFixtures
{
    /// <summary>
    /// Test fixture for API integration tests
    /// </summary>
    public class ApiTestFixture : DocumentManagementML.IntegrationTests.TestHelpers.CustomWebApplicationFactory, IDisposable
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // Use the correct content root for the test host
            var contentRoot = TestHelpers.WebApplicationFactoryContentRoot.GetProjectPath();
            builder.UseContentRoot(contentRoot);
            
            builder.ConfigureServices(services =>
            {
                // Remove the app's DbContext registration
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<DocumentManagementML.Infrastructure.Data.DocumentManagementDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add a new DbContext using an in-memory database for testing
                services.AddDbContext<DocumentManagementML.Infrastructure.Data.DocumentManagementDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                });

                // Build the service provider
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database context
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<DocumentManagementML.Infrastructure.Data.DocumentManagementDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<ApiTestFixture>>();

                    // Ensure the database is created
                    db.Database.EnsureCreated();

                    try
                    {
                        // Seed the database with test data
                        SeedTestData(db).Wait();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "An error occurred seeding the database with test data. Error: {Message}", ex.Message);
                    }
                }
            });
        }

        /// <summary>
        /// Seeds the test database with sample data
        /// </summary>
        /// <param name="context">The database context</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        private async Task SeedTestData(DocumentManagementML.Infrastructure.Data.DocumentManagementDbContext context)
        {
            // Add users
            var user1Id = Guid.NewGuid();
            var user2Id = Guid.NewGuid();
            
            context.Users.AddRange(
                new Domain.Entities.User
                {
                    UserId = user1Id,
                    Username = "testuser",
                    Email = "test@example.com",
                    PasswordHash = "AQAAAAIAAYagAAAAELBr9GsWa4U11qqCRVBtKVI2raDL8mZ4eS21QlBxZJ/sWvrqUlN1EMoFGQQJ8/YUPQ==", // password: Test123!
                    PasswordSalt = "8eCIInvRQGKt6n7XPb8qzg==",
                    FirstName = "Test",
                    LastName = "User",
                    IsActive = true,
                    IsAdmin = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Domain.Entities.User
                {
                    UserId = user2Id,
                    Username = "regularuser",
                    Email = "regular@example.com",
                    PasswordHash = "AQAAAAIAAYagAAAAELBr9GsWa4U11qqCRVBtKVI2raDL8mZ4eS21QlBxZJ/sWvrqUlN1EMoFGQQJ8/YUPQ==", // password: Test123!
                    PasswordSalt = "8eCIInvRQGKt6n7XPb8qzg==",
                    FirstName = "Regular",
                    LastName = "User",
                    IsActive = true,
                    IsAdmin = false,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                }
            );

            // Add document types
            var invoiceTypeId = Guid.NewGuid();
            var receiptTypeId = Guid.NewGuid();
            var contractTypeId = Guid.NewGuid();
            
            context.DocumentTypes.AddRange(
                new Domain.Entities.DocumentType
                {
                    DocumentTypeId = invoiceTypeId,
                    Name = "Invoice",
                    TypeName = "invoice",
                    Description = "Invoice documents",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Domain.Entities.DocumentType
                {
                    DocumentTypeId = receiptTypeId,
                    Name = "Receipt",
                    TypeName = "receipt",
                    Description = "Receipt documents",
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                },
                new Domain.Entities.DocumentType
                {
                    DocumentTypeId = contractTypeId,
                    Name = "Contract",
                    TypeName = "contract",
                    Description = "Contract documents",
                    IsActive = false, // Inactive
                    CreatedDate = DateTime.UtcNow,
                    LastModifiedDate = DateTime.UtcNow
                }
            );

            // Add documents
            var document1Id = Guid.NewGuid();
            var document2Id = Guid.NewGuid();
            
            context.Documents.AddRange(
                new Domain.Entities.Document
                {
                    DocumentId = document1Id,
                    DocumentName = "Invoice-2025-001",
                    DocumentTypeId = invoiceTypeId,
                    UploadedById = user1Id,
                    FileType = "pdf",
                    FileLocation = "/storage/invoices/invoice-2025-001.pdf",
                    FileSizeBytes = 1024,
                    CreatedDate = DateTime.UtcNow.AddDays(-5),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-5),
                    IsDeleted = false
                },
                new Domain.Entities.Document
                {
                    DocumentId = document2Id,
                    DocumentName = "Receipt-2025-001",
                    DocumentTypeId = receiptTypeId,
                    UploadedById = user2Id,
                    FileType = "pdf",
                    FileLocation = "/storage/receipts/receipt-2025-001.pdf",
                    FileSizeBytes = 512,
                    CreatedDate = DateTime.UtcNow.AddDays(-3),
                    LastModifiedDate = DateTime.UtcNow.AddDays(-3),
                    IsDeleted = false
                }
            );

            // Add metadata
            context.DocumentMetadata.AddRange(
                new Domain.Entities.DocumentMetadata
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document1Id,
                    MetadataKey = "InvoiceNumber",
                    MetadataValue = "INV-2025-001"
                },
                new Domain.Entities.DocumentMetadata
                {
                    Id = Guid.NewGuid(),
                    DocumentId = document1Id,
                    MetadataKey = "Amount",
                    MetadataValue = "1250.00"
                }
            );

            await context.SaveChangesAsync();
        }

        /// <summary>
        /// Creates an HTTP client with authentication token
        /// </summary>
        /// <param name="username">Username</param>
        /// <param name="password">Password</param>
        /// <returns>HTTP client with authentication</returns>
        public async Task<HttpClient> CreateAuthenticatedClientAsync(string username = "testuser", string password = "Test123!")
        {
            var client = CreateClient();
            
            var loginRequest = new
            {
                UsernameOrEmail = username,
                Password = password
            };
            
            var loginContent = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json"
            );

            var loginResponse = await client.PostAsync("/api/v1/auth/login", loginContent);
            loginResponse.EnsureSuccessStatusCode();
            
            var loginResponseContent = await loginResponse.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<LoginResponseDto>(loginResponseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
            
            return client;
        }
        
        /// <summary>
        /// DTO class for login response
        /// </summary>
        public class LoginResponseDto
        {
            public Guid UserId { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string AccessToken { get; set; }
            public string RefreshToken { get; set; }
            public DateTime TokenExpiration { get; set; }
            public List<string> Roles { get; set; }
        }
    }
}
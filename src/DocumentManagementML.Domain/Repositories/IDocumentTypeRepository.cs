// -----------------------------------------------------------------------------
// <copyright file="IDocumentTypeRepository.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            February 22, 2025
// Last Modified:      April 29, 2025
// Version:            0.9.0
// Description:        Repository interface for DocumentType entity that defines
//                     operations for document type management and classification.
// -----------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DocumentManagementML.Domain.Entities;

namespace DocumentManagementML.Domain.Repositories
{
    /// <summary>
    /// Repository interface for DocumentType entity
    /// </summary>
    public interface IDocumentTypeRepository : IRepository<DocumentType>
    {
        /// <summary>
        /// Gets all document types
        /// </summary>
        /// <returns>Collection of document types</returns>
        new Task<IEnumerable<DocumentType>> GetAllAsync();
        
        /// <summary>
        /// Gets all document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="take">Number of document types to take</param>
        /// <returns>Paged collection of document types</returns>
        Task<IEnumerable<DocumentType>> GetAllDocumentTypesAsync(int skip, int take);
        
        /// <summary>
        /// Gets all active document types with pagination
        /// </summary>
        /// <param name="skip">Number of document types to skip</param>
        /// <param name="take">Number of document types to take</param>
        /// <returns>Paged collection of active document types</returns>
        Task<IEnumerable<DocumentType>> GetActiveDocumentTypesAsync(int skip, int take);
        
        /// <summary>
        /// Gets all active document types
        /// </summary>
        /// <returns>Collection of active document types</returns>
        Task<IEnumerable<DocumentType>> GetActiveTypesAsync();
        
        /// <summary>
        /// Gets a document type by its identifier
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type</returns>
        new Task<DocumentType?> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Gets a document type by its name
        /// </summary>
        /// <param name="name">Document type name</param>
        /// <returns>Document type if found, null otherwise</returns>
        Task<DocumentType?> GetByNameAsync(string name);
        
        /// <summary>
        /// Gets the total count of document types
        /// </summary>
        /// <param name="activeOnly">If true, count only active document types</param>
        /// <returns>Total count of document types</returns>
        Task<int> GetDocumentTypeCountAsync(bool activeOnly = false);
        
        /// <summary>
        /// Adds a new document type
        /// </summary>
        /// <param name="documentType">Document type to add</param>
        /// <returns>Added document type</returns>
        new Task<DocumentType> AddAsync(DocumentType documentType);
        
        /// <summary>
        /// Updates an existing document type
        /// </summary>
        /// <param name="documentType">Document type to update</param>
        /// <returns>Updated document type</returns>
        new Task<DocumentType> UpdateAsync(DocumentType documentType);
        
        /// <summary>
        /// Deletes a document type
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>True if the document type was deleted, false otherwise</returns>
        new Task<bool> DeleteAsync(Guid id);
        
        /// <summary>
        /// Gets a document type by its identifier including related documents
        /// </summary>
        /// <param name="id">Document type identifier</param>
        /// <returns>Document type with documents if found, null otherwise</returns>
        Task<DocumentType?> GetWithDocumentsAsync(Guid id);
    }
}
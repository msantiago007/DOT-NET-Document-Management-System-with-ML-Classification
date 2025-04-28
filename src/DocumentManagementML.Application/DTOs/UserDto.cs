using System;
using System.ComponentModel.DataAnnotations;

namespace DocumentManagementML.Application.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        
        public string Username { get; set; } = string.Empty;
        
        public string Email { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public DateTime? LastLoginDate { get; set; }
    }
    
    public class UserCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;
    }
    
    public class UserUpdateDto
    {
        [StringLength(50, MinimumLength = 3)]
        public string? Username { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }
        
        [StringLength(100, MinimumLength = 6)]
        public string? Password { get; set; }
        
        public bool? IsActive { get; set; }
    }
} 
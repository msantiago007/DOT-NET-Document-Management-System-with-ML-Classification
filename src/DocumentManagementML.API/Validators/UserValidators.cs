// -----------------------------------------------------------------------------
// <copyright file="UserValidators.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        FluentValidation validators for User DTOs
// -----------------------------------------------------------------------------
using DocumentManagementML.API.Auth;
using DocumentManagementML.API.Controllers;
using DocumentManagementML.Application.DTOs;
using FluentValidation;
using System;
using System.Text.RegularExpressions;

namespace DocumentManagementML.API.Validators
{
    /// <summary>
    /// Static class containing validators for user-related DTOs
    /// </summary>
    public static class UserValidators
    {
        /// <summary>
        /// Validator for user creation DTOs
        /// </summary>
        public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
    {
        /// <summary>
        /// Initializes a new instance of the UserCreateDtoValidator class
        /// </summary>
        public UserCreateDtoValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");
            
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is not valid");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Must(BeStrongPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
        }
        
        private bool BeStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");
            
            return hasNumber.IsMatch(password) && 
                   hasUpperChar.IsMatch(password) && 
                   hasLowerChar.IsMatch(password) && 
                   hasSpecialChar.IsMatch(password);
        }
    }
    
    /// <summary>
    /// Validator for user update DTOs
    /// </summary>
    public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
    {
        /// <summary>
        /// Initializes a new instance of the UserUpdateDtoValidator class
        /// </summary>
        public UserUpdateDtoValidator()
        {
            RuleFor(x => x.Username)
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens")
                .When(x => !string.IsNullOrWhiteSpace(x.Username));
            
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Email is not valid")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));
            
            RuleFor(x => x.Password)
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Must(BeStrongPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")
                .When(x => !string.IsNullOrWhiteSpace(x.Password));
                
            RuleFor(x => x.FirstName)
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters")
                .When(x => x.FirstName != null);
                
            RuleFor(x => x.LastName)
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters")
                .When(x => x.LastName != null);
        }
        
        private bool BeStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");
            
            return hasNumber.IsMatch(password) && 
                   hasUpperChar.IsMatch(password) && 
                   hasLowerChar.IsMatch(password) && 
                   hasSpecialChar.IsMatch(password);
        }
    }
    
    /// <summary>
    /// Validator for login requests
    /// </summary>
    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        /// <summary>
        /// Initializes a new instance of the LoginRequestValidator class
        /// </summary>
        public LoginRequestValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage("Username or email is required");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required");
        }
    }
    
    /// <summary>
    /// Validator for password change requests
    /// </summary>
    public class ChangePasswordRequestValidator : AbstractValidator<UsersController.ChangePasswordRequest>
    {
        /// <summary>
        /// Initializes a new instance of the ChangePasswordRequestValidator class
        /// </summary>
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Current password is required");
            
            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("New password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Must(BeStrongPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
            
            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from current password");
        }
        
        private bool BeStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");
            
            return hasNumber.IsMatch(password) && 
                   hasUpperChar.IsMatch(password) && 
                   hasLowerChar.IsMatch(password) && 
                   hasSpecialChar.IsMatch(password);
        }
    }
    
    /// <summary>
    /// Validator for refresh token requests
    /// </summary>
    public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
    {
        /// <summary>
        /// Initializes a new instance of the RefreshTokenRequestValidator class
        /// </summary>
        public RefreshTokenRequestValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token is required");
            
            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token is required");
        }
    }
    
    /// <summary>
    /// Validator for register requests
    /// </summary>
    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        /// <summary>
        /// Initializes a new instance of the RegisterRequestValidator class
        /// </summary>
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Username is required")
                .MinimumLength(3).WithMessage("Username must be at least 3 characters")
                .MaximumLength(50).WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9_.-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");
            
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Email is not valid");
            
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required")
                .MinimumLength(8).WithMessage("Password must be at least 8 characters")
                .MaximumLength(100).WithMessage("Password cannot exceed 100 characters")
                .Must(BeStrongPassword).WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");
            
            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Confirm password is required")
                .Equal(x => x.Password).WithMessage("Passwords do not match");
            
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(50).WithMessage("First name cannot exceed 50 characters");
            
            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(50).WithMessage("Last name cannot exceed 50 characters");
        }
        
        private bool BeStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }
            
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSpecialChar = new Regex(@"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]+");
            
            return hasNumber.IsMatch(password) && 
                   hasUpperChar.IsMatch(password) && 
                   hasLowerChar.IsMatch(password) && 
                   hasSpecialChar.IsMatch(password);
        }
    }
}
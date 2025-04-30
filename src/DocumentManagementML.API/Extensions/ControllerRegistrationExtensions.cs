// -----------------------------------------------------------------------------
// <copyright file="ControllerRegistrationExtensions.cs" company="Marco Santiago">
//     Copyright (c) 2025 Marco Santiago. All rights reserved.
//     Proprietary and confidential.
// </copyright>
// -----------------------------------------------------------------------------
// Author(s):          Marco Santiago
// Created:            April 30, 2025
// Last Modified:      April 30, 2025
// Version:            0.9.0
// Description:        Extension methods for registering controllers with MVC
// -----------------------------------------------------------------------------
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DocumentManagementML.API.Extensions
{
    /// <summary>
    /// Extension methods for registering controllers with MVC
    /// </summary>
    public static class ControllerRegistrationExtensions
    {
        /// <summary>
        /// Use enhanced controllers instead of original controllers
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="useEnhanced">Whether to use enhanced controllers</param>
        /// <returns>Service collection</returns>
        public static IServiceCollection UseEnhancedControllers(this IServiceCollection services, bool useEnhanced = true)
        {
            if (!useEnhanced)
            {
                return services;
            }

            // Configure controller feature provider to filter controllers
            services.Configure<Microsoft.AspNetCore.Mvc.MvcOptions>(options =>
            {
                options.Conventions.Add(new ControllerConvention(useEnhanced));
            });

            return services;
        }

        /// <summary>
        /// MVC convention to filter controllers based on naming
        /// </summary>
        private class ControllerConvention : IControllerModelConvention
        {
            private readonly bool _useEnhanced;

            public ControllerConvention(bool useEnhanced)
            {
                _useEnhanced = useEnhanced;
            }

            public void Apply(ControllerModel controller)
            {
                // Determine if this is an enhanced controller by name
                var isEnhanced = controller.ControllerType.Name.StartsWith("Enhanced");
                
                // If we want enhanced controllers, suppress non-enhanced ones
                // If we want regular controllers, suppress enhanced ones
                if (isEnhanced != _useEnhanced)
                {
                    controller.Application.Controllers.Remove(controller);
                }
            }
        }
    }
}
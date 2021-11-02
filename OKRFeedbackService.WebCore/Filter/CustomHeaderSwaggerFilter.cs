﻿using Microsoft.OpenApi.Models;

namespace OKRFeedbackService.WebCore.Filter
{
    public class CustomHeaderSwaggerFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
    {
        public void Apply(OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
        {
            operation.Parameters ??= new System.Collections.Generic.List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Token",
                In = ParameterLocation.Header,
                Description = "access token",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString("Bearer ")
                }
            });
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "TenantId",
                In = ParameterLocation.Header,
                Description = "Domain Tenant",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Default = new Microsoft.OpenApi.Any.OpenApiString(string.Empty),
                }
            });
        }
    }
}

﻿using HRM_BACKEND_VSA.Domains.Staffs.Staff_Bio;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace HRM_SK.Extensions
{


    public static class SwaggerDoc
    {

        public static List<Type> enumTypes = new List<Type>
        {
            typeof(StaffListFilter),
        };

        public static void MapEnumsToString(SwaggerGenOptions options, IEnumerable<Type> enumTypes)
        {
            foreach (var enumType in enumTypes)
            {
                if (enumType.IsEnum)
                {
                    options.MapType(enumType, () => new OpenApiSchema
                    {
                        Type = "string",
                        Enum = Enum.GetNames(enumType)
                            .Select(name => new OpenApiString(name))
                            .ToList<IOpenApiAny>()
                    });
                }
            }
        }

        public class SwaggerFileOperationFilter : IOperationFilter
        {
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                var fileParams = context.ApiDescription.ParameterDescriptions
                    .Where(x => x.ModelMetadata.ElementType == typeof(IFormFile))
                    .Select(x => x.Name);

                if (fileParams.Any())
                {
                    operation.RequestBody = new OpenApiRequestBody
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            ["multipart/form-data"] = new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = fileParams.ToDictionary(param => param, param => new OpenApiSchema { Type = "string", Format = "binary" }),
                                    Required = fileParams.ToHashSet()
                                }
                            }
                        }
                    };
                }
            }
        }


        public static void OpenAuthentication(SwaggerGenOptions option)
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "HRM-SK API", Version = "v1" });

            MapEnumsToString(option, enumTypes);

            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.OperationFilter<SwaggerFileOperationFilter>();
            option.SchemaFilter<EnumSchemaFilter>();
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });
        }


    }

    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema model, SchemaFilterContext context)
        {
            if (context.Type.IsEnum)
            {
                model.Enum.Clear();
                Enum.GetNames(context.Type)
                    .ToList()
                    .ForEach(name => model.Enum.Add(new OpenApiString($"{name}")));
            }
        }
    }
}
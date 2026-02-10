using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileService.WebAPI.Filters;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.ApiDescription.ParameterDescriptions
            .Where(p => p.ModelMetadata?.ModelType == typeof(IFormFile))
            .ToList();

        if (!fileParameters.Any())
            return;

        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>(),
                        Required = new HashSet<string>()
                    }
                }
            }
        };

        var schema = operation.RequestBody.Content["multipart/form-data"].Schema;

        foreach (var param in context.ApiDescription.ParameterDescriptions)
        {
            if (param.ModelMetadata?.ModelType == typeof(IFormFile))
            {
                schema.Properties[param.Name] = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
                schema.Required.Add(param.Name);
            }
            else if (param.Source?.Id == "Form")
            {
                var propertyType = param.ModelMetadata?.ModelType;

                if (propertyType == typeof(Guid) || propertyType == typeof(Guid?))
                {
                    schema.Properties[param.Name] = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "uuid",
                        Nullable = propertyType == typeof(Guid?)
                    };
                }
                else if (propertyType == typeof(string))
                {
                    schema.Properties[param.Name] = new OpenApiSchema
                    {
                        Type = "string"
                    };
                }
                else if (propertyType == typeof(int) || propertyType == typeof(int?))
                {
                    schema.Properties[param.Name] = new OpenApiSchema
                    {
                        Type = "integer",
                        Format = "int32",
                        Nullable = propertyType == typeof(int?)
                    };
                }
            }
        }

        // Remove parameters from query/path that are now in request body
        var parametersToRemove = operation.Parameters
            .Where(p => context.ApiDescription.ParameterDescriptions
                .Any(pd => (pd.Source?.Id == "Form" || pd.ModelMetadata?.ModelType == typeof(IFormFile))
                          && pd.Name == p.Name))
            .ToList();

        foreach (var parameter in parametersToRemove)
        {
            operation.Parameters.Remove(parameter);
        }
    }
}
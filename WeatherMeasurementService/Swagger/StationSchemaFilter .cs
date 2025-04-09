using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WeatherMeasurementService.Swagger
{
    /// <summary>
    /// Schema filter to customize the Swagger schema for specific properties.
    /// </summary>
    public class MeasurementTypeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(string) && context.MemberInfo?.Name == "MeasurementType")
            {
                schema.Enum =
            [
                new OpenApiString("air_temperature"),
                new OpenApiString("water_temperature"),
                new OpenApiString("barometric_pressure_qfe"),
                new OpenApiString("humidity")
            ];
            }
        }
    }

    /// <summary>
    /// Schema filter to customize the Swagger schema for specific properties.
    /// </summary>
    public class StationSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(string) && context.MemberInfo?.Name == "Station")
            {
                schema.Enum =
            [
                new OpenApiString("tiefenbrunnen"),
                new OpenApiString("mythenquai")
            ];
            }
        }
    }
}

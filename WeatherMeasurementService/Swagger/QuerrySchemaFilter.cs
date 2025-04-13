using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace WeatherMeasurementService.Swagger
{
    /// <summary>
    /// Combined schema filter for customizing Swagger querry schemas:
    /// - Sets enum values for "MeasurementType" and "Station".
    /// - Sets default example for DateTime properties "Start" and "End".
    /// </summary>
    public class QuerrySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Set enum for "MeasurementType" property
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

            // Set enum for "Station" property
            if (context.Type == typeof(string) && context.MemberInfo?.Name == "Station")
            {
                schema.Enum =
                [
                    new OpenApiString("tiefenbrunnen"),
                    new OpenApiString("mythenquai")
                ];
            }

            // Set default example for DateTime properties "Start" and "End"
            if (context.Type == typeof(DateTime) &&
                (context.MemberInfo?.Name == "Start" || context.MemberInfo?.Name == "End"))
            {
                string defaultDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                schema.Example = new OpenApiString(defaultDate);
            }
        }
    }
}

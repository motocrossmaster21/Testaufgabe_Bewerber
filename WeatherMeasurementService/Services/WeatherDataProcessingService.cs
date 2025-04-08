namespace WeatherMeasurementService.Services
{
    using System;
    using System.Threading.Tasks;
    using WeatherMeasurementService.Data;
    using WeatherMeasurementService.Dtos;
    using WeatherMeasurementService.ExternalApiModels;
    using Microsoft.Extensions.Logging;
    using System.Text.Json;


    /// <summary>
    /// This service processes external weather data and persists it in the internal database.
    /// </summary>
    public class WeatherDataProcessingService(IWeatherDataRepository repository, ILogger<WeatherDataProcessingService> logger)
    {
        private readonly IWeatherDataRepository _repository = repository;
        private readonly ILogger<WeatherDataProcessingService> _logger = logger;

        /// <summary>
        /// Processes external weather data and saves valid measurements into the database.
        /// </summary>
        /// <param name="apiResponse">The response object from the external API.</param>
        public async Task ProcessWeatherDataAsync(WeatherApiResponse apiResponse)
        {
            if (!apiResponse.Ok || !string.IsNullOrEmpty(apiResponse.Message))
            {
                _logger.LogError("API response skipped. Ok: {Ok}, Message: {Message}", apiResponse.Ok, apiResponse.Message);
                return;
            }

            // Iterate over each record in the API response
            foreach (var record in apiResponse.Result)
            {
                if (string.IsNullOrEmpty(record.Station))
                {
                    // Log and skip if station is missing
                    _logger.LogWarning("Record skipped due to missing station information.");
                    continue;
                }

                // Optional: check if the station is one of the allowed ones ("tiefenbrunnen" or "mythenquai")
                var stationName = record.Station.ToLowerInvariant();
                if (stationName != "tiefenbrunnen" && stationName != "mythenquai")
                {
                    _logger.LogInformation("Record for station {Station} skipped.", record.Station);
                    continue;
                }

                // Process and validate each measurement within the record
                foreach (var (measurementType, measurementValue) in record.Values)
                {
                    // Skip timestamp_cet
                    if (measurementType.Equals("timestamp_cet", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var jsonValue = measurementValue.Value;
                    // Check if the JSON element is null
                    if (jsonValue.ValueKind == JsonValueKind.Null)
                    {
                        _logger.LogWarning("Measurement of type {MeasurementType} for station {Station} is null.",
                            measurementType, record.Station);
                        continue;
                    }

                    // Try to get a numeric value
                    if (!jsonValue.TryGetDouble(out double numericValue))
                    {
                        if (jsonValue.ValueKind == JsonValueKind.String)
                        {
                            var strValue = jsonValue.GetString();
                            if (!double.TryParse(strValue, out numericValue))
                            {
                                _logger.LogWarning("Measurement of type {MeasurementType} for station {Station} did not yield a valid numeric value from string: {StrValue}.",
                                    measurementType, record.Station, strValue);
                                continue;
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Measurement of type {MeasurementType} for station {Station} did not yield a valid numeric value.",
                                measurementType, record.Station);
                            continue;
                        }
                    }

                    // Create a DTO for valid measurement
                    var dto = new CreateMeasurementDto
                    {
                        StationName = record.Station,
                        TypeName = measurementType,
                        Unit = measurementValue.Unit ?? "N/A",
                        Value = numericValue,
                        Status = measurementValue.Status ?? string.Empty,
                        TimestampUtc = record.Timestamp // Use top-level timestamp;
                    };

                    _logger.LogDebug("Adding measurement: Station {Station}, Type {Type}, Value {Value}",
                        dto.StationName, dto.TypeName, dto.Value);

                    await _repository.AddMeasurementAsync(dto).ConfigureAwait(false);
                }
            }

            await _repository.SaveChangesAsync().ConfigureAwait(false);
        }
    }
}

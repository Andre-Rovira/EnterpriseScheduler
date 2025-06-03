using TimeZoneConverter;
using System.ComponentModel.DataAnnotations;

namespace EnterpriseScheduler.Models.Validation;

public class ValidTimeZoneAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is null)
        {
            return ValidationResult.Success;
        }

        var timeZoneId = value.ToString();
        if (string.IsNullOrEmpty(timeZoneId))
        {
            return ValidationResult.Success;
        }

        try
        {
            // Try to convert the timezone string to a TimeZoneInfo
            if (TZConvert.TryGetTimeZoneInfo(timeZoneId, out _))
            {
                return ValidationResult.Success;
            }
        }
        catch
        {
            // If conversion fails, the timezone is invalid
        }

        return new ValidationResult($"'{timeZoneId}' is not a valid timezone identifier. Please use a valid IANA timezone (e.g., 'America/New_York', 'Europe/London', 'Asia/Tokyo').");
    }
}

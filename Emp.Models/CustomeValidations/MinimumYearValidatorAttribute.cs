using System.ComponentModel.DataAnnotations;

namespace Emp.Api.CustomeValidations
{
    public class MinimumYearValidatorAttribute:ValidationAttribute
    {
            //private readonly int _minimumYear;
            //public MinimumYearValidatorAttribute(int minimumYear)
            //{
            //    _minimumYear = minimumYear;
            //}
            //protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
            //{
            //    if (value is DateOnly dateValue)
            //    {
            //        if (dateValue.Year < _minimumYear)
            //        {
            //            return new ValidationResult($"The year must be at least {_minimumYear}.");
            //        }
            //    }
            //    return ValidationResult.Success;
            //}
            protected override ValidationResult? IsValid(object? value, ValidationContext validationResult)
            {
                if (value != null)
                {
                    DateTime date = (DateTime)value;
                    if (date.Year < 2000)
                    {
                        return new ValidationResult("Minimum year is 2000.");
                    }
                    else
                    {
                        return ValidationResult.Success;
                    }
                }
                else
                {
                    return null;
                }
            }
        
    }
}

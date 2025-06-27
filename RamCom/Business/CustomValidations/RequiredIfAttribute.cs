using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.CustomValidations
{
    public class RequiredIfAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly string _conditionProperty;
        public string ConditionValue { get; set; } = string.Empty;

        public RequiredIfAttribute(string conditionProperty)
        {
            _conditionProperty = conditionProperty;
        }

        // Client-side validation rule
        public void AddValidation(ClientModelValidationContext context)
        {
            // Add custom data-val attribute
            context.Attributes.Add("data-val", "true");
            context.Attributes.Add("data-val-requiredif", ErrorMessage);
            context.Attributes.Add("data-val-requiredif-conditionproperty", _conditionProperty);

            context.Attributes.Add("data-val-requiredif-conditionvalue", ConditionValue);
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var conditionProperty = validationContext.ObjectType.GetProperty(_conditionProperty);
            if (conditionProperty == null)
            {
                return new ValidationResult($"Unknown property: {_conditionProperty}");
            }

            string currentValue = value != null ? value.ToString() : null;
            var conditionPropertyValue = Convert.ToString(conditionProperty.GetValue(validationContext.ObjectInstance));
            if (conditionPropertyValue == ConditionValue && string.IsNullOrWhiteSpace(currentValue))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}

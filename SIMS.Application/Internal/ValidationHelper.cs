using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace SIMS.Application.Internal
{
    internal static class ValidationHelper
    {
        public static IReadOnlyList<string> Validate(object? instance)
        {
            var errors = new List<string>();
            ValidateInternal(instance, errors, new HashSet<object>(ReferenceEqualityComparer.Instance));
            return errors;
        }

        private static void ValidateInternal(object? instance, List<string> errors, HashSet<object> visited)
        {
            if (instance is null)
            {
                return;
            }

            var type = instance.GetType();
            if (IsSimple(type) || !visited.Add(instance))
            {
                return;
            }

            var results = new List<ValidationResult>();
            Validator.TryValidateObject(instance, new ValidationContext(instance), results, validateAllProperties: true);

            foreach (var result in results)
            {
                var message = result.ErrorMessage;
                if (!string.IsNullOrWhiteSpace(message) && !errors.Contains(message))
                {
                    errors.Add(message);
                }
            }

            foreach (var property in type.GetProperties())
            {
                if (property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                var propertyType = property.PropertyType;
                if (IsSimple(propertyType))
                {
                    continue;
                }

                if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                {
                    continue;
                }

                object? value;
                try
                {
                    value = property.GetValue(instance);
                }
                catch
                {
                    continue;
                }

                ValidateInternal(value, errors, visited);
            }
        }

        private static bool IsSimple(Type type)
        {
            var target = Nullable.GetUnderlyingType(type) ?? type;
            return target.IsPrimitive
                || target.IsEnum
                || target == typeof(string)
                || target == typeof(decimal)
                || target == typeof(DateTime)
                || target == typeof(DateTimeOffset)
                || target == typeof(TimeSpan)
                || target == typeof(Guid);
        }
    }
}

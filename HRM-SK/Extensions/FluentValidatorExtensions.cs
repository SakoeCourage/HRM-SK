using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace HRM_SK.Extensions
{
    public static class FluentValidatorExtensions
    {
        public static bool IsImageFile(IFormFile file)
        {
            if (file == null) return true;

            var permittedExtensions = new[] { ".jpg", ".jpeg", ".png" };

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            return !string.IsNullOrEmpty(extension) && permittedExtensions.Contains(extension);
        }

        public static IRuleBuilderOptions<T, TProperty> IsNulOrUnique<T, TProperty>(
        this IRuleBuilder<T, TProperty> ruleBuilder,
        DbContext dbContext,
        Expression<Func<T, TProperty>> uniqueColumnExpression,
        TProperty? exceptValue = default) where T : class
        {
            var dbSet = dbContext.Set<T>();

            return ruleBuilder.MustAsync(async (model, value, cancellationToken) =>
            {
                if (value == null) return true;

                var propertyName = ((MemberExpression)uniqueColumnExpression.Body).Member.Name;

                var parameter = Expression.Parameter(typeof(T), "e");
                var property = Expression.Property(parameter, propertyName);
                var valueExpression = Expression.Constant(value);
                var equalsExpression = Expression.Equal(property, valueExpression);

                var lambda = Expression.Lambda<Func<T, bool>>(equalsExpression, parameter);

                var existingEntity = await dbSet.FirstOrDefaultAsync(lambda, cancellationToken);

                // If there's no match in the database, it's unique
                if (existingEntity == null) return true;

                // If the value matches the except value, it's considered unique
                if (exceptValue != null && value.Equals(exceptValue))
                {
                    return true;
                }

                return false;
            }).WithMessage("{PropertyName} must be unique.");
        }

    }
}
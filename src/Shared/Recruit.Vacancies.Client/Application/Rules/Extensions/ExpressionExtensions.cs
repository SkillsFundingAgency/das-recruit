using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Esfa.QA.Core.Extensions
{
    public static class ExpressionExtensions
    {
        public static string GetQualifiedFieldId(this Expression<Func<string>> property)
        {
            var fieldId = string.Empty;

            var memberExpression = property.Body as MemberExpression;
            while (memberExpression != null && memberExpression.Member.MemberType == MemberTypes.Property)
            {
                fieldId = memberExpression.Member.Name + (fieldId != string.Empty ? "." : string.Empty) + fieldId;
                memberExpression = memberExpression.Expression as MemberExpression;
            }

            return fieldId;
        }
    }
}

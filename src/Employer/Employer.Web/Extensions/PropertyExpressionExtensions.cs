using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class PropertyExpressionExtensions
    {
        public static string GetPropertyName<T>(this Expression<Func<T, object>> propertyExpression)
        {
            var getMemberExp = new Func<Expression, MemberExpression>(toUnwrap =>
            {
                if (toUnwrap is UnaryExpression expression)
                {
                    return expression.Operand as MemberExpression;
                }

                return toUnwrap as MemberExpression;
            });

            var memberNames = new Stack<string>();

            var memberExp = getMemberExp(propertyExpression.Body);

            while (memberExp != null)
            {
                memberNames.Push(memberExp.Member.Name);
                memberExp = getMemberExp(memberExp.Expression);
            }

            return string.Join(".", memberNames);
        }
    }
}

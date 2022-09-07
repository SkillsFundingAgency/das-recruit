using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class EntityValidationErrorExtensions
    {
        private const char PositionStart = '[';
        private const char PositionEnd = ']';

        public static int? GetIndexPosition(this EntityValidationError error)
        {
            var start = error.PropertyName.IndexOf(PositionStart) + 1;

            if (start == 0)
            {
                return null;
            }

            var end = error.PropertyName.IndexOf(PositionEnd, start);

            if (end == -1)
            {
                return null;
            }

            if (int.TryParse(error.PropertyName.Substring(start, end - start), out var position))
            {
                return position;
            }

            return null;
        }
    }
}


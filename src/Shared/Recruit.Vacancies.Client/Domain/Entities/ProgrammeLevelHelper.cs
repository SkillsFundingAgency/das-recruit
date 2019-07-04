using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public static class ProgrammeLevelHelper
    {
        public static bool TryRemapFromInt(int value, out ProgrammeLevel result)
        {
            switch (value)
            {
                case 5:
                    value = (int)ProgrammeLevel.Higher;
                    break;

                case 7:
                    value = (int)ProgrammeLevel.Degree;
                    break;
            }
            if (Enum.IsDefined(typeof(ProgrammeLevel), value))
            {
                result = (ProgrammeLevel)value;
                return true;
            }
            result = ProgrammeLevel.Unknown;
            return false;
        }

        public static ProgrammeLevel RemapFromInt(int value)
        {
            if (TryRemapFromInt(value, out ProgrammeLevel result))
                return result;
            throw new ArgumentException($"Cannot convert from int {value} to {nameof(ProgrammeLevel)}");
        }
    }
}
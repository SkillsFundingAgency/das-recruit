﻿using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    public interface IReferenceDataReader
    {
        Task<T> GetReferenceData<T>() where T : class, IReferenceDataItem;
    }
}

﻿using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.HardMocks
{
    internal class VacancyOrchestratorTestData
    {
        public const string AccountLegalEntityPublicHashedId123 = "ABC123";
        public const string AccountLegalEntityPublicHashedId456 = "ABC456";

        internal static Vacancy GetPart1CompleteVacancy()
        {
            return new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId123,
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                Title = "has a value",
                NumberOfPositions = 1,
                EmployerLocation = new Address { Postcode = "has a value" },
                ShortDescription = "has a value",
                ProgrammeId = "has a value",
                Wage = new Wage {Duration = 1, WageType = WageType.FixedWage },
                LegalEntityName = "LEGAL ENTITY NAME 123",
                EmployerNameOption = EmployerNameOption.RegisteredName,
                StartDate = DateTime.Now
            };
        }

        internal static VacancyUser GetVacancyUser()
        {
            return new VacancyUser
            {
                Email = "scotty@scotty.com",
                Name = "scott",
                UserId = "scott"
            };
        }

        internal static EmployerProfile GetEmployerProfile(string accountLegalEntityPublicHashedId123)
        {
            return new EmployerProfile
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId123
            };
        }

        internal static EmployerEditVacancyInfo GetEmployerEditVacancyInfo()
        {
            return new EmployerEditVacancyInfo
            {
                LegalEntities = new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        Name = "LEGAL ENTITY NAME 123",
                        AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId123,
                        Address = new Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo.Address
                        {
                            AddressLine1 = "this is a value",
                            AddressLine2 = "this is a value",
                            AddressLine3 = "this is a value",
                            AddressLine4 = "this is a value",
                            Postcode = "this is a value"
                        }
                    },
                    new LegalEntity
                    {
                        Name = "LEGAL ENTITY NAME 456",
                        AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId456,
                        Address = new Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo.Address
                        {
                            AddressLine1 = "this is a value",
                            AddressLine2 = "this is a value",
                            AddressLine3 = "this is a value",
                            AddressLine4 = "this is a value",
                            Postcode = "this is a value"
                        }
                    }
                }
            };
        }

        internal static GetAddressesListResponse GetAddressesListResponse()
        {
            return new GetAddressesListResponse
            {
                Addresses = new List<GetAddressesListItem> {
                    new GetAddressesListItem {
                        Postcode = "NN1 4YH",
                        PostTown = "Northampton",
                        County = "Northamptionshire",
                        Street ="North street",
                        House ="12"
                    },
                    new GetAddressesListItem {
                        Postcode = "MK2 4YH",
                        PostTown = "Bedford",
                        County = "Bedfordshire",
                        Street ="South street",
                        House ="13"
                    }
                }
            };
        }
    }
}

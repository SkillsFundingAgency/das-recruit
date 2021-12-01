using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.HardMocks
{
    internal class VacancyOrchestratorTestData
    {
        public const string AccountLegalEntityPublicHashedId123 = "ABC123";
        public const string AccountLegalEntityPublicHashedId456 = "ABC456";
        public const long TrainingProviderUkprn = 12345;

        internal static Vacancy GetPart1CompleteVacancy()
        {
            return new Vacancy
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Id = Guid.Parse("84af954e-5baf-4942-897d-d00180a0839e"),
                AccountLegalEntityPublicHashedId = AccountLegalEntityPublicHashedId123,
                LegalEntityName = "LEGAL ENTITY NAME 123",
                EmployerLocation = new Address { Postcode = "has a value" },
                EmployerNameOption = EmployerNameOption.RegisteredName,
                NumberOfPositions = 1,
                OwnerType = OwnerType.Provider,
                ProgrammeId = "has a value",
                ShortDescription = "has a value",
                StartDate = DateTime.Now,
                Status = VacancyStatus.Rejected,
                Title = "has a value",
                TrainingProvider = new TrainingProvider
                {
                    Ukprn = TrainingProviderUkprn
                },
                Wage = new Wage { Duration = 1, WageType = WageType.FixedWage }
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

        internal static EmployerInfo GetEmployerInfo()
        {
            return new EmployerInfo
            {
                EmployerAccountId = "EMPLOYER ACCOUNT ID",
                Name = "LEGAL ENTITY NAME 123",
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

        internal static ProviderEditVacancyInfo GetProviderEditVacancyInfo()
        {
            return new ProviderEditVacancyInfo
            {
                Employers = new List<EmployerInfo>
                {
                    GetEmployerInfo()
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

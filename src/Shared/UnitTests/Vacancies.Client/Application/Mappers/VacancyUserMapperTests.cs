using System;
using Esfa.Recruit.Vacancies.Client.Application.Mappers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.Mappers;

public class VacancyUserMapperTests
{
    [Fact]
    public void MapFromUser()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            IdamsUserId = Guid.NewGuid().ToString(),
            Email = "test@test.com",
            Name = "John Smith"
        };

        var vacancyUser = VacancyUserMapper.MapFromUser(user);
        vacancyUser.UserId.Should().Be(user.IdamsUserId);
        vacancyUser.Email.Should().Be(user.Email);
        vacancyUser.Name.Should().Be(user.Name);
    }
}
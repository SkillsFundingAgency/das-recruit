using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Xunit;
using EmployerWeb = Employer.Web;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.NewVacancy
{
    public class IndexEditModelTests
    {
        [Fact]
        public void ShouldErrorIfTitleIsNotSpecified()
        {
            var vm = new EmployerWeb.ViewModels.NewVacancy.IndexViewModel
            {
                Title = null
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            // Act
            var valid = Validator.TryValidateObject(vm, context, result, true);
            
            Assert.False(valid);
            Assert.Single(result);
            Assert.Equal("The Title field is required.", result.Single(r => r.MemberNames.Single() == "Title").ErrorMessage);
        }

        [Fact]
        public void ShouldBeValidIfTitleIsSpecified()
        {
            var vm = new EmployerWeb.ViewModels.NewVacancy.IndexViewModel
            {
                Title = "some text"
            };

            var context = new ValidationContext(vm, null, null);
            var result = new List<ValidationResult>();

            // Act
            var valid = Validator.TryValidateObject(vm, context, result, false);

            Assert.True(valid);
        }
    }
}

using SFA.DAS.Recruit.Api.Extensions;

namespace SFA.DAS.Recruit.Api.UnitTests.Extensions;

public class StringExtensionsTests
{
    [Test]
    public void RegexReplaceWithGroups_Does_Not_Change_Text_On_No_Match()
    {
        // arrange
        const string input = "Value";
        
        // act
        string result = input.RegexReplaceWithGroups("Non_Matching_Pattern", "Replacement_Text");
        
        // assert
        result.Should().Be(input);
    }
    
    [Test]
    public void RegexReplaceWithGroups_Changes_Simple_Text_On_Match()
    {
        // arrange
        const string input = "Value";
        
        // act
        string result = input.RegexReplaceWithGroups("Value", "Replacement_Text");
        
        // assert
        result.Should().Be("Replacement_Text");
    }
    
    [Test]
    public void RegexReplaceWithGroups_Changes_Text_On_Regex_Match()
    {
        // arrange
        const string input = "Value[2]";
        
        // act
        string result = input.RegexReplaceWithGroups(@"Value\[\d\]", "Replacement_Text");
        
        // assert
        result.Should().Be("Replacement_Text");
    }
    
    [Test]
    public void RegexReplaceWithGroups_Changes_Text_On_Regex_Match_With_Group_Substitution()
    {
        // arrange
        const string input = "Value[2][1]";
        
        // act
        string result = input.RegexReplaceWithGroups(@"Value\[(?<1>\d)\]\[(?<2>\d)\]", "Replacement_Text[{0}][{1}]");
        
        // assert
        result.Should().Be("Replacement_Text[2][1]");
    }
}
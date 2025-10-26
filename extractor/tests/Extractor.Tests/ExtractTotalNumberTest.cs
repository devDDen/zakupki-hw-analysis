using Extractor.pages;

namespace Extractor.Tests;

public class ExtractTotalNumberTest
{
    [Fact]
    public void SmallTotalNumber()
    {
        Assert.Equal(123, ExtendedSearch.ExtractTotalNumber("123 записей"));
    }

    [Fact]
    public void BigTotalNumber()
    {
        Assert.Equal(1400, ExtendedSearch.ExtractTotalNumber("более 1 400 записей"));
    }
}
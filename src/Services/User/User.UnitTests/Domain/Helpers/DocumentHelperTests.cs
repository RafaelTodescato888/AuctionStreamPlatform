using User.Domain.Helpers;

namespace User.UnitTests.Domain.Helpers;

public class DocumentHelperTests
{
    #region [CPF]

    [Theory]
    [InlineData("52998224725")]
    [InlineData("529.982.247-25")]
    [InlineData("11144477735")]
    public void IsValidCpf_Should_Return_True_When_Cpf_Is_Valid(string cpf)
    {
        var result = cpf.IsValidCpf();

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("123456789012")]
    [InlineData("11111111111")]
    [InlineData("00000000000")]
    [InlineData("52998224724")]
    [InlineData("529.982.247-24")]
    public void IsValidCpf_Should_Return_False_When_Cpf_Is_Invalid(string? cpf)
    {
        var result = cpf.IsValidCpf();

        Assert.False(result);
    }

    [Fact]
    public void IsValidCpf_Should_Return_False_When_Cpf_Has_Only_NonNumeric_Characters()
    {
        string cpf = "abcdefghijk";

        var result = cpf.IsValidCpf();

        Assert.False(result);
    }

    [Fact]
    public void IsValidCpf_Should_Return_True_When_Cpf_Has_Additional_NonNumeric_Characters()
    {
        string cpf = "529.982/247-25";

        var result = cpf.IsValidCpf();

        Assert.True(result);
    }

    #endregion

    #region [CNPJ]

    [Theory]
    [InlineData("11222333000181")]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11444777000161")]
    public void IsValidCnpj_Should_Return_True_When_Cnpj_Is_Valid(string cnpj)
    {
        var result = cnpj.IsValidCnpj();

        Assert.True(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("123")]
    [InlineData("1234567890123")]
    [InlineData("123456789012345")]
    [InlineData("11111111111111")]
    [InlineData("00000000000000")]
    [InlineData("11222333000180")]
    [InlineData("11.222.333/0001-80")]
    public void IsValidCnpj_Should_Return_False_When_Cnpj_Is_Invalid(string? cnpj)
    {
        var result = cnpj.IsValidCnpj();

        Assert.False(result);
    }

    [Fact]
    public void IsValidCnpj_Should_Return_False_When_Cnpj_Has_Only_NonNumeric_Characters()
    {
        string cnpj = "abcdefghijklmn";

        var result = cnpj.IsValidCnpj();

        Assert.False(result);
    }

    [Fact]
    public void IsValidCnpj_Should_Return_True_When_Cnpj_Has_Additional_NonNumeric_Characters()
    {
        string cnpj = "11.222.333/0001-81";

        var result = cnpj.IsValidCnpj();

        Assert.True(result);
    }

    #endregion
}
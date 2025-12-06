using ClearBank.DeveloperTest.Services.PaymentRules;
using ClearBank.DeveloperTest.Types;
using FluentAssertions;
using Xunit;

namespace ClearBank.DeveloperTest.Services.Tests
{
    public class BacsPaymentRuleTests
    {
        [Fact]
        public void IsValid_WhenAccountIsNull_ShouldReturnFalse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var sut = CreateSut();

            // Act
            var result = sut.IsValid(null, request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenAccountDoesNotAcceptPaymentScheme_ShouldReturnFalse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var account = new Account
            {
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps
            };

            var sut = CreateSut();

            // Act
            var result = sut.IsValid(account, request);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsValid_WhenValidationPasses_ShouldReturnTrue()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var account = new Account
            {
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            var sut = CreateSut();

            // Act
            var result = sut.IsValid(account, request);

            // Assert
            result.Should().BeTrue();
        }

        private BacsPaymentRule CreateSut()
        {
            return new BacsPaymentRule();
        }
    }
}
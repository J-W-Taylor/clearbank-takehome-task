using ClearBank.DeveloperTest.Services.PaymentRules;
using ClearBank.DeveloperTest.Types;
using FluentAssertions;
using Xunit;

namespace ClearBank.DeveloperTest.Services.Tests
{
    public class ChapsPaymentRuleTests
    {
        [Fact]
        public void IsValid_WhenAccountIsNull_ShouldReturnFalse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                Amount = 100,
                PaymentScheme = PaymentScheme.Chaps
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
                PaymentScheme = PaymentScheme.Chaps
            };

            var account = new Account
            {
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.FasterPayments,
                Status = AccountStatus.Live
            };

            var sut = CreateSut();

            // Act
            var result = sut.IsValid(account, request);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(AccountStatus.Disabled)]
        [InlineData(AccountStatus.InboundPaymentsOnly)]
        public void IsValid_WhenAccountStatusIsNotLive_ShouldReturnFalse(AccountStatus accountStatus)
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                Amount = 100,
                PaymentScheme = PaymentScheme.Chaps
            };

            var account = new Account
            {
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Status = accountStatus
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
                PaymentScheme = PaymentScheme.Chaps
            };

            var account = new Account
            {
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps,
                Status = AccountStatus.Live
            };

            var sut = CreateSut();

            // Act
            var result = sut.IsValid(account, request);

            // Assert
            result.Should().BeTrue();
        }

        private ChapsPaymentRule CreateSut()
        {
            return new ChapsPaymentRule();
        }
    }
}
using ClearBank.DeveloperTest.Data;
using ClearBank.DeveloperTest.Services.PaymentRules;
using ClearBank.DeveloperTest.Types;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using Xunit;

namespace ClearBank.DeveloperTest.Services.Tests
{
    public class PaymentServiceTests
    {
        private readonly Mock<IAccountDataStore> _primary;
        private readonly Mock<IAccountDataStore> _backup;
        private readonly List<IPaymentRule> _rules;

        public PaymentServiceTests()
        {
            _primary = new Mock<IAccountDataStore>();
            _backup = new Mock<IAccountDataStore>();
            _rules = new List<IPaymentRule>();
        }

        [Fact]
        public void MakePayment_WhenPrimaryFoundAndAccountExists_ShouldRetrieveAccountFromPrimary()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var account = new Account
            {
                AccountNumber = "123",
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            _primary.Setup(accountDataStore => accountDataStore.GetAccount(account.AccountNumber)).Returns(account);

            _rules.Add(new BacsPaymentRule());

            var sut = CreateSut();

            // Act
            var result = sut.MakePayment(request);

            // Assert
            _primary.Verify(accountDataStore => accountDataStore.GetAccount(account.AccountNumber), Times.Once);
            _backup.Verify(accountDataStore => accountDataStore.GetAccount(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void MakePayment_WhenPrimaryNotFoundAndAccountExists_ShouldRetrieveAccountFromBackup()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var account = new Account
            {
                AccountNumber = "123",
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            _primary
                .Setup(accountDataStore => accountDataStore.GetAccount(account.AccountNumber))
                .Throws(new Exception("Unable to connect to the primary database"));

            _backup.Setup(accountDataStore => accountDataStore.GetAccount(account.AccountNumber)).Returns(account);

            _rules.Add(new BacsPaymentRule());

            var sut = CreateSut();

            // Act
            sut.MakePayment(request);

            // Assert
            _primary.Verify(accountDataStore => accountDataStore.GetAccount(account.AccountNumber), Times.Once);
            _backup.Verify(accountDataStore => accountDataStore.GetAccount(account.AccountNumber), Times.Once);
        }

        [Fact]
        public void MakePayment_WhenAccountNotValid_ShouldReturnUnsuccessfulResponse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            var account = new Account
            {
                AccountNumber = "123",
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            // set up neither mock account data store - these should return Null by default

            _rules.Add(new BacsPaymentRule());

            // Note for future improvement - exception strings can be extracted to their own class, so if the wording in the
            // service changes we don't also need to update the wording in the test.
            var expectedResult = new MakePaymentResult
            {
                Success = false,
                FailureMessage = "No matching account was found for the provided Debtor Account Number"
            };

            var sut = CreateSut();

            // Act
            var result = sut.MakePayment(request);

            // Assert
            _primary.Verify(x => x.GetAccount("123"), Times.Once);

            // backup not called because the connection to primary was successful
            _backup.Verify(x => x.GetAccount(It.IsAny<string>()), Times.Never);

            result.Success.Should().Be(expectedResult.Success);
            result.FailureMessage.Should().Be(expectedResult.FailureMessage);
        }

        [Fact]
        public void MakePayment_WhenInvalidPaymentSchemeProvided_ShouldReturnUnsuccessfulResponse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = 100,
                PaymentScheme = PaymentScheme.Chaps
            };

            var debtorAccount = new Account
            {
                AccountNumber = "123",
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            var creditorAccount = new Account
            {
                AccountNumber = "321",
                Balance = 250,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            _primary.Setup(accountDataStore => accountDataStore.GetAccount(debtorAccount.AccountNumber)).Returns(debtorAccount);
            _primary.Setup(accountDataStore => accountDataStore.GetAccount(creditorAccount.AccountNumber)).Returns(creditorAccount);

            // Only add rule for Bacs, not Chaps
            _rules.Add(new BacsPaymentRule());

            var expectedResult = new MakePaymentResult
            {
                Success = false,
                FailureMessage = $"Invalid Payment Scheme provided: {request.PaymentScheme}"
            };

            var sut = CreateSut();

            // Act
            var result = sut.MakePayment(request);

            // Assert
            result.Success.Should().Be(expectedResult.Success);
            result.FailureMessage.Should().Be(expectedResult.FailureMessage);
        }

        [Fact]
        public void MakePayment_WhenPaymentValidationFails_ShouldReturnUnsuccessfulResponse()
        {
            // Arrange
            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = 100,
                PaymentScheme = PaymentScheme.Bacs
            };

            // Do not allow Bacs payments
            var debtorAccount = new Account
            {
                AccountNumber = "123",
                Balance = 500,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps
            };

            var creditorAccount = new Account
            {
                AccountNumber = "321",
                Balance = 250,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Chaps
            };

            _primary.Setup(accountDataStore => accountDataStore.GetAccount(debtorAccount.AccountNumber)).Returns(debtorAccount);
            _primary.Setup(accountDataStore => accountDataStore.GetAccount(creditorAccount.AccountNumber)).Returns(creditorAccount);

            _rules.Add(new BacsPaymentRule());

            var expectedResult = new MakePaymentResult
            {
                Success = false,
                FailureMessage = "Payment failed validation"
            };

            var sut = CreateSut();

            // Act
            var result = sut.MakePayment(request);

            // Assert
            result.Success.Should().Be(expectedResult.Success);
            result.FailureMessage.Should().Be(expectedResult.FailureMessage);
        }

        [Fact]
        public void MakePayment_WhenPaymentValidationPasses_ShouldUpdateAccountAndReturnSuccess()
        {
            // Arrange
            var debtorStartingBalance = 500;
            var creditorStartingBalance = 250;
            var amountToTransfer = 100;

            var request = new MakePaymentRequest
            {
                DebtorAccountNumber = "123",
                CreditorAccountNumber = "321",
                Amount = amountToTransfer,
                PaymentScheme = PaymentScheme.Bacs
            };

            var debtorAccount = new Account
            {
                AccountNumber = "123",
                Balance = debtorStartingBalance,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            var creditorAccount = new Account
            {
                AccountNumber = "321",
                Balance = creditorStartingBalance,
                AllowedPaymentSchemes = AllowedPaymentSchemes.Bacs
            };

            _primary.Setup(accountDataStore => accountDataStore.GetAccount(debtorAccount.AccountNumber)).Returns(debtorAccount);
            _primary.Setup(accountDataStore => accountDataStore.GetAccount(creditorAccount.AccountNumber)).Returns(creditorAccount);

            _rules.Add(new BacsPaymentRule());

            var expectedResult = new MakePaymentResult
            {
                Success = true
            };

            var sut = CreateSut();

            // Act
            var result = sut.MakePayment(request);

            // Assert
            result.Success.Should().Be(expectedResult.Success);
            debtorAccount.Balance.Should().Be(debtorStartingBalance - amountToTransfer);
            creditorAccount.Balance.Should().Be(creditorStartingBalance + amountToTransfer);

            _primary.Verify(x => x.UpdateAccount(debtorAccount), Times.Once);
            _primary.Verify(x => x.UpdateAccount(creditorAccount), Times.Once);
            _backup.Verify(x => x.UpdateAccount(It.IsAny<Account>()), Times.Never);
        }

        private PaymentService CreateSut()
        {
            var accountStoreFactory = new Mock<IDataStoreFactory>();
            accountStoreFactory.Setup(p => p.Primary).Returns(_primary.Object);
            accountStoreFactory.Setup(p => p.Backup).Returns(_backup.Object);

            return new PaymentService(accountStoreFactory.Object, _rules);
        }
    }
}
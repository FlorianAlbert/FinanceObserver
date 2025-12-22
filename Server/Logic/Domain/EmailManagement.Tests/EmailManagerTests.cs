using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using Email = FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models.Email;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.EmailManagement.Tests;

public class EmailManagerTests
{
    private readonly Fixture _fixture;
    private readonly Email _email;

    private readonly IRepository<Guid, Email> _emailRepositoryMock;
    private readonly IFluentEmail _fluentEmailMock;

    private readonly EmailManager _sut;

    public EmailManagerTests()
    {
        _fixture = new Fixture();
        _fixture.Register(() => DateOnly.FromDateTime(_fixture.Create<DateTimeOffset>().Date));
        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => _fixture.Behaviors.Remove(b));
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _emailRepositoryMock = Substitute.For<IRepository<Guid, Email>>();

        IRepositoryFactory repositoryFactoryMock = Substitute.For<IRepositoryFactory>();
        repositoryFactoryMock.CreateRepository<Guid, Email>().Returns(_emailRepositoryMock);

        _email = _fixture.Create<Email>();

        _fluentEmailMock = Substitute.For<IFluentEmail>();
        // ReSharper disable PossibleMultipleEnumeration
        _fluentEmailMock.To(Arg.Is((IEnumerable<Address> addresses) => addresses.Select(a => a.EmailAddress)
                                                                           .SequenceEqual(_email.Receivers.Select(r =>
                                                                               r.Email)) &&
                                                                       addresses.Select(a => a.Name)
                                                                           .SequenceEqual(
                                                                               _email.Receivers.Select(r =>
                                                                                   r.FullName))))
            
            .Returns(_fluentEmailMock);
        // ReSharper restore PossibleMultipleEnumeration
        _fluentEmailMock.Subject(_email.Subject).Returns(_fluentEmailMock);
        _fluentEmailMock.Body(_email.Message).Returns(_fluentEmailMock);

        IFluentEmailFactory fluentEmailFactoryMock = Substitute.For<IFluentEmailFactory>();
        fluentEmailFactoryMock.Create().Returns(_fluentEmailMock);

        _sut = new EmailManager(repositoryFactoryMock, fluentEmailFactoryMock);
    }

    [Fact]
    public async Task SendEmailAsync_Call_ResultSucceeded()
    {
        // Arrange
        _fixture.Customize<SendResponse>(customizationComposer =>
            customizationComposer.With(sendResponse => sendResponse.ErrorMessages, []));

        SendResponse sendResponse = _fixture.Create<SendResponse>();

        _emailRepositoryMock.InsertAsync(_email, Arg.Any<CancellationToken>()).Returns(_email);
        _fluentEmailMock.SendAsync(Arg.Any<CancellationToken>()).Returns(sendResponse);

        // Act
        Result result = await _sut.SendEmailAsync(_email);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_Call_RepositoryInsertCalled()
    {
        // Arrange
        _fixture.Customize<SendResponse>(customizationComposer =>
            customizationComposer.With(sendResponse => sendResponse.ErrorMessages, []));

        SendResponse sendResponse = _fixture.Create<SendResponse>();

        _emailRepositoryMock.InsertAsync(_email, Arg.Any<CancellationToken>()).Returns(_email);
        _fluentEmailMock.SendAsync(Arg.Any<CancellationToken>()).Returns(sendResponse);

        // Act
        _ = await _sut.SendEmailAsync(_email);

        // Assert
        await _emailRepositoryMock.Received(1).InsertAsync(_email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SendEmailAsync_CallWithFailingSend_ResultFailed()
    {
        // Arrange
        _fixture.Customize<SendResponse>(customizationComposer =>
            customizationComposer.With(sendResponse => sendResponse.ErrorMessages,
                [.. _fixture.CreateMany<string>()]));

        _emailRepositoryMock.InsertAsync(_email, Arg.Any<CancellationToken>()).Returns(_email);
        SendResponse sendResponse = _fixture.Create<SendResponse>();

        _fluentEmailMock.SendAsync(Arg.Any<CancellationToken>()).Returns(sendResponse);

        // Act
        Result result = await _sut.SendEmailAsync(_email);

        // Assert
        result.Failed.Should().BeTrue();
    }

    [Fact]
    public async Task SendEmailAsync_CallWithFailingSend_ResultContainsOneError()
    {
        // Arrange
        _fixture.Customize<SendResponse>(customizationComposer =>
            customizationComposer.With(sendResponse => sendResponse.ErrorMessages,
                [.. _fixture.CreateMany<string>()]));

        SendResponse sendResponse = _fixture.Create<SendResponse>();

        _emailRepositoryMock.InsertAsync(_email, Arg.Any<CancellationToken>()).Returns(_email);
        _fluentEmailMock.SendAsync(Arg.Any<CancellationToken>()).Returns(sendResponse);

        // Act
        Result result = await _sut.SendEmailAsync(_email);

        // Assert
        result.Errors.Should().ContainSingle();
    }

    [Fact]
    public async Task SendEmailAsync_CallWithFailingSend_ResultErrorContainsAllSendErrorMessages()
    {
        // Arrange
        List<string> sendErrorMessages = [.. _fixture.CreateMany<string>()];

        _fixture.Customize<SendResponse>(customizationComposer =>
            customizationComposer.With(sendResponse => sendResponse.ErrorMessages, sendErrorMessages));

        SendResponse sendResponse = _fixture.Create<SendResponse>();

        _emailRepositoryMock.InsertAsync(_email, Arg.Any<CancellationToken>()).Returns(_email);
        _fluentEmailMock.SendAsync(Arg.Any<CancellationToken>()).Returns(sendResponse);

        // Act
        Result result = await _sut.SendEmailAsync(_email);

        // Assert
        result.Errors.Single().Detail.Should().ContainAll(sendErrorMessages);
    }
}
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using NSubstitute;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.DataTransactionHandling.Tests;

public class DataTransactionHandlerTests
{
    private readonly IDbTransactionHandler _dbTransactionHandlerMock;
    
    private readonly DataTransactionHandler _sut;

    public DataTransactionHandlerTests()
    {
        _dbTransactionHandlerMock = Substitute.For<IDbTransactionHandler>();
        _sut = new DataTransactionHandler(_dbTransactionHandlerMock);
    }

    [Fact]
    public async Task StartDbTransactionAsync_CalledWithoutCancellationToken_CallsDbTransactionHandlerStartTransactionAsyncOnce()
    {
        // Act
        await _sut.StartDbTransactionAsync();
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).StartTransactionAsync();
    }

    [Fact]
    public async Task StartDbTransactionAsync_CalledWithCancellationToken_CallsDbTransactionHandlerStartTransactionAsyncOnce()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        // Act
        await _sut.StartDbTransactionAsync(cancellationToken);
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).StartTransactionAsync(cancellationToken);
    }

    [Fact]
    public async Task CommitDbTransactionAsync_CalledWithoutCancellationToken_CallsDbTransactionHandlerCommitTransactionAsyncOnce()
    {
        // Act
        await _sut.CommitDbTransactionAsync();
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).CommitTransactionAsync();
    }

    [Fact]
    public async Task CommitDbTransactionAsync_CalledWithCancellationToken_CallsDbTransactionHandlerCommitTransactionAsyncOnce()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        // Act
        await _sut.CommitDbTransactionAsync(cancellationToken);
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).CommitTransactionAsync(cancellationToken);
    }

    [Fact]
    public async Task RollbackDbTransactionAsync_CalledWithoutCancellationToken_CallsDbTransactionHandlerRollbackTransactionAsyncOnce()
    {
        // Act
        await _sut.RollbackDbTransactionAsync();
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).RollbackTransactionAsync();
    }

    [Fact]
    public async Task RollbackDbTransactionAsync_CalledWithCancellationToken_CallsDbTransactionHandlerRollbackTransactionAsyncOnce()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        
        // Act
        await _sut.RollbackDbTransactionAsync(cancellationToken);
        
        // Assert
        await _dbTransactionHandlerMock.Received(1).RollbackTransactionAsync(cancellationToken);
    }
}
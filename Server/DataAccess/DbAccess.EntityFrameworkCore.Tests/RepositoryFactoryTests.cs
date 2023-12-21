using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests.TestModel;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.Tests;

public class RepositoryFactoryTests
{
    private readonly RepositoryFactory _sut;

    public RepositoryFactoryTests()
    {
        var contextMock = MockedDbContextBuilder<FinanceObserverContext>.Create().Build();
        var inclusionEvaluatorMock = Substitute.For<IInclusionEvaluator>();
        
        _sut = new RepositoryFactory(contextMock, inclusionEvaluatorMock);
    }
    
    [Fact]
    public void CreateRepository_Call_ReturnedRepositoryIsNotNull()
    {
        // Arrange

        // Act
        var repository = _sut.CreateRepository<Guid, TestEntity>();

        // Assert
        repository.Should().NotBeNull();
    }
}
using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Data.Inclusion;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;
using FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement;

public class UserManager : IUserManager
{
    private readonly IRepository<Guid, User> _repository;

    public UserManager(IRepositoryFactory repositoryFactory)
    {
        _repository = repositoryFactory.CreateRepository<Guid, User>();
    }

    public async Task<Result<User>> AddNewUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var userAlreadyExists = await _repository.ExistsAsync(
            existingUser => existingUser.UserName == user.UserName || existingUser.EmailAddress == user.EmailAddress,
            cancellationToken: cancellationToken);

        if (userAlreadyExists)
        {
            return Result<User>.Failure(Errors.UserAlreadyExistsError);
        }

        var insertedUser = await _repository.InsertAsync(user, cancellationToken);

        return Result<User>.Success(insertedUser);
    }

    public async Task<Result> RemoveUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(users as IQueryable<User> ?? users.AsQueryable(), cancellationToken);

        return Result.Success();
    }

    public Task<Result<User>> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken);
    }

    public async Task<Result<IQueryable<User>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _repository.QueryAsync(cancellationToken: cancellationToken);

        return Result<IQueryable<User>>.Success(users);
    }
}
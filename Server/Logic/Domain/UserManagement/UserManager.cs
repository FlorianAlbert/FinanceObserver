using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using FlorianAlbert.FinanceObserver.Server.CrossCutting.Infrastructure;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract;
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
        bool userAlreadyExists = await _repository.ExistsAsync(
            existingUser => existingUser.UserName == user.UserName || existingUser.EmailAddress == user.EmailAddress,
            cancellationToken: cancellationToken);

        if (userAlreadyExists)
        {
            return Result.Failure<User>(Errors.UserAlreadyExistsError);
        }

        User insertedUser = await _repository.InsertAsync(user, cancellationToken);

        return Result.Success(insertedUser);
    }

    public async Task<Result> RemoveUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(id, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RemoveUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default)
    {
        await _repository.DeleteAsync(users, cancellationToken);

        return Result.Success();
    }

    public Task<Result<User>> GetUserAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.FindAsync(id, cancellationToken: cancellationToken);
    }

    public async Task<Result<IQueryable<User>>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<User> users = await _repository.QueryAsync(cancellationToken: cancellationToken);

        return Result.Success(users);
    }

    public async Task<Result<User>> GetUserByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        IQueryable<User> allUsersQuery = await _repository.QueryAsync(cancellationToken: cancellationToken);
        User? matchingUser = allUsersQuery.SingleOrDefault(user => user.EmailAddress == emailAddress);

        if (matchingUser is null)
        {
            return Result.Failure<User>(Errors.UserNotFoundError);
        }

        return Result.Success(matchingUser);
    }
}
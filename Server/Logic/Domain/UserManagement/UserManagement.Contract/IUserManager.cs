using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;
using FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.Contract.Models;

namespace FlorianAlbert.FinanceObserver.Server.Logic.Domain.UserManagement.Contract;

public interface IUserManager
{
    Task<Result<User>> AddNewUserAsync(User user, CancellationToken cancellationToken = default);

    Task<Result> RemoveUserAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Result> RemoveUsersAsync(IEnumerable<User> users, CancellationToken cancellationToken = default);

    Task<Result<User>> GetUserAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<IQueryable<User>>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}
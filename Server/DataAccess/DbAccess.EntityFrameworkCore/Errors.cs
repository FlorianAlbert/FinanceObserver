using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.InfrastructureTypes;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore;

internal static class Errors
{
    private static Error? _entityNotFoundError;

    public static Error EntityNotFoundError =>
        _entityNotFoundError ??= new Error(nameof(EntityNotFoundError),
            "Entity not found",
            "The requested entity could not be found!");
}
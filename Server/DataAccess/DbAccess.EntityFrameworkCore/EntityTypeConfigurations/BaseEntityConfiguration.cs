using FlorianAlbert.FinanceObserver.Server.CrossCutting.DataClasses.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlorianAlbert.FinanceObserver.Server.DataAccess.DbAccess.EntityFrameworkCore.EntityTypeConfigurations;

internal abstract class BaseEntityConfiguration<TKey, TEntity> : IEntityTypeConfiguration<TEntity>
    where TKey : IParsable<TKey>,
                 IEquatable<TKey>
    where TEntity : BaseEntity<TKey>
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);
    }
}
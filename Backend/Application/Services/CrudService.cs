using BarberSync.Domain.Interfaces;

namespace BarberSync.Application.Services;

public class CrudService<TEntity, TDto, TCreateDto, TUpdateDto>
    where TEntity : class, new()
{
    protected readonly IRepository<TEntity> Repository;
    public CrudService(IRepository<TEntity> repository) => Repository = repository;
}

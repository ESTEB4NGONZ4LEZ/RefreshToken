
using System.Linq.Expressions;
using Dominio.Entities;
using Dominio.Interface;
using Microsoft.EntityFrameworkCore;
using Persistencia;

namespace Aplicacion.Repository;

public class GenericRepository<T> : IGeneric<T> where T : BaseEntity
{
    protected readonly MainContext _context;
    public GenericRepository(MainContext context)
    {
        _context = context;
    }
    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _context.Set<T>().ToListAsync();
    }
    public virtual async Task<T> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    public virtual void Add(T entity)
    {
        _context.Set<T>().Add(entity);
    }
    public virtual void AddRange(T entities)
    {
        _context.Set<T>().AddRange(entities);
    }
    public virtual void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }
    public virtual void Remove(T entity)
    {
        _context.Set<T>().Remove(entity);
    }

    public void RemoveRange(T entities)
    {
        _context.Set<T>().RemoveRange(entities);
    }
    public virtual IEnumerable<T> Find(Expression<Func<T, bool>> expression)
    {
        return _context.Set<T>().Where(expression);
    }
}


using Aplicacion.Repository;
using Dominio.Interface;
using Persistencia;

namespace Aplicacion.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly MainContext _context;
    public UnitOfWork(MainContext context)
    {
        _context = context;
    }
    private UserRepository _users;
    private RefreshTokenRepository _refreshToken;
    public IUser User
    {
        get
        {
            _users ??= new UserRepository(_context);
            return _users;
        }
    }

    public IRefreshToken RefreshToken 
    {
        get
        {
            _refreshToken ??= new RefreshTokenRepository(_context);
            return _refreshToken;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<int> SaveAsync()
    {
        return await _context.SaveChangesAsync();
    }
}

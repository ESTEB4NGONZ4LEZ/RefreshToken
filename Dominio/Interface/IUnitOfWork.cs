
namespace Dominio.Interface;

public interface IUnitOfWork
{
    IUser User { get; }
    IRefreshToken RefreshToken { get; }
    Task<int> SaveAsync();
}

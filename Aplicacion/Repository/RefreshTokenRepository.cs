
using Dominio.Entities;
using Dominio.Interface;
using Persistencia;

namespace Aplicacion.Repository;

public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshToken
{
    public RefreshTokenRepository(MainContext context) : base(context)
    {
    }
}


using Dominio.Entities;
using Dominio.Interface;
using Persistencia;

namespace Aplicacion.Repository;

public class UserRepository : GenericRepository<User>, IUser
{
    public UserRepository(MainContext context) : base(context)
    {
    }
    public async Task<User> GetUserByUsername(string username)
    {
        return _context.Users.FirstOrDefault(x => x.Username.ToLower() == username.ToLower());
    }
}

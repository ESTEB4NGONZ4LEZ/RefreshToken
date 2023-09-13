
using Dominio.Entities;

namespace Dominio.Interface;

public interface IUser : IGeneric<User>
{
    Task<User> GetUserByUsername(string username); 
}

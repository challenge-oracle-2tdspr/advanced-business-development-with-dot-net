using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class UserRepository : InMemoryRepository<User>, IRepository<User>
    {
    }
}
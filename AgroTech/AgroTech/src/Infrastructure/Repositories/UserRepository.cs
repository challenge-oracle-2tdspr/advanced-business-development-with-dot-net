using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;

namespace AgroTech.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IRepository<User>
    {
        public UserRepository(AgroTechDbContext context) : base(context)
        {
        }
    }
}
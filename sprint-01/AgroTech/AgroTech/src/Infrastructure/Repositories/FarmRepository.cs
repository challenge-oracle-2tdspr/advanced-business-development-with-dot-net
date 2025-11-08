using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;

namespace AgroTech.Infrastructure.Repositories
{
    public class FarmRepository : Repository<Farm>, IRepository<Farm>
    {
        public FarmRepository(AgroTechDbContext context) : base(context)
        {
        }
    }
}
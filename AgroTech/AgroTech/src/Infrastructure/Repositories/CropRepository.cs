using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;

namespace AgroTech.Infrastructure.Repositories
{
    public class CropRepository : Repository<Crop>, IRepository<Crop>
    {
        public CropRepository(AgroTechDbContext context) : base(context)
        {
        }
    }
}
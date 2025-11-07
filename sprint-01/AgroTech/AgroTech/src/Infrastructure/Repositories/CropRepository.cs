using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class CropRepository : InMemoryRepository<Crop>, IRepository<Crop>
    {
    }
}
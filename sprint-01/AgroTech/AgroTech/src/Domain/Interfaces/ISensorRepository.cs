using AgroTech.Domain.Entities;

namespace AgroTech.Domain.Interfaces
{
    public interface ISensorRepository : IRepository<Sensor>
    {
        Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId);
    }
}
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AgroTech.Infrastructure.Data;

namespace AgroTech.Infrastructure.Repositories
{
    public class SensorRepository : Repository<Sensor>, ISensorRepository
    {
        public SensorRepository(AgroTechDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Sensor>> GetByFarmIdAsync(Guid farmId)
        {
            return await _dbSet.Where(s => s.FarmId == farmId).ToListAsync();
        }
    }
}
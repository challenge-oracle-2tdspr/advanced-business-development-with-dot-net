using AgroTech.Application.Interfaces;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Mongo.Documents;

namespace AgroTech.IntegrationTests
{
    public class FakeSensorReadingMongoRepository : ISensorReadingMongoRepository
    {
        public Task AddAsync(
            SensorReadingDocument document,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
using AgroTech.Application.Interfaces;
using AgroTech.Configuration;
using AgroTech.Infrastructure.Mongo.Documents;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace AgroTech.Infrastructure.Mongo.Repositories;

    public class SensorReadingMongoRepository : ISensorReadingMongoRepository
    {
        private readonly IMongoCollection<SensorReadingDocument> _collection;

        public SensorReadingMongoRepository(
            IMongoDatabase database,
            IOptions<MongoDbOptions> options)
        {
            var collectionName = options.Value.SensorReadingsCollectionName;

            if (string.IsNullOrWhiteSpace(collectionName))
                throw new InvalidOperationException("MongoDb:SensorReadingsCollectionName não configurado.");

            _collection = database.GetCollection<SensorReadingDocument>(collectionName);
        }

        public async Task AddAsync(SensorReadingDocument document, CancellationToken cancellationToken = default)
        {
            await _collection.InsertOneAsync(document, cancellationToken: cancellationToken);
        }
    }
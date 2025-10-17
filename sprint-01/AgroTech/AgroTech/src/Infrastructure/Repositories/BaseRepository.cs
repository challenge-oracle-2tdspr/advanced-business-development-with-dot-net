using AgroTech.Domain.Common;
using AgroTech.Domain.Interfaces;

namespace AgroTech.Infrastructure.Repositories
{
    public class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly List<T> _items = new();

        public Task AddAsync(T entity)
        {
            _items.Add(entity);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            var entity = _items.FirstOrDefault(e => e.Id == id);
            if (entity != null)
                _items.Remove(entity);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<T>> GetAllAsync()
        {
            return Task.FromResult(_items.AsEnumerable());
        }

        public Task<T?> GetByIdAsync(Guid id)
        {
            var entity = _items.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(T entity)
        {
            var existing = _items.FirstOrDefault(e => e.Id == entity.Id);
            if (existing != null)
            {
                _items.Remove(existing);
                _items.Add(entity);
            }
            return Task.CompletedTask;
        }
    }
}
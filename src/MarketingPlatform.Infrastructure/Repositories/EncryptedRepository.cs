using MarketingPlatform.Core.Interfaces;
using MarketingPlatform.Core.Interfaces.Repositories;
using MarketingPlatform.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace MarketingPlatform.Infrastructure.Repositories
{
    /// <summary>
    /// Repository decorator that provides transparent encryption/decryption for sensitive entity fields.
    /// Automatically encrypts data before saving and decrypts when retrieving.
    /// </summary>
    public class EncryptedRepository<T> : IRepository<T> where T : class
    {
        private readonly IRepository<T> _innerRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IServiceProvider _serviceProvider;

        public EncryptedRepository(
            IRepository<T> innerRepository, 
            IEncryptionService encryptionService,
            IServiceProvider serviceProvider)
        {
            _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            var entity = await _innerRepository.GetByIdAsync(id);
            if (entity != null)
            {
                DecryptEntity(entity);
            }
            return entity;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var entities = await _innerRepository.GetAllAsync();
            foreach (var entity in entities)
            {
                DecryptEntity(entity);
            }
            return entities;
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            var entities = await _innerRepository.FindAsync(predicate);
            foreach (var entity in entities)
            {
                DecryptEntity(entity);
            }
            return entities;
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            var entity = await _innerRepository.FirstOrDefaultAsync(predicate);
            if (entity != null)
            {
                DecryptEntity(entity);
            }
            return entity;
        }

        public Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return _innerRepository.AnyAsync(predicate);
        }

        public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return _innerRepository.CountAsync(predicate);
        }

        public async Task AddAsync(T entity)
        {
            EncryptEntity(entity);
            await _innerRepository.AddAsync(entity);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                EncryptEntity(entity);
            }
            await _innerRepository.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            EncryptEntity(entity);
            _innerRepository.Update(entity);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                EncryptEntity(entity);
            }
            _innerRepository.UpdateRange(entities);
        }

        public void Remove(T entity)
        {
            _innerRepository.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            _innerRepository.RemoveRange(entities);
        }

        public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize)
        {
            var entities = await _innerRepository.GetPagedAsync(pageNumber, pageSize);
            foreach (var entity in entities)
            {
                DecryptEntity(entity);
            }
            return entities;
        }

        public async Task<IEnumerable<T>> GetPagedAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
        {
            var entities = await _innerRepository.GetPagedAsync(predicate, pageNumber, pageSize);
            foreach (var entity in entities)
            {
                DecryptEntity(entity);
            }
            return entities;
        }

        public IQueryable<T> GetQueryable()
        {
            // Note: Queryable operations won't have decryption applied automatically
            // Use other methods for encrypted data
            return _innerRepository.GetQueryable();
        }

        /// <summary>
        /// Encrypts sensitive fields on an entity before saving to database.
        /// Override this method in specific encrypted repository implementations.
        /// </summary>
        protected virtual void EncryptEntity(T entity)
        {
            // Base implementation - override in specific repositories
        }

        /// <summary>
        /// Decrypts sensitive fields on an entity after retrieving from database.
        /// Override this method in specific encrypted repository implementations.
        /// </summary>
        protected virtual void DecryptEntity(T entity)
        {
            // Base implementation - override in specific repositories
        }
    }
}

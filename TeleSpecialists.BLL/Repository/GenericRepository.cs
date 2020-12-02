using System.Linq;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Collections.Generic;
using TeleSpecialists.BLL.Model;
using System;
using System.Data.Entity.Migrations;

namespace TeleSpecialists.BLL.Repository
{
    public interface IGenericRepository<T>
    {
        Task<T> FindAsync(int id);
        T Find(int id);
        IQueryable<T> Query();
        void Insert(T entity);
        void InsertRange(IEnumerable<T> entity);
        void Update(T entity);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(int id);
        void Delete(Guid id);
        void DeleteRange(IEnumerable<T> entity);
        void DeleteRate(T entity);
        void AddUpdate(T entity);
    }

    public abstract class GenericRepository<T> : IGenericRepository<T> where T : class, new()
    {
        protected readonly TeleSpecialistsContext _dbContext;
        public GenericRepository(TeleSpecialistsContext dbContext)
        {
            _dbContext = dbContext;
            _dbContext.Database.Log = (msg => System.Diagnostics.Debug.Write(msg, "SQL"));
        }
        public async Task<T> FindAsync(int id)
        {
            return await _dbContext.Set<T>().FindAsync(id);
        }
        public T Find(int id)
        {
            return _dbContext.Set<T>().Find(id);
        }
        public T Find(Guid id)
        {
            return _dbContext.Set<T>().Find(id);
        }
        public IQueryable<T> Query()
        {
            return _dbContext.Set<T>().AsQueryable();
        }
        public void Insert(T entity)
        {
            _dbContext.Set<T>().Add(entity);
        }
        public void InsertRange(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().AddRange(entities);
        }
        public void Update(T entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }
        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }
        public void Delete(int id)
        {
            var entity = Find(id);
            // if entity exists
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
            }
        }
        public void Delete(Guid id)
        {
            var entity = Find(id);
            // if entity exists
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
            }
        }
        public void DeleteRange(IEnumerable<T> entities)
        {
            _dbContext.Set<T>().RemoveRange(entities);
        }
        public void DeleteRate(T entity)
        {
            // if entity exists
            if (entity != null)
            {
                _dbContext.Set<T>().Remove(entity);
            }
        }
        public void AddUpdate(T entity)
        {
            _dbContext.Set<T>().AddOrUpdate(entity);
        }
    }
}

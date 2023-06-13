using DataLayer;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace RepositoryLayer
{
    public class Repository<T> where T : class
    {
        private SchoolContext _context;
        private DbSet<T> _dbSet;
        public Repository(SchoolContext schoolContext)
        {
            _context = schoolContext;
            _dbSet = _context.Set<T>();
        }

        public Task<int> SaveChangeAsync()
        {
            return _context.SaveChangesAsync();
        }

        public IQueryable<T> Queryable()
        {
            return _dbSet;
        }

        public void Update(object key,T entity)
        {
            var originObj = _dbSet.Find(key);

            var updatedObj = CheckUpdateObject(originObj, entity);

            _context.Entry(originObj).CurrentValues.SetValues(updatedObj);
        }

        private object CheckUpdateObject(object originalObj, object updateObj)
        {
            foreach (var property in updateObj.GetType().GetProperties())
            {
                if (property.GetValue(updateObj, null) == null)
                {
                    property.SetValue(updateObj, originalObj.GetType().GetProperty(property.Name).GetValue(originalObj, null));
                }
            }

            return updateObj;
        }
    }
}

using DataLayer;
using System;
using System.Data.Entity;
using System.Linq;

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

        public IQueryable<T> Queryable()
        {
            return _dbSet;
        }

        public void Update(T entity)
        {
            var oldobj = _dbSet.Find(entity);

            var UpdatedObj = CheckUpdateObject(oldobj, entity);

            _context.Entry(oldobj).CurrentValues.SetValues(UpdatedObj);
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

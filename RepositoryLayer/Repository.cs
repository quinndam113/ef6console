using DataLayer;
using System.Linq;

namespace RepositoryLayer
{
    public class Repository<T> where T : class
    {
        private SchoolContext _context;
        public Repository(SchoolContext schoolContext)
        {
            _context = schoolContext;
        }

        public IQueryable<T> Queryable()
        {
            return _context.Set<T>();
        }
    }
}

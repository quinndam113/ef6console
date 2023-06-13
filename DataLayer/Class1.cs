using EntitiesLayer;
using System.Data.Entity;
using System.Runtime.Remoting.Contexts;

namespace DataLayer
{
    public class SchoolContext : DbContext
    {
        public SchoolContext() : base("name=SchoolDBConnectionString")
        {
            Database.Log = s => System.Diagnostics.Debug.WriteLine(s);
        }

        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }
    }
}

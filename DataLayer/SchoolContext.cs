using EntitiesLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace DataLayer
{
    public class SchoolContext : DbContext
    {
        private int _userId = 1;

        public SchoolContext() : base("name=SchoolDBConnectionString")
        {
            Database.Log = s => Console.WriteLine(s);
        }

        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Grade> Grades { get; set; }

        public virtual DbSet<HistoryEntry> HistoryEntrys { get; set; }
        public virtual DbSet<HistoryEntryChange> HistoryEntryChanges { get; set; }

        public override int SaveChanges()
        {
            var entityAudits = OnBeforeSaveChanges();

            int result = base.SaveChanges();

            SaveHistoryEntry(entityAudits);

            return result;
        }

        private IEnumerable<HistoryEntryChange> OnBeforeSaveChanges()
        {
            ChangeTracker.DetectChanges();
            var auditEntries = new List<HistoryEntryChange>();

            foreach (DbEntityEntry entry in ChangeTracker.Entries())
            {
                if (!entry.ShouldBeAudited())
                {
                    continue;
                }

                var entityType = entry.Entity.GetType();
                var entityName = entityType.Name;
                var primaryKeyNames = GetPrimaryKeyNames(entityName);
                var props = entityType.GetProperties().Where(x => primaryKeyNames.Contains(x.Name));
                var p = string.Join(",", (props.ToDictionary(x => x.Name, x => x.GetValue(entry.Entity))).Select(x => x.Key + "=" + x.Value));

                auditEntries.Add(new HistoryEntryChange(entry, entityName, p, _userId));
            }
            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries;
        }

        private void SaveHistoryEntry(IEnumerable<HistoryEntryChange> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count() == 0)
                return;

            base.SaveChanges();
        }

        private List<string> GetPrimaryKeyNames(string entityName)
        {
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;
            var entitySet = objectContext.MetadataWorkspace
                                         .GetItemCollection(DataSpace.SSpace)
                                         .GetItems<EntityContainer>()
                                         .Single()
                                         .BaseEntitySets
                                         .FirstOrDefault(es => es.ElementType.Name == entityName);

            if (entitySet == null)
            {
                throw new InvalidOperationException($"No entity set found for type {entityName}");
            }

            var keyMembers = entitySet.ElementType.KeyMembers;
            return keyMembers.Select(km => km.Name).ToList();
        }
    }
}

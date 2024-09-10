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

                if(entry.State == EntityState.Modified)
                {
                    var entityName = entry.Entity.GetType().Name;
                    var entityId = GetEntityId(entry);

                    auditEntries.Add(new HistoryEntryChange(entry, entityName, entityId, _userId));
                }
            }
            // keep a list of entries where the value of some properties are unknown at this step
            return auditEntries;
        }

        public List<(string ForeignKeyPropertyName, string EntityName)> GetForeignKeys(string entityName)
        {
            // Get the ObjectContext from the DbContext
            var objectContext = ((IObjectContextAdapter)this).ObjectContext;

            // Get the metadata workspace
            var metadataWorkspace = objectContext.MetadataWorkspace;

            // Get the entity type in the conceptual model (C-Space)
            var entityType = metadataWorkspace
                .GetItems<EntityType>(DataSpace.CSpace)
                .FirstOrDefault(e => e.Name == entityName);

            if (entityType == null)
            {
                throw new InvalidOperationException($"Entity type '{entityName}' not found in the model.");
            }

            // List to hold foreign key property names
            var foreignKeysAndNavigationTypes = new List<(string ForeignKeyPropertyName, string EntityName)>();

            // Iterate through all navigation properties of the entity
            foreach (var navigationProperty in entityType.NavigationProperties)
            {
                // Get the association type (relationship) for the navigation property
                var associationType = metadataWorkspace
                    .GetItems<AssociationType>(DataSpace.CSpace)
                    .FirstOrDefault(a => a.Name == navigationProperty.RelationshipType.Name);

                if (associationType != null && associationType.IsForeignKey)
                {
                    // Find the referential constraint associated with the navigation property
                    var referentialConstraint = associationType.Constraint;
                    var fromProperty = referentialConstraint.FromProperties.FirstOrDefault();
                    var entityTypeee = navigationProperty.ToEndMember.GetEntityType();

                    foreignKeysAndNavigationTypes.Add((fromProperty.Name, entityTypeee.Name));
                }
            }

            return foreignKeysAndNavigationTypes;
        }

        private void SaveHistoryEntry(IEnumerable<HistoryEntryChange> auditEntries)
        {
            if (auditEntries == null || auditEntries.Count() == 0)
                return;

            Set<HistoryEntryChange>().AddRange(auditEntries);

            base.SaveChanges();
        }

        private string GetEntityId(DbEntityEntry entry)
        {
            var entityType = entry.Entity.GetType();
            var entityName = entityType.Name;
            var primaryKeyNames = GetPrimaryKeyNames(entityName);
            var primaryKeyProp = entityType.GetProperties().FirstOrDefault(x => primaryKeyNames.Contains(x.Name));

            return primaryKeyProp.GetValue(entry.Entity).ToString();
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

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace DataLayer
{
    public class HistoryEntryChange
    {
        [Key]
        public Guid Id { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime CreatedDate { get; set; }
        public EntityState EntityState { get; set; }
        public string ByUser { get; set; }

        public Guid HistoryEntryId { get; set; }
        [ForeignKey("HistoryEntryId")]
        public HistoryEntry HistoryEntry { get; set; }

        public HistoryEntryChange(DbEntityEntry entry,
            string entityName,
            string primaryKey,
            int userId)
        {
            var oldValuesDict = new Dictionary<string, object>();
            var newValuesDict = new Dictionary<string, object>();
            
            foreach (var propName in entry.CurrentValues.PropertyNames)
            {
                DbPropertyEntry property = entry.Property(propName);
                if (property.IsAuditable())
                {
                    string propertyName = property.Name;

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValuesDict[propertyName] = property.CurrentValue;
                            break;

                        case EntityState.Deleted:
                            oldValuesDict[propertyName] = property.OriginalValue;
                            break;

                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValuesDict[propertyName] = property.OriginalValue;
                                newValuesDict[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }
            }

            ByUser = userId.ToString();
            Id = Guid.NewGuid();
            OldValues = oldValuesDict.Any() ? JsonConvert.SerializeObject(oldValuesDict) : null;
            NewValues = newValuesDict.Any() ? JsonConvert.SerializeObject(newValuesDict) : null;
            CreatedDate = DateTime.Now;
        }
    }

    public class HistoryEntry
    {
        [Key]
        public Guid Id { get; set; }
        public string ReadablePrimaryKey { get; set; }
        public string EntityName { get; set; }
        public ICollection<HistoryEntryChange> Changes { get; set; }
    }
}

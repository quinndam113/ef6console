using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace DataLayer
{
    public class HistoryEntryChange
    {
        [Key]
        public Guid Id { get; set; }
        public string EntityId { get; set; }
        public string EntityName { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime CreatedDate { get; set; }
        public EntityState EntityState { get; set; }
        public string ByUser { get; set; }

        public HistoryEntryChange() { }

        public HistoryEntryChange(DbEntityEntry entry,
            string entityName,
            string primaryKey,
            int userId)
        {
            var oldValuesDict = new Dictionary<string, object>();
            var newValuesDict = new Dictionary<string, object>();

            foreach (var propName in entry.CurrentValues.PropertyNames)
            {
                var property = entry.Property(propName);
                if (property.IsAuditable() && property.IsModified)
                {
                    oldValuesDict[propName] = property.OriginalValue;
                    newValuesDict[propName] = property.CurrentValue;
                }
            }

            EntityState = entry.State;
            ByUser = userId.ToString();
            Id = Guid.NewGuid();
            EntityId = primaryKey;
            EntityName = entityName;
            OldValues = oldValuesDict.Any() ? JsonConvert.SerializeObject(oldValuesDict) : null;
            NewValues = newValuesDict.Any() ? JsonConvert.SerializeObject(newValuesDict) : null;
            CreatedDate = DateTime.Now;
        }
    }

    public class AuditValue
    {
        public string PropertyName { get; set; }
        public object Value { get; set; }
        public string ValueType { get; set; }
    }

    public class AuditForeigonValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public static class JsonToObjectValueListConverter
    {
        public static List<AuditValue> ToAuditValues(this string jsonString)
        {
            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return new List<AuditValue>();
            }

            // Deserialize the JSON string into a Dictionary
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            // Convert the Dictionary to a List<ObjectValue>
            List<AuditValue> objectValues = new List<AuditValue>();

            foreach (var kvp in dictionary)
            {
                objectValues.Add(new AuditValue
                {
                    PropertyName = kvp.Key,
                    Value = kvp.Value,
                    ValueType = kvp.Value?.GetType().Name ?? null
                }); ;
            }

            return objectValues;
        }
    }
}

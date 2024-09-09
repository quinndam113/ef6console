using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using EntitiesLayer;

namespace DataLayer
{
    public static class  InternalExtensions
    {
        internal static bool ShouldBeAudited(this DbEntityEntry entry)
        {
            return entry.State != EntityState.Detached && entry.State != EntityState.Unchanged &&
                   !(entry.Entity is HistoryEntry) && !(entry.Entity is HistoryEntryChange) &&
            entry.IsAuditable();
        }
        internal static bool IsAuditable(this DbEntityEntry entityEntry)
        {
            HistoryableAttribute enableAuditAttribute = (HistoryableAttribute)Attribute.GetCustomAttribute(entityEntry.Entity.GetType(), typeof(HistoryableAttribute));
            return enableAuditAttribute != null;
        }

        internal static bool IsAuditable(this DbPropertyEntry propertyEntry)
        {
            Type entityType = propertyEntry.EntityEntry.Entity.GetType();
            PropertyInfo propertyInfo = entityType.GetProperty(propertyEntry.Name);
            bool disableAuditAttribute = Attribute.IsDefined(propertyInfo, typeof(UnHistoryableAttribute));

            return IsAuditable(propertyEntry.EntityEntry) && !disableAuditAttribute;
        }
    }
}

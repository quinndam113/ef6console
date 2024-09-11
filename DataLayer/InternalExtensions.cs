using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using EntitiesLayer;
using System.Linq;
using System.Collections.Generic;

namespace DataLayer
{
    public static class AuditableExtensions
    {
        internal static bool ShouldBeAudited(this DbEntityEntry entry)
        {
            return entry.State != EntityState.Detached &&
                entry.State != EntityState.Unchanged &&
                entry.IsAuditable();
        }

        internal static bool IsAuditable(this DbEntityEntry entityEntry)
        {
            var entityType = entityEntry.Entity.GetType();
            var historyableMap = AuditableExtensions.FindHistoryableMap(entityType.Name);

            return historyableMap != null;
        }

        internal static bool IsAuditable(this DbPropertyEntry propertyEntry)
        {
            var entityType = propertyEntry.EntityEntry.Entity.GetType();
            var skipAuditProperties = AuditableExtensions.GetSkipAuditProperties(entityType.Name);

            return (skipAuditProperties == null
                || skipAuditProperties.Count == 0 ||
                !skipAuditProperties.Contains(propertyEntry.Name));
        }

        public static Type FindHistoryableMap(string entityTypeName)
        {
            // Get all assemblies loaded in the current AppDomain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            // Look for a type that inherits from HistoryableMap<T> where T matches the provided entityType
            var mapType = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .FirstOrDefault(type => type.BaseType != null
                               && type.BaseType.IsGenericType
                               && type.BaseType.GetGenericTypeDefinition() == typeof(HistoryableMap<>)
                               && type.BaseType.GenericTypeArguments.Any(y => y.Name == entityTypeName));

            return mapType;
        }


        public static List<HistoryableMapConfig> GetMaps(string entityName)
        {
            var historyableMapType = FindHistoryableMap(entityName);
            var instance = Activator.CreateInstance(historyableMapType);
            return (List<HistoryableMapConfig>)historyableMapType.GetProperty("Maps").GetValue(instance);
        }

        public static List<string> GetSkipAuditProperties(string entityName)
        {
            var historyableMapType = FindHistoryableMap(entityName);
            var instance = Activator.CreateInstance(historyableMapType);
            return (List<string>)historyableMapType.GetProperty("SkipAuditProperties").GetValue(instance);
        }
    }
}

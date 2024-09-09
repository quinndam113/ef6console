using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace ServiceLayer
{
    public static class DbContextExtensions
    {
        #region Bulk Insert
        public static void BulkInsert<T>(this DbContext context, List<T> entities) where T : class
        {
            if (entities == null || !entities.Any())
            {
                return;
            }

            // Get the connection string
            var connectionString = context.Database.Connection.ConnectionString;

            // Get the table name
            string tableName = context.GetTableName<T>();

            // Get column mappings from EF configuration
            var columnMappings = context.GetColumnMappings<T>();

            // Create SqlBulkCopy instance
            using (var bulkCopy = new SqlBulkCopy(connectionString))
            {
                // Set the destination table name
                bulkCopy.DestinationTableName = tableName;

                // Set column mappings
                foreach (var mapping in columnMappings)
                {
                    bulkCopy.ColumnMappings.Add(mapping.Key, mapping.Value);
                }

                // Convert the list of entities to a DataTable
                var dataTable = entities.ToDataTable();

                // Write the DataTable to the server
                bulkCopy.WriteToServer(dataTable);
            }
        }

        private static DataTable ToDataTable<T>(this IEnumerable<T> data)
        {
            var properties = typeof(T).GetProperties();

            var dataTable = new DataTable();

            foreach (var prop in properties)
            {
                dataTable.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            }

            foreach (var item in data)
            {
                var values = properties.Select(prop => prop.GetValue(item) ?? DBNull.Value).ToArray();
                dataTable.Rows.Add(values);
            }

            return dataTable;
        }
        #endregion Bulk Insert

        public static void BulkUpdateEntities<T>(this DbContext context,
            List<T> entities,
            object propertyValue) where T : class
        {
            if (entities == null || !entities.Any())
            {
                throw new InvalidOperationException($"Entities list is empty or null for type {typeof(T).Name}");
            }

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    // Get the table name
                    string tableName = context.GetTableName<T>();

                    // Get the primary key names
                    var keyNames = context.GetPrimaryKeyNames<T>();

                    if (!keyNames.Any())
                    {
                        throw new InvalidOperationException($"No primary key defined for type {typeof(T).Name}");
                    }

                    if (keyNames.Count > 1)
                    {
                        throw new InvalidOperationException($"Does not support more than 1 key in table {typeof(T).Name} for now");
                    }

                    var primaryKeyName = keyNames.First();

                    // Get all properties of the entity except the primary keys
                    var vauleObjectType = propertyValue.GetType();
                    var properties = vauleObjectType.GetProperties().Where(p => !keyNames.Contains(p.Name)).ToList();

                    if (!properties.Any())
                    {
                        throw new InvalidOperationException($"No properties to update for type {typeof(T).Name}");
                    }
                    // Build the SQL UPDATE command
                    var setClause = string.Join(", ", properties.Select(p => $"[{p.Name}] = @{p.Name}"));
                    var (KeysParameter, Parameters) = GenerateKeysParameters<T>(primaryKeyName, entities);

                    var updateCommandText = $"UPDATE {tableName} SET {setClause} WHERE [{primaryKeyName}] in ({KeysParameter})";
                    context.Database.ExecuteSqlCommand(updateCommandText, Parameters);

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public static void BulkDeleteIds<T, TKey>(this DbContext context, List<TKey> ids) where T : class
        {
            if (ids.Count == 0) return;

            string tableName = context.GetTableName<T>();
            var keyNames = context.GetPrimaryKeyNames<T>();
            var primaryKeyName = keyNames.First();

            ids.ForEachPage(2000, items =>
            {
                var (KeysParameter, Parameters) = GenerateIdsParameters(primaryKeyName, items);
                var commandText = $"DELETE FROM {tableName} where [{primaryKeyName}] in ({KeysParameter})";

                context.Database.ExecuteSqlCommand(commandText, Parameters);
            });
        }

        public static void BulkDeleteEntities<T>(this DbContext context, List<T> entities) where T : class
        {
            if (entities == null || !entities.Any())
            {
                throw new InvalidOperationException($"Entities list is empty or null for type {typeof(T).Name}");
            }

            // Get the table name
            string tableName = context.GetTableName<T>();

            // Get the primary key names
            var keyNames = context.GetPrimaryKeyNames<T>();

            if (!keyNames.Any())
            {
                throw new InvalidOperationException($"No primary key defined for type {typeof(T).Name}");
            }

            if (keyNames.Count > 1)
            {
                throw new InvalidOperationException($"Does not support more than 1 key in table {typeof(T).Name} for now");
            }

            var primaryKeyName = keyNames.First();

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
                    var pageSize = 1000;
                    var totalPages = (int)Math.Ceiling((double)entities.Count / pageSize);

                    for (int page = 1; page <= totalPages; page++)
                    {
                        var pageEntities = entities.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                        var (KeysParameter, Parameters) = GenerateKeysParameters<T>(primaryKeyName, pageEntities);
                        var commandText = $"DELETE FROM {tableName} where [{primaryKeyName}] in ({KeysParameter})";

                        context.Database.ExecuteSqlCommand(commandText, Parameters);
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        #region Helper Methods
        private static (string KeysParameter, SqlParameter[] Parameters) GenerateIdsParameters<T>(string primaryKeyName,
            IEnumerable<T> ids)
        {
            if (ids.Count() > 2100)
                throw new InvalidOperationException("2100 is maximum parameters");

            var keysParameter = new List<string>();
            var parameters = new List<SqlParameter>();

            for (int i = 0; i < ids.Count(); i++)
            {
                var parameterName = $"@{primaryKeyName}{i}";
                keysParameter.Add(parameterName);

                object value = ids.ElementAt(i);

                var p = new SqlParameter(parameterName, value ?? DBNull.Value);
                parameters.Add(p);
            }


            var keyConditions = string.Join(",", keysParameter);

            return (keyConditions, parameters.ToArray());
        }

        private static (string KeysParameter, SqlParameter[] Parameters) GenerateKeysParameters<T>(string primaryKeyName,
            List<T> entities) where T : class
        {
            if (entities.Count > 2100)
                throw new InvalidOperationException("2100 is maximum parameters");

            var keysParameter = new List<string>();
            var parameters = new List<SqlParameter>();

            for (int i = 0; i < entities.Count; i++)
            {
                var parameterName = $"@{primaryKeyName}{i}";
                keysParameter.Add(parameterName);

                var entity = entities[i];
                var property = entity.GetType().GetProperty(primaryKeyName);
                var value = property.GetValue(entity);

                var p = new SqlParameter(parameterName, value ?? DBNull.Value);
                parameters.Add(p);
            }


            var keyConditions = string.Join(",", keysParameter);

            return (keyConditions, parameters.ToArray());
        }

        private static List<string> GetPrimaryKeyNames<T>(this DbContext context) where T : class
        {
            var entityType = typeof(T);
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var entitySet = objectContext.MetadataWorkspace
                                         .GetItemCollection(DataSpace.SSpace)
                                         .GetItems<EntityContainer>()
                                         .Single()
                                         .BaseEntitySets
                                         .FirstOrDefault(es => es.ElementType.Name == entityType.Name);

            if (entitySet == null)
            {
                throw new InvalidOperationException($"No entity set found for type {entityType.Name}");
            }

            var keyMembers = entitySet.ElementType.KeyMembers;
            return keyMembers.Select(km => km.Name).ToList();
        }

        private static string GetTableName<T>(this DbContext context) where T : class
        {
            var entityType = typeof(T);
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;

            var sql = objectContext.CreateObjectSet<T>().ToTraceString();
            var regex = new System.Text.RegularExpressions.Regex("FROM (?<table>.*) AS");
            var match = regex.Match(sql);

            if (!match.Success)
                throw new InvalidOperationException($"Cannot find table name for type {entityType.Name}");

            return match.Groups["table"].Value;
        }

        private static Dictionary<string, string> GetColumnMappings<T>(this DbContext context) where T : class
        {
            var metadata = ((IObjectContextAdapter)context).ObjectContext.MetadataWorkspace;
            var entityType = metadata.GetItems<EntityType>(DataSpace.OSpace)
                                     .Single(e => e.Name == typeof(T).Name);

            var entitySet = metadata.GetItems<EntityContainer>(DataSpace.CSpace)
                                    .Single()
                                    .EntitySets
                                    .Single(s => s.ElementType.Name == entityType.Name);

            var mappings = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace)
                                   .Single()
                                   .EntitySetMappings
                                   .Single(s => s.EntitySet == entitySet)
                                   .EntityTypeMappings
                                   .Single()
                                   .Fragments
                                   .Single()
                                   .PropertyMappings
                                   .OfType<ScalarPropertyMapping>();

            var columnMappings = new Dictionary<string, string>();

            foreach (var mapping in mappings)
            {
                var propertyName = mapping.Property.Name;
                var columnName = mapping.Column.Name;
                columnMappings[propertyName] = columnName;
            }

            return columnMappings;
        }

        #endregion Helper Methods
    }

    public static class ListExtension
    {
        public static void ForEachPage<T>(this IEnumerable<T> source, int pageSize, Action<IEnumerable<T>> action)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than zero.");

            int totalItems = source.Count();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            for (int pageIndex = 0; pageIndex < totalPages; pageIndex++)
            {
                var page = source.Skip(pageIndex * pageSize).Take(pageSize);
                action(page);
            }
        }
    }
}

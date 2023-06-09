﻿using DataLayer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

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

        public Task<int> SaveChangeAsync()
        {
            return _context.SaveChangesAsync();
        }

        public IQueryable<T> Queryable()
        {
            return _dbSet;
        }

        public IQueryable<T> QueryableNoTracking()
        {
            return _dbSet.AsNoTracking();
        }

        public T Add(T entity)
        {
            _dbSet.Add(entity);

            return entity;
        }

        public void MarkAsChanged(T entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void MarkAsChangedNotracking(T entity)
        {
            if(entity!=null)
            {
                _context.Entry(entity).State = EntityState.Modified;
            }
        }

        public T AttachedOrGet(Func<T, bool> predicate, T entity)
        {
            var match = _dbSet.Local.FirstOrDefault(predicate);
            if (match == null)
            {
                _dbSet.Attach(entity);

                return entity;
            }

            return match;
        }

        public List<ObjectDiff> UpdateDiff(T entity, object updateValueDto)
        {
            var diff = CheckUpdateObject(entity, updateValueDto);
            _context.Entry(entity).CurrentValues.SetValues(updateValueDto);

            return diff;
        }

        private List<ObjectDiff> CheckUpdateObject(object originalObj, object updateObj)
        {
            var items = new List<ObjectDiff>();

            foreach (var updateProperty in updateObj.GetType().GetProperties())
            {
                var originProp = originalObj.GetType().GetProperty(updateProperty.Name);

                if(originProp == null) continue; //bypass do not exists property.

                var originPropValue = originProp.GetValue(originalObj, null);
                var updatePropValue = updateProperty.GetValue(updateObj, null);

                if (!updatePropValue.Equals( originPropValue))
                {
                    items.Add(new ObjectDiff { Name = originProp.Name, OldValue = originPropValue, NewValue = updatePropValue });
                }
            }

            if(items.Any())
            {
                Console.WriteLine("================ DIFF ============\n");
                Console.WriteLine(JsonConvert.SerializeObject(items));
                Console.WriteLine("\n================ END DIFF ============");
            }

            return items;
        }
    }

    public class ObjectDiff
    {
        public string Name { get; set; }
        public object OldValue { get; set; }
        public object NewValue { get; set; }
    }
}

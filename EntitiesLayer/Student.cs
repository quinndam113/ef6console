using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntitiesLayer
{

    public class Student
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public byte[] Photo { get; set; }
        public decimal Height { get; set; }
        public float Weight { get; set; }

        public int GradeId { get; set; }
        [ForeignKey("GradeId")]
        public Grade Grade { get; set; }
    }

    public class StudentAuditMap : HistoryableMap<Student>
    {
        public StudentAuditMap()
        {
            CreateForeignMap<Grade>(foreignKey: "GradeId", foreignDisplayProperty: "GradeName", caption: "Học Sinh");

            SkipAudit("Height");
        }
    }

    public class Grade
    {
        public int GradeId { get; set; }
        public string GradeName { get; set; }
        public string Section { get; set; }
        public ICollection<Student> Students { get; set; }
    }

    public class HistoryableMap<T>
    {
        public List<HistoryableMapConfig> Maps { get; private set; }
        public List<string> SkipAuditProperties { get; private set; }

        public HistoryableMap()
        {
            Maps = new List<HistoryableMapConfig>();
            SkipAuditProperties = new List<string>();
        }

        public void CreateForeignMap<TForeignEntity>(string foreignKey,
            string foreignDisplayProperty,
            string caption = null) where TForeignEntity : class
        {
            var typeName = typeof(T).Name;
            var mapConfig = new HistoryableMapConfig
            {
                PropertyName = foreignKey,
                DisplayName = caption ?? typeName,
                ForeignEntityType = typeof(TForeignEntity),
                ForeignEntityNameProperty = foreignDisplayProperty
            };

            Maps.Add(mapConfig);
        }

        //public void CreateMap<TForeignEntity>(Expression<Func<T, object>> property,
        //    Expression<Func<TForeignEntity, object>> foreignproperty,
        //    string displayName = null) where TForeignEntity: class
        //{
        //    var typeName = typeof(T).Name;
        //    var propertyName = ((MemberExpression)property.Body).Member.Name;
        //    var foreignEntityName = ((MemberExpression)foreignproperty.Body).Member.Name;

        //    var mapConfig = new HistoryableMapConfig
        //    {
        //        PropertyName = propertyName,
        //        DisplayName = displayName ?? typeName,
        //        ForeignEntityType = typeof(TForeignEntity),
        //        ForeignEntityNameProperty = foreignEntityName
        //    };

        //    Maps.Add(mapConfig);
        //}

        public void SkipAudit(string propertyName)
        {
            SkipAuditProperties.Add(propertyName);
        }
    }

    public class HistoryableMapConfig
    {
        public string PropertyName { get; set; }
        public string DisplayName { get; set; }
        public Type ForeignEntityType { get; set; }
        public string ForeignEntityNameProperty { get; set; }
    }
}

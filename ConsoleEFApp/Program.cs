using DataLayer;
using EntitiesLayer;
using RepositoryLayer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleEFApp
{
    internal class Program
    {
        static AuditForeigonValue FindAuditValue(SchoolContext ctx,
            string entityName,
            AuditValue auditValue)
        {
            var foreignKeys = ctx.GetForeignKeys(entityName);

            var fk = foreignKeys.FirstOrDefault(x => x.ForeignKeyPropertyName == auditValue.PropertyName);

            if(fk.ForeignKeyPropertyName == auditValue.PropertyName &&
                fk.EntityName == nameof(Grade))
            {
                var grade = ctx.Set<Grade>().Find(auditValue.Value);
                if (grade != null)
                {
                    return new AuditForeigonValue
                    {
                        Id = auditValue.Value.ToString(),
                        Name = grade.GradeName
                    };
                }
            }

            return null;
        }

        static async Task Main(string[] args)
        {
            var ctx = new SchoolContext();
            var studentRepo = new Repository<Student>(ctx);
            var gradeRepo = new Repository<Grade>(ctx);

            var s = new Student
            {
                StudentName = "test stu 1",
                GradeId = 6
            };
            studentRepo.Add(s);
            ctx.SaveChanges();


            var firstStu = studentRepo.Queryable().FirstOrDefault();
            firstStu.StudentName = $"{DateTime.Now:ddMMyyyyHHmmss} Student Name";
            firstStu.GradeId = 8;

            ctx.SaveChanges();

            var html = "";
            var audits = ctx.Set<HistoryEntryChange>()
                .Where(x => x.EntityId == firstStu.StudentID.ToString() && x.EntityName == "Student")
                .OrderByDescending(x => x.CreatedDate)
                .ToList();

            foreach (var audit in audits)
            {
                html += $"{audit.CreatedDate:HHmmss}\n";
                var origin = audit.OldValues.ToAuditValues();
                foreach (var auditValue in origin)
                {
                    if(auditValue.PropertyName == "GradeId")
                    {
                        var value = FindAuditValue(ctx, audit.EntityName, auditValue);
                        html += $"Origin-{auditValue.PropertyName}({auditValue.Value}): {value?.Name}\n";
                    }
                    else
                    {
                        html += $"Origin-{auditValue.PropertyName}: {auditValue.Value}\n";
                    }
                }

                var newV = audit.NewValues.ToAuditValues();
                foreach (var auditValue in newV)
                {
                    if (auditValue.PropertyName == "GradeId")
                    {
                        var value = FindAuditValue(ctx,audit.EntityName, auditValue);
                        html += $"New-{auditValue.PropertyName}({auditValue.Value}): {value?.Name}\n";
                    }
                    else
                    {
                        html += $"New-{auditValue.PropertyName}: {auditValue.Value}\n";
                    }
                }

                //var newAuditValue = audit.NewValues.ToAuditValues();
                html += "================================\n\n";
            }

            Console.WriteLine(html);

            //var newGrade = new Grade
            //{
            //    GradeName = "firstTest"
            //};

            //gradeRepo.Add(newGrade);
            //ctx.SaveChanges();


            //g.GradeName = $"{DateTime.Now:ddMMyyyyHHmmss} Grade";
            //ctx.Grades.Remove(g);
            //ctx.SaveChanges();
            //var studentService = new StudentService(gradeRepo, studentRepo);

            //var faker = new Faker<Student>()
            //    .RuleFor(x => x.GradeId, f => 1)
            //    .RuleFor(x => x.StudentName, f => f.Name.FullName());

            //var students = faker.Generate(5000000);

            //var timer = new Stopwatch();
            //timer.Start();
            //ctx.BulkInsert(students);

            //Console.WriteLine("Save 100000 Done");

            //var students = ctx.Students.Take(10000).ToList();
            //var ids = ctx.Students.Select(x => x.StudentID).Take(10000).ToList();



            //ctx.Students.RemoveRange(students);
            //ctx.SaveChanges();
            //ctx.BulkDeleteIds<Student,int>(ids);
            //timer.Stop();
            //TimeSpan timeTaken = timer.Elapsed;
            //string foo = "Time taken: " + timeTaken.ToString(@"m\:ss\.fff");

            Console.ReadLine();
        }
    }
}

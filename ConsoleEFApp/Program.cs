
using Bogus;
using DataLayer;
using EntitiesLayer;
using RepositoryLayer;
using ServiceLayer;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ConsoleEFApp
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var ctx = new SchoolContext();
            var studentRepo = new Repository<Student>(ctx);
            var gradeRepo = new Repository<Grade>(ctx);

            var g = gradeRepo.Queryable().FirstOrDefault();
            g.GradeName = $"{DateTime.Now:ddMMyyyyHHmmss} Grade";
            ctx.SaveChanges();
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

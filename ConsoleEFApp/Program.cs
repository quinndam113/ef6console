
using DataLayer;
using EntitiesLayer;
using RepositoryLayer;
using ServiceLayer;
using System;
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

            var studentService = new StudentService(gradeRepo, studentRepo);

            var stu = new DtoLayer.StudentDto
            {
                StudentID = 2,
                StudentName = "Student 20",
                GradeId = 1,
                Pro = "123",
                Height = 400
            };

            // DEMO

            //case 1 - problem inside Logistics
            await studentService.UpdateAsync(stu);
            //case 1

            //case 2 update MarkAsChanged & diffUpdate
            //stu.Height = 500;
            //await studentService.DiffUpdateAsync(stu);
            //end case 2


            //NOTE: AsNoTracking Update should becarefull if another Entity Attactted to DbContext current Scope.
            ////case 3 update asNoTracking
            //await studentService.UpdateNoTrackingAsync(stu);
            ////end case 3

            ////case 4 update asNoTracking diff
            //await studentService.DiffUpdateNoTrackingAsync(stu);
            ////end case 4

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}

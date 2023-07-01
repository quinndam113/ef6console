
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

            var orginStu = await studentService.GetLatestStudentAsync();
            if (orginStu != null)
            {
                Console.WriteLine($"Origin Object: {orginStu.StudentID}  {orginStu.StudentName}");
            }
            else  {
                orginStu = await studentService.AddStudent(new DtoLayer.StudentDto
                {
                    StudentName = "Student 20",
                    GradeId = 1
                });
            }

            var stu = new DtoLayer.StudentDto
            {
                StudentID = orginStu.StudentID,
                StudentName = "Student 20",
                GradeId = 1,
                Pro = "123",
                Height = 400
            };

            // DEMO

            ////case 1 - problem inside Logistics
            //await studentService.UpdateAsync(stu);
            ////case 1

            ////case 2 update MarkAsChanged & diffUpdate
            //await studentService.DiffUpdateAsync(stu);
            ////end case 2

            //mix case 1 & 2
            await studentService.UpdateAsync(stu); //update all field
            stu.Height = 500;
            await studentService.DiffUpdateAsync(stu); // update Height only
                                                       //end mix case 1 & 2





            //ONE MORE THING






































            //Update an untracking Object after get with .AsNoTracking(). 

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

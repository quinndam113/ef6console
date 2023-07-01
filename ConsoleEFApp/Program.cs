
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

            // PREPARE DATA
            string sameStudentName = "Student 20";

            var orginStu = await studentService.GetLatestStudentAsync();
            if (orginStu != null)
            {
                Console.WriteLine($"Origin Object: {orginStu.StudentID}  {orginStu.StudentName}");
            }
            else  {
                orginStu = await studentService.AddStudent(new DtoLayer.StudentDto
                {
                    StudentName = sameStudentName,
                    GradeId = 1
                });
            }

            // GET STARTED
            var rand = new Random();
            var stu = new DtoLayer.StudentDto
            {
                StudentID = orginStu.StudentID,
                StudentName = sameStudentName,
                GradeId = 1,
                Pro = "123",
                Height = rand.Next() // <== Wantted update only
            };

            //case 0 - Normal way
            await studentService.UpdateNormalWayAsync(stu);
            //end case 0


























            ////case 1 - Logistics way - MarkAsChanged
            //await studentService.LogiscticUpdateAsync(stu);
            ////end case 1


























            ////case 2 - DiffUpdate way
            //await studentService.DiffUpdateAsync(stu);
            ////end case 2





















            // _context.Entry(entity).CurrentValues.SetValues(updateValueDto);
            // Summary:
            //     Sets the values of this dictionary by reading values out of the given object.
            //     The given object can be of any type. Any property on the object with a name that
            //     matches a property name in the dictionary and can be read will be read.

            //** Other properties will be ignored.
            //   This allows, for example, copying of properties from simple Data Transfer Objects (DTOs).



            //In the furture, needed to define UpdateDTO only, contains which field really to be edited only.
            // Its look like JsonPatch way use for [HttpPatch] Mr.Cuong present before.


            ////mix case 1 & 2
            //await studentService.LogiscticUpdateAsync(stu); //update all field

            //stu.Height = 500;
            //await studentService.DiffUpdateAsync(stu); // update Height only
            ////end mix case 1 & 2





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

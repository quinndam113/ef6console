using ConsoleEFApp.Models;
using DataLayer;
using EntitiesLayer;
using RepositoryLayer;
using ServiceLayer;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            var contents = await studentService.GetStudentGradesAsync();

            foreach (var content in contents)
            {
                Console.WriteLine($"{content.StudentName} - {content.StudentID}");
            }

            var stu = await studentService.GetStudentById(2);
            stu.StudentName = "Student 10";
            stu.GradeId = 1;

            await studentService.UpdateStudent(stu);

            Console.ReadLine();
        }
    }
}

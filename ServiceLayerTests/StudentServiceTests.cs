using DataLayer;
using EntitiesLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RepositoryLayer;
using ServiceLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayerTests
{
    [TestClass]
    public class StudentServiceTests
    {
        private IQueryable<Student> students;
        private IQueryable<Grade> grades;
        private Repository<Student> studentRepo;
        private Repository<Grade> gradeRepo;
        public StudentServiceTests()
        {
            students = new List<Student> {
                new Student { StudentID = 1, StudentName = "Hieu", GradeId = 1},
                new Student { StudentID = 2, StudentName = "Huy", GradeId = 1},
                new Student { StudentID = 3, StudentName = "Dam"},
            }.AsQueryable();

            grades = new List<Grade> {
                new Grade {  GradeId = 1, GradeName = "Grade 1"}
            }.AsQueryable();
        }

        private void SetupData()
        {

            var studentDbSetMock = students.AsAsyncMock();
            var gradeDbSetMock = grades.AsAsyncMock();

            var mockContext = new Mock<SchoolContext>();
            mockContext.Setup(c => c.Set<Student>()).Returns(studentDbSetMock.Object);
            mockContext.Setup(c => c.Set<Grade>()).Returns(gradeDbSetMock.Object);

            studentRepo = new Repository<Student>(mockContext.Object);
            gradeRepo = new Repository<Grade>(mockContext.Object);
        }

        [TestMethod]
        public async Task GetStudentById_ReturnNull()
        {
            SetupData();

            var service = new StudentService(gradeRepo, studentRepo);
            var result = await service.GetStudentByIdAsync(0);

            Assert.IsNull(result);
        }

        [TestMethod]
        public async Task GetStudentById_ReturnData()
        {
            SetupData();

            var service = new StudentService(gradeRepo, studentRepo);
            var result = await service.GetStudentByIdAsync(1);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task TestGdtGetStudentGrades_HasData()
        {
            SetupData();

            var service = new StudentService(gradeRepo, studentRepo);

            var result = await service.GetStudentGradesAsync();

            Assert.IsTrue(result.Any());
        }
    }
}

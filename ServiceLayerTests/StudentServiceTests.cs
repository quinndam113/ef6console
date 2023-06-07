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
        
        public StudentServiceTests() {
            
        }
        
        [TestMethod]
        public async Task TestGdtGetStudentGrades_HasData()
        {
            var studentQueryable = new List<Student> {
                new Student { StudentID = 1, StudentName  ="Hieu", GradeId = 1},
                new Student { StudentID = 2, StudentName  ="Hieu", GradeId = 1}
            }.AsQueryable() ;

            var gradeQueryable = new List<Grade> {
                new Grade {  GradeId = 1, GradeName = "Grade 1"}
            }.AsQueryable();

            var studentDbSetMock = studentQueryable.AsAsyncMock();
            var gradeDbSetMock = gradeQueryable.AsAsyncMock();

            var mockContext = new Mock<SchoolContext>();
            mockContext.Setup(c => c.Set<Student>()).Returns(studentDbSetMock.Object);
            mockContext.Setup(c => c.Set<Grade>()).Returns(gradeDbSetMock.Object);

            var studentRepo = new Repository<Student>(mockContext.Object);
            var gradeRepo = new Repository<Grade>(mockContext.Object);

            var service = new StudentService(gradeRepo, studentRepo);

            var result = await service.GetStudentGrades();

            Assert.IsTrue(result.Any());
        }
    }
}

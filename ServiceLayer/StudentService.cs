using DtoLayer;
using EntitiesLayer;
using RepositoryLayer;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace ServiceLayer
{
    public class StudentService
    {
        private readonly Repository<Student> _studentRepo;
        private readonly Repository<Grade> _gradeRepo;
        public StudentService(Repository<Grade> gradeRepo, Repository<Student> studentRepo)
        {
            _gradeRepo = gradeRepo;
            _studentRepo = studentRepo;
        }

        public Task<List<StudentGradeDto>> GetStudentGrades()
        {
            return _studentRepo.Queryable().LeftJoin(_gradeRepo.Queryable(),
                    student => student.GradeId,
                    grade => grade.GradeId,
                   (student, grade) => new { student, grade })
                   .Select(x => new StudentGradeDto { StudentID = x.student.StudentID, StudentName = x.student.StudentName,  GradeName = x.grade.GradeName, GradeId = x.grade.GradeId })
                   .ToListAsync();
        }
    }
}

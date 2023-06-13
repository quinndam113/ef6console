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

        public Task<List<StudentGradeDto>> GetStudentGradesAsync()
        {
            var query = _studentRepo.Queryable().LeftJoin(_gradeRepo.Queryable(),
                    student => student.GradeId,
                    grade => grade.GradeId,
                   (student, grade) => new
                   {
                       student,
                       GradeName = grade != null ? grade.GradeName : ""
                   })
                   .Select(x => new StudentGradeDto
                   {
                       StudentID = x.student.StudentID,
                       StudentName = x.student.StudentName,
                       GradeName = x.GradeName
                   }).AsNoTracking();

            return query.ToListAsync();
        }

        public async Task<StudentDto> GetStudentById(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return await _studentRepo.Queryable().Where(x => x.StudentID == id)
                                                 .Select(x => new StudentDto { StudentID = x.StudentID, StudentName = x.StudentName, GradeId = x.GradeId })
                                                 .FirstOrDefaultAsync();
        }

        public async Task<StudentDto> UpdateStudent(StudentDto stu)
        {
            var student = await _studentRepo.Queryable().FirstOrDefaultAsync(x => x.StudentID == stu.StudentID);

            if (student != null)
            {
                student.StudentName = stu.StudentName;
                student.GradeId = stu.GradeId;
            }

            _studentRepo.Update(stu.StudentID, student);

            await _studentRepo.SaveChangeAsync();

            return stu;
        }
    }
}

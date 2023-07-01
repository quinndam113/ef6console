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

        public async Task<StudentDto> AddStudent(StudentDto stu)
        {
            var s = _studentRepo.Add(new Student
            {
                StudentName = stu.StudentName,
                GradeId = stu.GradeId
            });

            await _studentRepo.SaveChangeAsync();

            stu.StudentID = s.StudentID;

            return stu;
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

        public async Task<StudentDto> GetStudentByIdAsync(int id)
        {
            if (id == 0)
            {
                return null;
            }

            return await _studentRepo.Queryable().Where(x => x.StudentID == id)
                                                 .Select(x => new StudentDto { StudentID = x.StudentID, StudentName = x.StudentName, GradeId = x.GradeId })
                                                 .AsNoTracking()
                                                 .FirstOrDefaultAsync();
        }

        public Task<StudentDto> GetLatestStudentAsync()
        {

            return  _studentRepo.Queryable()
                        .OrderByDescending(x => x.StudentID)
                        .Select(x => new StudentDto { StudentID = x.StudentID, StudentName = x.StudentName, GradeId = x.GradeId })
                        .AsNoTracking()
                        .FirstOrDefaultAsync();
        }

        public async Task<StudentDto> UpdateAsync(StudentDto stu)
        {
            var student = await _studentRepo.Queryable().FirstOrDefaultAsync(x => x.StudentID == stu.StudentID);

            if (student != null)
            {
                student.StudentName = stu.StudentName;
                student.GradeId = stu.GradeId;
                student.Height = stu.Height;

                _studentRepo.MarkAsChanged(student);

                await _studentRepo.SaveChangeAsync();
            }

            return null;
        }

        public async Task<StudentDto> DiffUpdateAsync(StudentDto stu)
        {
            var student = await _studentRepo.Queryable().FirstOrDefaultAsync(x => x.StudentID == stu.StudentID);

            if (student != null)
            {
                _studentRepo.UpdateDiff(student, stu);

                await _studentRepo.SaveChangeAsync();
            }

            return null;
        }

        public async Task<StudentDto> UpdateNoTrackingAsync(StudentDto stu)
        {
            var student = await _studentRepo.QueryableNoTracking().FirstOrDefaultAsync(x => x.StudentID == stu.StudentID);

            if (student != null)
            {
                student.StudentName = stu.StudentName;
                student.GradeId = stu.GradeId;
                student.Height = stu.Height;

                _studentRepo.MarkAsChangedNotracking(student);

                await _studentRepo.SaveChangeAsync();
            }

            return null;
        }

        public async Task<StudentDto> DiffUpdateNoTrackingAsync(StudentDto stu)
        {
            var student = await _studentRepo.QueryableNoTracking().FirstOrDefaultAsync(x => x.StudentID == stu.StudentID);

            if (student != null)
            {
                _studentRepo.UpdateDiffNotracking(student, stu);

                await _studentRepo.SaveChangeAsync();
            }

            return null;
        }
    }
}

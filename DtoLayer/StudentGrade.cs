using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoLayer
{
    public class StudentGradeDto
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }

        public int GradeId { get; set; }
        public string GradeName { get; set; }
    }
}

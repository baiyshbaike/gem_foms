using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dialysis.Shared.Dto
{
    public class Exam2Dto
    {
        public QualityExam2 QualityExam2 { get; set; }
        public List<QualityExam2Patient> QualityExam2Patient { get; set; }
    }
}
using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Dialysis.Shared.Dto
{
    public class Exam3Dto
    {
        public QualityExam3 QualityExam3 { get; set; }
        public List<QualityExam3Row> QualityExam3Row { get; set; }
    }
}
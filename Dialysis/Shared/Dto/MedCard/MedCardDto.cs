using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dialysis.Shared.Models;

namespace Dialysis.Shared.Dto
    {

        public class MedCardDto
        {
        public MedCard MedCard { get; set; }
        public Patient Patient { get; set; }
        public MedCenter MedCenter { get; set; }
       
        public FirstAnalysis FirstAnalysis { get; set; }
        public FirstAnalysisResult FirstAnalysisResult { get; set; }
        public FirstCardiovascular FirstCardiovascular { get; set; }
        public FirstConfectionery FirstConfectionery { get; set; }
        public FirstEndocrine FirstEndocrine { get; set; }
        public FirstInspection FirstInspection { get; set; }
        public FirstNeuro FirstNeuro { get; set; }
        public FirstRespiratory FirstRespiratory { get; set; }
        public FirstUrogenital FirstUrogenital { get; set; }

        public List<HDSessionDto> HDSessions { get; set; }
        public List<Epicrisis> Epicrisises { get; set; }
        public List<AnalysisResultsDto> AnalysisResultsDtos { get; set; }

    }

    public class MedCardArgs
    {
        public MedCard MedCard { get; set; }
        public FirstCardiovascular FirstCardiovascular { get; set; }
        public FirstConfectionery FirstConfectionery { get; set; }
        public FirstEndocrine FirstEndocrine { get; set; }
        public FirstInspection FirstInspection { get; set; }
        public FirstNeuro FirstNeuro { get; set; }
        public FirstRespiratory FirstRespiratory { get; set; }
        public FirstUrogenital FirstUrogenital { get; set; }
        public FirstAnalysis FirstAnalysis { get; set; }
    }


}


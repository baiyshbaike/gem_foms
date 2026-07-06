using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace Dialysis.Shared.Dto
    {

        public class MedCardListDto
        {
        public MedCard MedCard { get; set; }
        public Patient Patient { get; set; }
        public MedCenter MedCenter { get; set; }

    }


    }


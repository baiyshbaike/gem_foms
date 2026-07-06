using Dialysis.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Params
{
    public class MachineAddParams
    {
        public string? FileName { get; set; }
        public byte[]? FileContent { get; set; }
        public MedCenterMachine MedCenterMachine { get; set; }
    }
}

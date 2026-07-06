using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Constants
{
    public enum HDSessionEnum: long
    {
        Identification = 1,
        Started = 2,
        Paused = 3,
        Finished =4,
        SendToPay = 5,
        Payed = 6,
        PayedAct = 7,
        Stopped = 8,
        Failed = 9,
        Rejected = 10,
        EndIdentification = 11,
        Ended = 12
    }
}

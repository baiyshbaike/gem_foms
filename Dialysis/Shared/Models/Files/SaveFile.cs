using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace Dialysis.Shared.Models.Files
{
    public class SaveFile
    {
        public long Id { get; set; }
        public long? EntityId { get; set; }
        public string ContentType { get; set; }
        public string Base64File { get; set; }
        public string FileName { get; set; }
        public byte[] Data { get; set; }
        public DateTime CreatedOn { get; set; }
        public long? CreatedBy { get; set; }
    }
}

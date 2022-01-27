using System;
using System.Collections.Generic;
using System.Text;

namespace SalesforceDemoApp
{
    internal class TopLayer
    {
        public int totalSize { get; set; }
        public bool done { get; set; }
        public List<sfRecord> records { get; set; }
        public string nextRecordsUrl { get; set; }
    }
    public class sfRecord
    {
        public string AccountId { get; set; }
        public string Email { get; set; }
    }
}

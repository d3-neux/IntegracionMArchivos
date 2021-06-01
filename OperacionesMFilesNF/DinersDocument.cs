using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using MFaaP.MFWSClient;
using Newtonsoft.Json;

namespace OperacionesMFiles
{

    public class Indexes
    {
        
    }

    public class Doc
    {
        public string id { get; set; }
        public string online { get; set; }
        public Indexes indexes { get; set; }
    }

    public class DinersDocument
    {
        public string numPagActual { get; set; }
        public string numTotalPag { get; set; }
        public string numTotalRegs { get; set; }

        public List<MFilesDocument> mfilesDocuments;
    }


}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VolebniPrukaz.API.Facebook
{
    [Serializable]
    public class Quick_Replies
    {
        public string content_type { get; set; }
        public string title { get; set; }
        public string payload { get; set; }
    }

    [Serializable]
    public class QuickReplyData
    {
        public string text { get; set; }
        public Quick_Replies[] quick_replies { get; set; }
    }
}
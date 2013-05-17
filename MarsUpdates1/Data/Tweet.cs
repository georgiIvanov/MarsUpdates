using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarsUpdates.Data
{
    struct Tweet
    {
        string content;
        string publisher;
        DateTime datePublished;
        string avaUrl;

        public Tweet(string content, string publisher, DateTime datePublished, string avaUrl)
        {
            this.content = content;
            this.publisher = publisher;
            this.datePublished = datePublished;
            this.avaUrl = avaUrl;
        }

        public string Content
        {
            get { return content; }
        }

        public string Publisher
        {
            get { return publisher; }
        }

        public DateTime DatePublished
        {
            get { return datePublished; }
        }

        public string GetAvatarUrl
        {
            get { return avaUrl; }
        }

        public string GetDateString
        {
            get { return datePublished.ToString("yyyy.MM.dd    HH:mm:ss"); }
        }
    }
}

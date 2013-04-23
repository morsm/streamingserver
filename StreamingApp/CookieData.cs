using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StreamingApp
{
    [Serializable]
    public class CookieData
    {
        private bool m_bFtp = false;

        public bool StreamToFtp
        {
            get { return m_bFtp; }
            set { m_bFtp = value; }
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StreamingApp
{
    public partial class Error : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get the exception
            Exception ex = Application[Global.ERROR_ID] as Exception;
            if (lbDetailedErrorMessage.Visible = (ex != null && ex is StreamingWebException))
            {
                lbDetailedErrorMessage.Text = ((StreamingWebException)ex).Message;
            }
        }
    }
}
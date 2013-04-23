using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace StreamingApp
{
    public partial class Channels : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Extract the row index from the command args
            int rowIndex = Int32.Parse(e.CommandArgument.ToString());

            // Get channel ID
            GridViewRow row = gviewChannels.Rows[rowIndex];
            int channelId = Int32.Parse(row.Cells[1].Text);

            // Get the global flags, e.g. whether to stream to remote FTP or not
            CookieData data = Global.GetCookieData(Request);

            ServiceConnection conn = new ServiceConnection();
            conn.StartStreaming(channelId, data.StreamToFtp);

            Response.Redirect("~/Default.aspx");

        }


    }
}

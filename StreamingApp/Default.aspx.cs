using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Termors.Services.Tv.MPStreamingInterface;

namespace StreamingApp
{
    public partial class _Default : System.Web.UI.Page
    {

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Anything streaming?
                ServiceConnection svc = new ServiceConnection();

                StreamingResult[] result = svc.GetCurrentStreamingResult();
                if (tbCurrent.Visible = result.Length > 0)
                {
                    FillCurrentTable(result);
                }
            }
        }

        private void FillCurrentTable(StreamingResult[] streamingResult)
        {
            // Only one stream implemented right now
            StreamingResult s = streamingResult[0];

            tbCurrentChannelRow.Text = s.Channel.Name;
            localUrlLink.HRef = localUrlLink.InnerText = s.LocalVideoUrl;
            remoteUrlLink.HRef = remoteUrlLink.InnerText = s.RemoteVideoUrl;
        }

        protected void OnStopClicked(object sender, EventArgs e)
        {
            ServiceConnection svc = new ServiceConnection();

            svc.StopStreaming(svc.GetCurrentStreamingResult()[0]);

            tbCurrent.Visible = false;
        }

    }
}

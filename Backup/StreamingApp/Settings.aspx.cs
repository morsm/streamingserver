using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Termors.Services.Tv.MPStreamingInterface;

namespace StreamingApp
{
    public partial class Settings : System.Web.UI.Page
    {
        private ServiceConnection service = new ServiceConnection();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lbSaved.Visible = false;

                StreamingSettings s = service.Configuration;
                CookieData data = Global.GetCookieData(Request);

                textAudioBitrate.Text = s.AudioBitrate.ToString();
                textFtpJobs.Text = s.FtpConcurrentJobs.ToString();
                textMaxVideoBitrate.Text = s.MaxVideoBitrate.ToString();
                textSegmentCount.Text = s.SegmentCount.ToString();
                textSegmentLength.Text = s.SegmentLength.ToString();
                textVideoBitrate.Text = s.VideoBitrate.ToString();

                string videoSize = s.VideoSize;
                ListItem existingItem = listVideoSize.Items.FindByText(videoSize);
                if (existingItem == null)
                {
                    existingItem = new ListItem(videoSize);
                    listVideoSize.Items.Add(existingItem);
                }

                int toSelect = listVideoSize.Items.IndexOf(existingItem);
                listVideoSize.SelectedIndex = toSelect;

                cbStreamToFtp.Checked = data.StreamToFtp;
            }
        }

        protected void butSave_Click(object sender, EventArgs e)
        {
            Validate();

            if (IsValid)
            {
                // Post settings to server
                StreamingSettings s = new StreamingSettings();

                s.AudioBitrate = UInt16.Parse(textAudioBitrate.Text);
                s.FtpConcurrentJobs = UInt16.Parse(textFtpJobs.Text);
                s.MaxVideoBitrate = UInt16.Parse(textMaxVideoBitrate.Text);
                s.SegmentCount = UInt16.Parse(textSegmentCount.Text);
                s.SegmentLength = UInt16.Parse(textSegmentLength.Text);
                s.VideoBitrate = UInt16.Parse(textVideoBitrate.Text);
                s.VideoSize = listVideoSize.SelectedValue;

                service.Configuration = s;

                CookieData data = new CookieData();
                data.StreamToFtp = cbStreamToFtp.Checked;
                Global.SetCookieData(Response, data);

                lbSaved.Visible = true;
            }
        }
    }
}
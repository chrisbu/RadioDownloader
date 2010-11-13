/* 
 * This file is part of the Podcast Provider for Radio Downloader.
 * Copyright © 2007-2010 Matt Robinson
 * 
 * This program is free software: you can redistribute it and/or modify it under the terms of the GNU General
 * Public License as published by the Free Software Foundation, either version 3 of the License, or (at your
 * option) any later version.
 * 
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the
 * implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public
 * License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with this program.  If not, see
 * <http://www.gnu.org/licenses/>.
 */

namespace PodcastProvider
{
    using System;
    using System.Net;
    using System.Windows.Forms;
    using System.Xml;

    internal partial class FindNew : Form
    {
        private PodcastProvider pluginInst;

        public FindNew(PodcastProvider pluginInst)
        {
            // This call is required by the Windows Form Designer.
            this.InitializeComponent();

            this.pluginInst = pluginInst;
        }

        private void cmdViewEps_Click(object sender, System.EventArgs e)
        {
            try
            {
                this.cmdViewEps.Enabled = false;
                this.lblResult.ForeColor = System.Drawing.Color.Black;
                this.lblResult.Text = "Checking feed...";

                Application.DoEvents();

                Uri feedUrl = null;
                XmlDocument rss = null;

                try
                {
                    feedUrl = new Uri(this.txtFeedURL.Text);
                }
                catch (UriFormatException)
                {
                    this.lblResult.Text = "The specified URL was not valid.";
                    this.lblResult.ForeColor = System.Drawing.Color.Red;
                    this.cmdViewEps.Enabled = true;
                    return;
                }

                // Test that we can load something from the URL, and it is valid XML
                try
                {
                    rss = this.pluginInst.LoadFeedXml(feedUrl);
                }
                catch (WebException)
                {
                    this.lblResult.Text = "There was a problem requesting the feed from the specified URL.";
                    this.lblResult.ForeColor = System.Drawing.Color.Red;
                    this.cmdViewEps.Enabled = true;
                    return;
                }
                catch (XmlException)
                {
                    this.lblResult.Text = "The data returned from the specified URL was not a valid RSS feed.";
                    this.lblResult.ForeColor = System.Drawing.Color.Red;
                    this.cmdViewEps.Enabled = true;
                    return;
                }

                // Finally, make sure that the required elements that we need (title and description) exist
                XmlNode checkTitle = rss.SelectSingleNode("./rss/channel/title");
                XmlNode checkDescription = rss.SelectSingleNode("./rss/channel/description");

                if (checkTitle == null | checkDescription == null)
                {
                    this.lblResult.Text = "The RSS feed returned from the specified URL was not valid.";
                    this.lblResult.ForeColor = System.Drawing.Color.Red;
                    this.cmdViewEps.Enabled = true;
                    return;
                }

                this.lblResult.Text = "Loading information...";
                Application.DoEvents();

                this.pluginInst.RaiseFoundNew(feedUrl.ToString());

                this.lblResult.Text = string.Empty;
                this.cmdViewEps.Enabled = true;
            }
            catch (Exception unandled)
            {
                this.pluginInst.RaiseFindNewException(unandled);
            }
        }

        private void txtFeedURL_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            try
            {
                if (e.KeyChar == (char)Keys.Enter)
                {
                    if (this.cmdViewEps.Enabled)
                    {
                        this.cmdViewEps_Click(sender, e);
                    }
                }
            }
            catch (Exception unhandled)
            {
                this.pluginInst.RaiseFindNewException(unhandled);
            }
        }
    }
}

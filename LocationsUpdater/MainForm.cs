using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Xml;

using GoArrow;

namespace LocationsUpdater {
	public partial class MainForm : Form {
		private WebClient downloader = new WebClient();
		private string downlodPath = "";

		private enum DownloadTypeEnum { CrossroadsOfDereth, AcSpedia }

		public MainForm() {
			InitializeComponent();
			downloadType.SelectedIndex = 0;
			downloader.DownloadProgressChanged += new DownloadProgressChangedEventHandler(downloader_DownloadProgressChanged);
			downloader.DownloadFileCompleted += new AsyncCompletedEventHandler(downloader_DownloadFileCompleted);
		}

		private void defaultButton_Click(object sender, EventArgs e) {

		}

		private void downloadButton_Click(object sender, EventArgs e) {
			Uri url;
			if (!Uri.TryCreate(locationsFileUrl.Text, UriKind.Absolute, out url)) {
				MessageBox.Show("The specified URL is invalid", "Invalid URL",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
				return;
			}

			switch (downloadType.SelectedIndex) {
				case (int)DownloadTypeEnum.CrossroadsOfDereth: // CoD
					downlodPath = Path.Combine(Application.StartupPath, "cod_locations.xml");
					break;
				case (int)DownloadTypeEnum.AcSpedia: // ACSpedia
					downlodPath = Path.Combine(Application.StartupPath, "acspedia_locations.xml");
					break;
				default:
					MessageBox.Show("Invalid database source selected: " + downloadType.SelectedItem,
						"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
			}

			downloadButton.Enabled = false;
			abortButton.Enabled = true;

			try {
				statusLabelA.Text = "Connecting...";
				downloader.DownloadFileAsync(url, downlodPath);
			}
			catch (Exception ex) {
				statusLabelA.Text = "Download Failed";
				downloadButton.Enabled = true;
				abortButton.Enabled = false;
				MessageBox.Show(ex.Message, "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void abortButton_Click(object sender, EventArgs e) {
			downloadButton.Enabled = true;
			abortButton.Enabled = false;
			downloader.CancelAsync();
		}

		string bytesToString(long bytes) {
			if (bytes < 1000)
				return bytes + " bytes";
			if (bytes < 1024 * 1000)
				return (bytes / 1024.0).ToString("0.00") + " KB";
			if (bytes < 1024 * 1024 * 1000)
				return (bytes / (1024.0 * 1024.0)).ToString("0.00") + " MB";
			return (bytes / (1024.0 * 1024.0 * 1024.0)).ToString("0.00") + " GB";
		}

		void downloader_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
			long totalBytes;
			int pct;
			if (e.TotalBytesToReceive < 0) {
				//totalBytes = Math.Max(3000 * 1024, e.BytesReceived); // Assume file is ~3,000 KB
				//pct = (int)(e.BytesReceived * 100 / totalBytes);
				downloadProgress.Style = ProgressBarStyle.Marquee;
				statusLabelA.Text = "Downloading...";
				statusLabelB.Text = bytesToString(e.BytesReceived);
			}
			else {
				totalBytes = e.TotalBytesToReceive;
				pct = e.ProgressPercentage;
				statusLabelA.Text = "Downloading... " + pct + "%";
				statusLabelB.Text = bytesToString(e.BytesReceived) + " / " + bytesToString(totalBytes);
				downloadProgress.Value = pct;
			}
		}

		void downloader_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e) {
			downloadButton.Enabled = true;
			abortButton.Enabled = false;
			if (e.Error != null) {
				statusLabelA.Text = "Download Failed";
				MessageBox.Show(e.Error.Message, "Download Failed",
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
			else if (e.Cancelled) {
				statusLabelA.Text = "Canceled";
			}
			else {
				downloadProgress.Value = 0;
				downloadProgress.Style = ProgressBarStyle.Blocks;
				statusLabelA.Text = "Converting...";
				statusLabelB.Text = "";

				XmlDocument dnldXml = new XmlDocument();
				try {
					dnldXml.Load(downlodPath);
				}
				catch (Exception ex) {
					statusLabelA.Text = "Failed to open downloaded file";
					MessageBox.Show("Failed to open downloaded file: \n" + ex.Message,
						"Failed to open downloaded file", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				XmlDocument locXml = new XmlDocument();
				locXml.AppendChild(locXml.CreateElement("locations"));

				foreach (XmlElement locEle in dnldXml.DocumentElement.GetElementsByTagName("location")) {
					Location loc;
					if (downloadType.SelectedIndex == (int)DownloadTypeEnum.CrossroadsOfDereth)
						loc = GoArrow.Location.FromXmlWarcry(locEle);
					else
						loc = GoArrow.Location.FromXmlAcSpedia(locEle);
					locXml.DocumentElement.AppendChild(loc.ToXml(locXml));
				}

				try {
					XmlWriterSettings writerSettings = new XmlWriterSettings();
					writerSettings.Indent = true;
					writerSettings.IndentChars = "    ";
					locXml.Save(XmlWriter.Create(Path.Combine(Application.StartupPath, "locations.xml")));
				}
				catch (Exception ex) {
					statusLabelA.Text = "Failed to save locations.xml";
					MessageBox.Show(ex.Message, "Failed to save locations.xml",
						MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}

				statusLabelA.Text = "Download completed successfully";
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
			Application.Exit();
		}
	}
}
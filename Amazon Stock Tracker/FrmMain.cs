/**
 * This file is part of Amazon Stock Tracker <https://github.com/StevenJDH/Amazon-Stock-Tracker>.
 * Copyright (C) 2021 Steven Jenkins De Haro.
 *
 * Amazon Stock Tracker is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * Amazon Stock Tracker is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with Amazon Stock Tracker.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon.Runtime;
using Amazon_Stock_Tracker.Classes;
using Amazon_Stock_Tracker.Extensions;
using Amazon_Stock_Tracker.Models;
using Amazon_Stock_Tracker.Services;
using static Amazon_Stock_Tracker.Models.ProductDetails;

namespace Amazon_Stock_Tracker
{
    public partial class FrmMain : Form
    {
        private readonly AppConfiguration _config;
        private readonly SpeechSynthesizer _synthesizer;
        private IEnumerable<INotificationService> _notifications;
        private Task _checkStockTask;
        private string _selectedItemUrl;

        enum Columns
        {
            Store = 0,
            Item,
            Price,
            Status,
            LastInStock
        }

        public FrmMain()
        {
            InitializeComponent();
            _config = AppConfiguration.Instance;
            _synthesizer = new SpeechSynthesizer();
            _synthesizer.SetOutputToDefaultAudioDevice();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            RegisterNotificationServices();

            tmrStockChecker.Interval = 1000 * _config.Settings.CheckIntervalSeconds;
            
            if (!_config.Settings.LocalVoiceName.Equals("default", StringComparison.InvariantCultureIgnoreCase))
            {
                _synthesizer.SelectVoice(_config.Settings.LocalVoiceName);
            }

            foreach (var product in _config.Products)
            {
                ListViewItem entryListItem = listView1.Items.Add(product.Store);

                entryListItem.ForeColor = Color.Orange; // Must match ListView's ForeColor or it will be overridden when clicked.
                entryListItem.UseItemStyleForSubItems = false; // Disables inheritance of styles for sub-items.
                entryListItem.SubItems.Add(product.Name).ForeColor = Color.White;
                entryListItem.SubItems.Add("---").ForeColor = Color.White;
                entryListItem.SubItems.Add("---").ForeColor = Color.White;
                entryListItem.SubItems.Add("---").ForeColor = Color.White;

                product.WasNotified = true; // Here to disable notifications on first run.
            }
            
            // Simple workaround to remove horizontal scrollbars. The ^1 is an index from end expression.
            listView1.Columns[^1].Width += - 4 - SystemInformation.VerticalScrollBarWidth;

            int count = _config.Products.Count();

            toolStripStatus.Text = $"Loaded {count} item{(count == 1 ? "" : "s")} for in-stock status monitoring.";
        }

        private void FrmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            string checkMsg = _checkStockTask?.IsCompleted ?? true ? "" : "Checks are currently running. ";

            if (MessageBox.Show($"{checkMsg}Are you sure you want to exit?",Application.ProductName, 
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                e.Cancel = true;
                return;
            }

            tmrStockChecker.Stop();
        }

        private void RegisterNotificationServices()
        {
            var services = new List<INotificationService>();

            try
            {
                if (_config.Settings.AwsSmsEnabled)
                {
                    services.Add(new AmazonSnsService(phoneNumber: _config.Settings.AwsSmsNumber,
                        smsSenderId: _config.Settings.AwsSmsSenderId, smsType: _config.Settings.AwsSmsType,
                        smsMaxPrice: _config.Settings.AwsSmsMaxPrice,
                        smsMonthlySpendLimit: _config.Settings.AwsSmsMonthlySpendLimit,
                        serviceAccess: new AmazonServiceAccess(awsRegion: _config.Settings.AwsRegion,
                            awsProfile: _config.Settings.AwsProfile)));
                }

                if (_config.Settings.AwsEmailEnabled)
                {
                    services.Add(new AmazonSesService(email: _config.Settings.AwsEmailAddress,
                        serviceAccess: new AmazonServiceAccess(awsRegion: _config.Settings.AwsRegion,
                            awsProfile: _config.Settings.AwsProfile)));
                }
            }
            catch (AmazonServiceException ex)
            {
                MessageBox.Show($"Error: {ex.Message}", Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (_config.Settings.AzureVoiceEnabled)
            {
                services.Add(new AzureCognitiveSpeechService(subscriptionKey: _config.Settings.AzureVoiceKey,
                    serviceRegion: _config.Settings.AzureVoiceRegion, voiceName: _config.Settings.AzureVoiceName));
            }

            _notifications = services;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!btnCheck.Enabled)
            {
                MessageBox.Show("A stock check is already in progress.", Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            if (!Connection.IsInternetAvailable())
            {
                MessageBox.Show("A connection to the Internet was not detected.", 
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            btnStart.Enabled = false;
            mnuTestNotifications.Enabled = false;
            Application.DoEvents();
            _checkStockTask = CheckStockAsync(); // Initial check when starting up.
            tmrStockChecker.Start();
            btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            tmrStockChecker.Stop();
            mnuTestNotifications.Enabled = true;
            btnStart.Enabled = true;
        }

        private void tmrStockChecker_Tick(object sender, EventArgs e)
        {
            _checkStockTask = CheckStockAsync();
        }

        private async void btnCheck_Click(object sender, EventArgs e)
        {
            if (_checkStockTask?.IsCompleted == false)
            {
                MessageBox.Show("A stock check is already in progress.", Application.ProductName,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            if (!Connection.IsInternetAvailable())
            {
                MessageBox.Show("A connection to the Internet was not detected.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                return;
            }

            ToggleButtonState(btnCheck);
            await CheckStockAsync(); // ConfigureAwait needs to be the default of true.
            ToggleButtonState(btnCheck);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            mnuExit_Click(this, EventArgs.Empty);
        }

        private static void ToggleButtonState(Control btn)
        {
            btn.Enabled = !btn.Enabled;
            Application.DoEvents();
        }

        private async Task CheckStockAsync()
        {
            using IAmazonProductDataService amazon = new AmazonProductDataService(timeoutSeconds: 30);
            int count = _config.Products.Count();
            int newStock = 0;

            for (int i = 0; i < count; i++)
            {
                ProductDetails prodDetails;
                var product = _config.Products.ElementAt(i);

                toolStripStatus.Text = $"Checking [{i + 1} of {count}]: {product.Name} @ {product.Store}";

                try
                {
                    prodDetails = await amazon.GetProductDetailsAsync(store: product.Store, asin: product.Asin);
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine(ex.Message);
                    UpdateListViewEntry(index: i, new ProductDetails { Status = StockStatus.NotSupported });
                    
                    continue;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Debug.WriteLine($"Error: {ex.Message}");
                    UpdateListViewEntry(index: i, new ProductDetails { Status = StockStatus.Unavailable });

                    continue;
                }

                UpdateListViewEntry(index: i, prodDetails);
                
                if (prodDetails.Status == StockStatus.InStock && !product.WasNotified)
                {
                    string msg = _config.Settings.NotificationMessage
                        .Replace("{PRODUCT}", product.Name)
                        .Replace("{PRICE}", prodDetails.PriceTag)
                        .Replace("{STORE}", prodDetails.Store);

                    await NotifyInStockAsync(msg);
                    product.WasNotified = true;
                    newStock++;
                }
                else if (prodDetails.Status != StockStatus.InStock && product.WasNotified)
                {
                    // Was in stock, but not anymore, so we prep it for notifications when it's back in-stock.
                    product.WasNotified = false;
                }
            }

            toolStripStatus.Text = $"Finished status updates with {newStock} new detection{(newStock == 1 ? "" : "s")}.";
        }
        
        private void UpdateListViewEntry(int index, ProductDetails prodDetails)
        {
            switch (prodDetails.Status)
            {
                case StockStatus.InStock:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = 
                        String.IsNullOrWhiteSpace(prodDetails.PriceTag) ? "---" : prodDetails.PriceTag;
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "In-Stock";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.LightGreen;
                    listView1.Items[index].SubItems[(int)Columns.LastInStock].Text =
                        DateTime.Now.ToString("ddd, dd MMM yyy h:mm tt");
                    break;
                case StockStatus.IsRedirected:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = "---";
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Redirected";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.Yellow;
                    break;
                case StockStatus.HasCaptcha:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = "---";
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Captcha";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.BurlyWood;
                    break;
                case StockStatus.NotSupported:
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Not Supported";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.DeepSkyBlue;
                    break;
                case StockStatus.Unavailable:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = "---";
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Unavailable";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.PowderBlue;
                    break;
                default:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = "---";
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Out of Stock";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.Red;
                    break;
            }
        }

        /// <summary>
        /// Issues an In-Stock notification in parallel for services that implement <see cref="INotificationService"/>.
        /// </summary>
        /// <param name="message">The message to use for the notification.</param>
        /// <returns>A <see cref="Task"/> representing an async operation.</returns>
        private async Task NotifyInStockAsync(string message)
        {
            if (!_config.Settings.AzureVoiceEnabled)
            {
                _synthesizer.SpeakAsync(message);
            }

            await _notifications.ParallelForEachAsync(async service => 
                    await service.SendNotificationAsync(message), maxDegreeOfParallelism: 5);
        }

        private void mnuAwsAccount_Click(object sender, EventArgs e)
        {
            using var frm = new FrmAddAwsAccount();
            frm.ShowDialog(this);
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new();

            foreach (ListViewItem item in listView1.Items)
            {
                sb.AppendFormat("{0,-15}{1,-40}{2,-15}{3,-20}{4}\n", 
                    item.Text, 
                    item.SubItems[(int)Columns.Item].Text,
                    item.SubItems[(int)Columns.Price].Text, 
                    item.SubItems[(int)Columns.Status].Text,
                    item.SubItems[(int)Columns.LastInStock].Text);
            }

            Clipboard.SetText(sb.ToString().TrimEnd());
        }

        private async void mnuTestNotifications_Click(object sender, EventArgs e)
        {
            int count = 0;

            foreach (ListViewItem item in listView1.Items)
            {
                if (item.SubItems[(int)Columns.Status].Text.Equals("In-Stock"))
                {
                    string msg = _config.Settings.NotificationMessage
                        .Replace("{PRODUCT}", item.SubItems[(int)Columns.Item].Text)
                        .Replace("{PRICE}", item.SubItems[(int)Columns.Price].Text)
                        .Replace("{STORE}", item.SubItems[(int)Columns.Store].Text);

                    await NotifyInStockAsync(msg).ConfigureAwait(false);
                    count++;
                }
            }

            // With ConfigureAwait(false) to reduce context switches, this is needed to model parent on UI thread.
            this.Invoke(new Action(() =>
            {
                MessageBox.Show(this, $"A notification should have triggered for {count} in-stock item{(count == 1 ? "" : "s")}.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
        }

        private void mnuDonate_Click(object sender, EventArgs e)
        {
            OpenWebsite("https://www.paypal.me/stevenjdh/5");
        }

        private void mnuCheckUpdates_Click(object sender, EventArgs e)
        {
            OpenWebsite("https://github.com/StevenJDH/Amazon-Stock-Tracker/releases/latest");
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            using var frm = new FrmAbout();
            frm.ShowDialog(this);
        }

        /// <summary>
        /// Sends a URL to the operating system to have it open in the default web browser.
        /// </summary>
        /// <param name="url">URL of website to open.</param>
        public static void OpenWebsite(string url)
        {
            try
            {
                // UseShellExecute is false on .NET/Core, so we set it like .NET Framework to avoid Win32Exception.
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception ex) 
            {
                // Consuming exceptions.
                Debug.WriteLine(ex.Message);		
            }
        }

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                string store = e.Item.SubItems[(int)Columns.Store].Text;
                string asin = _config.Products.ElementAt(e.ItemIndex).Asin;
                
                _selectedItemUrl = $"https://www.{store}/dp/{asin}";
                e.Item.Selected = false;
                e.Item.Focused = false;
            }
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(_selectedItemUrl))
            {
                OpenWebsite(_selectedItemUrl);
            }
        }

        private void listView1_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            // Disables column resizing.
            e.NewWidth = listView1.Columns[e.ColumnIndex].Width;
            e.Cancel = true;
        }

        protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
        {
            base.ScaleControl(factor, specified);
            ScaleListViewColumns(listView1, factor);
        }

        /// <summary>
        /// Scales ListView columns to match system DPI since this is not done automatically.
        /// </summary>
        /// <param name="listView">ListView control to apply column scaling to.</param>
        /// <param name="factor">Scale factor based on system DPI settings.</param>
        private static void ScaleListViewColumns(ListView listView, SizeF factor)
        {
            foreach (ColumnHeader column in listView.Columns)
            {
                column.Width = (int)Math.Round(column.Width * factor.Width);
            }
        }
    }
}

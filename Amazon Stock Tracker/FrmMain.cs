﻿/**
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
using Amazon_Stock_Tracker.Classes;
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

                entryListItem.ForeColor = Color.Orange;
                entryListItem.UseItemStyleForSubItems = false; // Disables inheritance of styles for sub-items.
                entryListItem.SubItems.Add(product.Name);
                entryListItem.SubItems.Add("---");
                entryListItem.SubItems.Add("---");
                entryListItem.SubItems.Add("---");

                product.WasNotified = true; // Here to disable notifications on first run.
            }

            // Simple workaround to remove horizontal scrollbars. The ^1 is an index from end expression.
            listView1.Columns[^1].Width += - 4 - SystemInformation.VerticalScrollBarWidth;
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

            foreach (var service in _notifications)
            {
                service.Dispose();
            }
        }

        private void RegisterNotificationServices()
        {
            var services = new List<INotificationService>();

            if (_config.Settings.AwsSmsEnabled)
            {
                services.Add(new AmazonSnsService(phoneNumber: _config.Settings.AwsSmsNumber,
                    awsRegion: _config.Settings.AwsRegion, smsSenderId: _config.Settings.AwsSmsSenderId,
                    smsType: _config.Settings.AwsSmsType, smsMaxPrice: _config.Settings.AwsSmsMaxPrice,
                    smsMonthlySpendLimit: _config.Settings.AwsSmsMonthlySpendLimit,
                    awsProfile: _config.Settings.AwsProfile));
            }

            if (_config.Settings.AwsEmailEnabled)
            {
                services.Add(new AmazonSesService(email: _config.Settings.AwsEmailAddress,
                    awsRegion: _config.Settings.AwsRegion, awsProfile: _config.Settings.AwsProfile));
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

            ToggleButtonState(btnCheck);
            await CheckStockAsync();
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

            for (int i = 0; i < _config.Products.Count(); i++)
            {
                ProductDetails prodDetails;
                var product = _config.Products.ElementAt(i);

                try
                {
                    prodDetails = await amazon.GetProductDetailsAsync(store: product.Store, asin: product.Asin);
                }
                catch (ArgumentException ex)
                {
                    Debug.WriteLine(ex.Message);
                    UpdateListViewEntry(index: i, null);
                    
                    continue;
                }
                catch (Exception ex) when (ex is HttpRequestException || ex is TaskCanceledException)
                {
                    Debug.WriteLine($"Error: {ex.Message}");

                    continue;
                }

                UpdateListViewEntry(index: i, prodDetails);
                
                if (prodDetails.Status == StockStatus.InStock && !product.WasNotified)
                {
                    string msg = _config.Settings.NotificationMessage
                        .Replace("{PRODUCT}", product.Name)
                        .Replace("{PRICE}", prodDetails.PriceTag)
                        .Replace("{STORE}", prodDetails.Store);

                    UpdateListViewEntry(index: i, prodDetails);
                    await NotifyInStockAsync(msg);
                    product.WasNotified = true;
                }
                else if (prodDetails.Status != StockStatus.InStock && product.WasNotified)
                {
                    // Was in stock, but not anymore, so we prep it for notifications when it's back in-stock.
                    product.WasNotified = false;
                }
            }
        }
        
        private void UpdateListViewEntry(int index, ProductDetails prodDetails)
        {
            if (prodDetails == null)
            {
                listView1.Items[index].SubItems[(int)Columns.Status].Text = "Not Supported";
                listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.DeepSkyBlue;
                return;
            }

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
                default:
                    listView1.Items[index].SubItems[(int)Columns.Price].Text = "---";
                    listView1.Items[index].SubItems[(int)Columns.Status].Text = "Out of Stock";
                    listView1.Items[index].SubItems[(int)Columns.Status].ForeColor = Color.Red;
                    break;
            }
        }

        private async Task NotifyInStockAsync(string message)
        {
            if (!_config.Settings.AzureVoiceEnabled)
            {
                _synthesizer.SpeakAsync(message);
            }

            foreach (var service in _notifications)
            {
                await service.SendNotificationAsync(message); // TODO: Consider firing without waiting.
            }
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

                    await NotifyInStockAsync(msg);
                    ++count;
                }
            }

            MessageBox.Show($"A notification was triggered for {count} in-stock item{(count == 1 ? "" : "s")}.",
                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                e.Item.Selected = false;
                e.Item.Focused = false;
            }
        }
    }
}

﻿/*
 * This file is part of Amazon Stock Tracker <https://github.com/StevenJDH/Amazon-Stock-Tracker>.
 * Copyright (C) 2021-2022 Steven Jenkins De Haro.
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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon_Stock_Tracker.Components;

namespace Amazon_Stock_Tracker;

public partial class FrmAddAwsAccount : Form
{
    private readonly AppConfiguration _config;
    
    public FrmAddAwsAccount()
    {
        InitializeComponent();
        _config = AppConfiguration.Instance;
    }

    private void FrmAddAwsAccount_Load(object sender, EventArgs e)
    {
        foreach (var endpoint in RegionEndpoint.EnumerableAllRegions)
        {
            cmbAwsRegion.Items.Add(endpoint.DisplayName);
        }
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void cmbAwsRegion_SelectedIndexChanged(object sender, EventArgs e)
    {
        lblRegion.Text = RegionEndpoint.EnumerableAllRegions
            .FirstOrDefault(r => r.DisplayName.Equals(cmbAwsRegion.Text))?.SystemName ?? "";
            
    }

    private void btnSave_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrWhiteSpace(txtProfileName.Text) || 
            String.IsNullOrWhiteSpace(txtAccessKey.Text) ||
            String.IsNullOrWhiteSpace(txtSecretKey.Text) ||
            cmbAwsRegion.Text.Equals(""))
        {

            MessageBox.Show("All fields are required.", Application.ProductName,
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            return;
        }
        
        if (!RegisterAccount(txtProfileName.Text.Trim(), txtAccessKey.Text.Trim(),
                txtSecretKey.Text.Trim(), lblRegion.Text))
        {
            return;
        }

        _config.Settings.AwsProfile = txtProfileName.Text.Trim();
        _config.Settings.AwsRegion = lblRegion.Text;
        _config.SaveSettings();
        Close();
    }

    /// <summary>
    /// Creates a new profile in the AWS SDK Store for storing credentials in encrypted form.
    /// </summary>
    /// <remarks>
    /// The encrypted credentials in the SDK Store are located in the'%LOCALAPPDATA%\AWSToolkit'
    /// folder in the RegisteredAccounts.json file.
    /// </remarks>
    /// <param name="profileName">Profile name to associate with credentials.</param>
    /// <param name="accessKey">The access key to store with <paramref name="profileName"/>.</param>
    /// <param name="secretKey">The secret key to store with <paramref name="profileName"/>.</param>
    /// <param name="region">The AWS region to associate with <paramref name="profileName"/>.</param>
    /// <returns>Successful or not result.</returns>
    private static bool RegisterAccount(string profileName, string accessKey, string secretKey, string region)
    {
        var chain = new CredentialProfileStoreChain();

        if (chain.ListProfiles().Any(p => p.Name.Equals(profileName, 
            StringComparison.InvariantCultureIgnoreCase)))
        {
            MessageBox.Show("The profile name already exists in one or more locations.",
                Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        var options = new CredentialProfileOptions
        {
            AccessKey = accessKey,
            SecretKey = secretKey
        };
        
        var profile = new CredentialProfile(profileName, options);
        var netSdkFile = new NetSDKCredentialsFile();
        
        profile.Region = RegionEndpoint.GetBySystemName(region);
        netSdkFile.RegisterProfile(profile);

        MessageBox.Show("AWS account was stored successfully.",
            Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
        return true;
    }

    protected override void ScaleControl(SizeF factor, BoundsSpecified specified)
    {
        base.ScaleControl(factor, specified);

        // A fix to prevent cropping on high DPI screens.
        this.Width = (int)Math.Round(385 * factor.Width);
        this.Height = (int)Math.Round(296 * factor.Height);
    }
}

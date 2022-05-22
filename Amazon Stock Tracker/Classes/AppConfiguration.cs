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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Amazon_Stock_Tracker.Models;

namespace Amazon_Stock_Tracker.Classes
{
    sealed class AppConfiguration
    {
        private readonly string _configPath;
        private readonly string _productDataPath;
        private readonly JsonSerializerOptions _jsonOptions;

        /// <summary>
        /// Constructs a new <see cref="AppConfiguration"/> instance to manage application data.
        /// </summary>
        private AppConfiguration()
        {
            _configPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ASC-C", "Amazon Stock Tracker", "AmazonStockTrackerConfig.json");	
            _productDataPath = Path.Combine(Path.GetDirectoryName(_configPath)!, "AmazonStockTrackerProducts.json");
            _jsonOptions = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping, // Will escape even + if not set.
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreReadOnlyProperties = true,
                WriteIndented = true
            };

            try
            {
                LoadSettings();
                LoadProducts();
            }
            catch (IOException ex)
            {
                MessageBox.Show($"{ex.GetType().Name}: {ex.Message}",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Process.GetCurrentProcess().Kill();
            }
        }

        /// <summary>
        /// Gets a singleton instance of <see cref="AppConfiguration"/>.
        /// </summary>
        public static AppConfiguration Instance { get; } = new AppConfiguration();

        public ConfigSettings Settings { get; private set; }

        public IEnumerable<Product> Products { get; private set; }

        /// <summary>
        /// Sets up the configuration file and loads any settings used by the application.
        /// </summary>
        private void LoadSettings()
        {
            if (File.Exists(_configPath))
            {
                string jsonData = File.ReadAllText(_configPath);

                // Any trouble reading the configuration will just use the defaults.
                try
                {
                    Settings = JsonSerializer.Deserialize<ConfigSettings>(jsonData, _jsonOptions) ?? throw new JsonException();
                }
                catch (JsonException)
                {
                    ResetSettings(createBackup: true);
                    MessageBox.Show("Error: The configuration has been reset due to corrupt settings.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("A new configuration file will be created for first time use.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);

                ResetSettings(createBackup: false);

                MessageBox.Show("All done! You are now ready to start using the program.",
                    Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        /// <summary>
        /// Loads the list of Amazon products to track.
        /// </summary>
        private void LoadProducts()
        {
            if (File.Exists(_productDataPath))
            {
                string jsonData = File.ReadAllText(_productDataPath);

                try
                {
                    Products = JsonSerializer.Deserialize<IEnumerable<Product>>(jsonData, _jsonOptions);
                }
                catch (JsonException)
                {
                    Products = new List<Product>();
                    MessageBox.Show("Error: Invalid product data.",
                        Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                Products = new List<Product>();
                string jsonData = JsonSerializer.Serialize(Products, _jsonOptions);
                File.WriteAllText(_productDataPath, jsonData);
            }
        }

        /// <summary>
        /// Resets the application settings and creates an optional backup if an existing configuration
        /// file exists.
        /// </summary>
        /// <param name="createBackup">
        /// Set to <see langword="true" /> to create backup, <see langword="false" /> if not needed.
        /// </param>
        private void ResetSettings(bool createBackup)
        {
            if (createBackup && File.Exists(_configPath))
            {
                // Create configuration backup before resetting everything if it exists.
                File.Copy(_configPath, $"{_configPath}_{DateTime.Now:yyyy-MM-dd_HHmmss}.bak", overwrite: true);
            }

            Settings = new ConfigSettings
            {
                CheckIntervalSeconds = 120,
                NotificationMessage = "The {PRODUCT} is in stock for {PRICE}",
                LocalVoiceName = "default",
                AwsProfile = "default",
                AwsRegion = "eu-west-3",
                AwsSmsEnabled = false,
                AwsSmsNumber = "+1XXX5550100",
                AwsSmsSenderId = "default",
                AwsSmsType = "Promotional",
                AwsSmsMaxPrice = "0.50",
                AwsSmsMonthlySpendLimit = "1",
                AwsEmailEnabled = false,
                AwsEmailAddress = "success@simulator.amazonses.com",
                AzureVoiceEnabled = false,
                AzureVoiceName = "default",
                AzureVoiceKey = "xxxxxx",
                AzureVoiceRegion = "westeurope",
            };

            SaveSettings();
        }

        /// <summary>
        /// Saves the application settings to a configuration file.
        /// </summary>
        public void SaveSettings()
        {
            string jsonData = JsonSerializer.Serialize(Settings, _jsonOptions);

            // Builds any missing folders in path where the configuration will be stored.
            Directory.CreateDirectory(Path.GetDirectoryName(_configPath)!);
            // Saves the configuration to profile.
            File.WriteAllText(_configPath, jsonData);
        }
    }
}

/*
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

namespace Amazon_Stock_Tracker;

public partial class FrmAbout : Form
{
    public FrmAbout()
    {
        InitializeComponent();
        lblButton.BackColor = Color.Transparent;
    }

    private void btnOK_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void lblButton_Click(object sender, EventArgs e)
    {
        FrmMain.OpenWebsite("https://www.paypal.me/stevenjdh");
    }

    private void pnlButtonImage_Click(object sender, EventArgs e)
    {
        lblButton_Click(this, EventArgs.Empty);
    }

    private void FrmAbout_Load(object sender, EventArgs e)
    {
        lblTitleVer.Text = $"{Application.ProductName} v{Application.ProductVersion}";

        // We store the actual link this way in case we ever want to make changes to the link label.
        lnkGitHub.Links.Add(new LinkLabel.Link { LinkData = "https://github.com/StevenJDH/Amazon-Stock-Tracker" });
    }

    private void lnkGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        FrmMain.OpenWebsite(e.Link.LinkData.ToString()!);
    }
}

/* 
       \    /\
        )  ( ') (QUACK)
       (  /  )
        \(__)|
----------------------->
*/

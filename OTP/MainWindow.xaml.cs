// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OTP
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string username = txtUsername.Text;
                string password = txtPassword.Password;

                // Perform authentication logic
                // For testing, credentials are hardcoded as "user" and "pass"
                if (username == "user" && password == "pass")
                {
                    // Navigate to OTP_Page on successful login
                    Frame rootFrame = new Frame();
                    this.Content = rootFrame;

                    rootFrame.Navigate(typeof(OTP_Page));
                }
                else
                {
                    // Display error dialog for invalid credentials
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Login Failed",
                        Content = "Invalid username or password.",
                        CloseButtonText = "OK"
                    };
                    errorDialog.XamlRoot = this.Content.XamlRoot;
                    await errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions and display error dialog
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "An error occurred: " + ex.Message,
                    CloseButtonText = "OK"
                };
                await errorDialog.ShowAsync();
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            txtUsername.Text = "";
            txtPassword.Password = ""; // Consider using a secure method for clearing the password
        }
    }
}

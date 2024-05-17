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
using System.Net.Mail;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace OTP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 
    public sealed partial class OTP_Page : Page
    {
        private string randomCode;
        private const string fromMail = "otpassessmentvooleechun@gmail.com";
        private const string fromPassword = "ygnyejxrohkizikg";
        public static string to;

        public const int STATUS_EMAIL_OK = 0;
        public const int STATUS_EMAIL_FAIL = 1;
        public const int STATUS_EMAIL_INVALID = 2;

        public const int STATUS_OTP_OK = 0;
        public const int STATUS_OTP_FAIL = 1;
        public const int STATUS_OTP_TIMEOUT = 2;
        public const int STATUS_OTP_CONTINUE = 3;

        private readonly DispatcherTimer otpTimer;
        private bool timerElapsed;
        private int tries = 0;
        private const int maxTries = 10;
        private DateTime otpSentTime;

        public OTP_Page()
        {
            this.InitializeComponent();
            otpTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Tick every second
            };
            otpTimer.Tick += OtpTimer_Tick;

            verifyOTPButton.IsEnabled = false;
        }

        /// <summary>
        /// Handles the Send OTP button click event.
        /// </summary>
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            to = email_box.Text;
            int result = generate_OTP_email(to);

            switch (result)
            {
                case STATUS_EMAIL_OK:
                    resultText.Text = "Email containing OTP has been sent successfully";
                    otpSentTime = DateTime.Now; // Record the time the OTP was sent
                    otpTimer.Start(); // Start the timer when OTP is sent
                    timerElapsed = false; // Reset timer flag
                    tries = 0;
                    verifyOTPButton.IsEnabled = true;
                    email_box.IsEnabled = false;
                    break;
                case STATUS_EMAIL_FAIL:
                    resultText.Text = "Email address does not exist or sending to the email has failed.";
                    ClearFields();
                    break;
                case STATUS_EMAIL_INVALID:
                    resultText.Text = "Email address is invalid! It must be from the '@dso.org.sg' domain.";
                    ClearFields();
                    break;
                default:
                    resultText.Text = "An unknown error occurred.";
                    ClearFields();
                    break;
            }
        }

        /// <summary>
        /// Handles the Verify OTP button click event.
        /// </summary>
        private async void button2_Click(object sender, RoutedEventArgs e)
        {
            string enteredOTP = code_box.Text;

            if (timerElapsed)
            {
                resultText.Text = "Timeout: No valid OTP entered within 1 minute";
                ClearFields();
                return;
            }

            int result = await check_OTP(enteredOTP);

            switch (result)
            {
                case STATUS_OTP_OK:
                    resultText.Text = "OTP is valid and checked";
                    ClearFields();
                    otpTimer.Stop();
                    break;
                case STATUS_OTP_FAIL:
                    resultText.Text = "OTP is wrong after 10 tries";
                    ClearFields();
                    otpTimer.Stop();
                    break;
                case STATUS_OTP_TIMEOUT:
                    resultText.Text = "Timeout: No valid OTP entered within 1 minute";
                    ClearFields();
                    otpTimer.Stop();
                    break;
                case STATUS_OTP_CONTINUE:
                    resultText.Text = "Invalid OTP";
                    code_box.Text = "";
                    break;
                default:
                    resultText.Text = "An unknown error occurred.";
                    ClearFields();
                    otpTimer.Stop();
                    break;
            }
        }

        /// <summary>
        /// Handles the Reset button click event.
        /// </summary>
        private void button3_Click(object sender, RoutedEventArgs e)
        {
            tries = 0;
            email_box.IsEnabled = true;
            ClearFields();
            otpTimer.Stop();
        }

        /// <summary>
        /// Generates and sends an OTP email.
        /// </summary>
        private int generate_OTP_email(string user_email)
        {
            if (!user_email.EndsWith("@dso.org.sg"))
            {
                return STATUS_EMAIL_INVALID; // Invalid email domain
            }

            string messageBody;
            Random rand = new Random();
            randomCode = (rand.Next(999999)).ToString("D6"); // Ensure 6 digits with leading zeros if necessary
            messageBody = "Your OTP Code is " + randomCode + ". The code is valid for 1 minute";

            MailMessage message = new MailMessage
            {
                From = new MailAddress(fromMail),
                Body = messageBody,
                Subject = "OTP Code"
            };
            message.To.Add(user_email);

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromMail, fromPassword),
                EnableSsl = true
            };
            System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate (object s,
                System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                System.Security.Cryptography.X509Certificates.X509Chain chain,
                System.Net.Security.SslPolicyErrors sslPolicyErrors)
            {
                return true;
            };

            try
            {
                smtp.Send(message);
                return STATUS_EMAIL_OK; // Successfully sent
            }
            catch (Exception)
            {
                return STATUS_EMAIL_FAIL; // Failed to send
            }
        }

        /// <summary>
        /// Timer tick event handler to manage OTP timeout.
        /// </summary>
        private void OtpTimer_Tick(object sender, object e)
        {
            TimeSpan elapsed = DateTime.Now - otpSentTime;
            TimeSpan remainingTime = TimeSpan.FromMinutes(1) - elapsed;
            if (remainingTime <= TimeSpan.Zero)
            {
                timerElapsed = true;
                resultText.Text = "Timeout: No valid OTP entered within 1 minute";
                ClearFields();
                otpTimer.Stop();
            }
            else
            {
                UpdateTimerText(remainingTime);
            }
        }

        /// <summary>
        /// Checks the entered OTP.
        /// </summary>
        private async Task<int> check_OTP(string otp)
        {
            if (timerElapsed)
            {
                return STATUS_OTP_TIMEOUT;
            }

            if (tries < maxTries)
            {
                if (otp == randomCode)
                {
                    return STATUS_OTP_OK;
                }
                else
                {
                    tries++;
                    UpdateAttemptsText();
                    await Task.Delay(1000); // Delay for 1 second between attempts
                }
            }

            if (tries >= maxTries)
            {
                return STATUS_OTP_FAIL;
            }
            else
            {
                return STATUS_OTP_CONTINUE;
            }
        }

        /// <summary>
        /// Updates the timer text to show remaining time.
        /// </summary>
        private void UpdateTimerText(TimeSpan remainingTime)
        {
            timerText.Text = $"Time remaining: {remainingTime:mm\\:ss}";
        }

        /// <summary>
        /// Updates the attempts text to show current attempt count.
        /// </summary>
        private void UpdateAttemptsText()
        {
            attemptsText.Text = $"Attempts: {tries}/{maxTries}";
        }

        /// <summary>
        /// Clears input fields and resets relevant state variables.
        /// </summary>
        private void ClearFields()
        {
            email_box.Text = string.Empty;
            code_box.Text = string.Empty;
            randomCode = string.Empty;
            to = string.Empty;
            verifyOTPButton.IsEnabled = false;
            tries = 0;
            email_box.IsEnabled = true;
            timerElapsed = false;
            UpdateAttemptsText();
            timerText.Text = string.Empty;
        }
    }
}

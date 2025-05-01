using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Runtime.InteropServices;

namespace OverNuke
{
    public partial class Form1 : Form
    {
        private Label codesLabel;
        private NotifyIcon trayIcon;
        private Config config;
        private System.Windows.Forms.Keys toggleKey; // Use the correct Keys enum here

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const int WM_HOTKEY = 0x0312;

        public Form1()
        {
            InitializeComponent();
            config = Config.Load();
            toggleKey = (System.Windows.Forms.Keys)Enum.Parse(typeof(System.Windows.Forms.Keys), config.Hotkey); // Explicit reference

            InitializeOverlay();
            RegisterHotKey(this.Handle, HOTKEY_ID, 0, (int)toggleKey);
            SetupTrayIcon();
            FetchCodes();
        }

        private void InitializeOverlay()
        {
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            BackColor = Color.LimeGreen;
            TransparencyKey = Color.LimeGreen;
            Opacity = 0.75;
            Width = 300;
            Height = 150;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(config.LocationX, config.LocationY);

            codesLabel = new Label()
            {
                AutoSize = true,
                Font = new Font("Consolas", config.FontSize),
                ForeColor = config.GetFontColor(),
                BackColor = Color.Transparent,
                Location = new Point(10, 10)
            };

            Controls.Add(codesLabel);
        }

        private void FetchCodes()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless");
            chromeOptions.AddArgument("--disable-gpu");
            chromeOptions.AddArgument("--window-size=800,600");
            chromeOptions.AddArgument("user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64)");

            try
            {
                using (IWebDriver driver = new ChromeDriver(chromeOptions))
                {
                    driver.Navigate().GoToUrl("https://dev.nukacrypt.com/FO76/");
                    System.Threading.Thread.Sleep(5000); // Wait for JS to render

                    string pageSource = driver.PageSource;

                    // Extract code blocks following "Alpha", "Bravo", and "Charlie"
                    var matches = Regex.Matches(pageSource, @"(Alpha|Bravo|Charlie)[^0-9]*([0-9]{8})");

                    if (matches.Count >= 3)
                    {
                        var codes = matches.Cast<Match>()
                            .Select(m => $"{m.Groups[1].Value}: {FormatCode(m.Groups[2].Value)}") // Apply formatting to each code
                            .ToList();

                        // Join the codes with a newline so each code appears on a new line
                        codesLabel.Text = string.Join(Environment.NewLine, codes);
                    }
                    else
                    {
                        codesLabel.Text = "Codes not found.";
                    }
                }
            }
            catch (Exception ex)
            {
                codesLabel.Text = "Error: " + ex.Message;
            }
        }

        private string FormatCode(string code)
        {
            // Group the 8-digit code into 3-2-3 format: 123 45 678
            if (code.Length == 8)
            {
                return $"{code.Substring(0, 3)} {code.Substring(3, 2)} {code.Substring(5, 3)}";
            }
            return code; // Return code as is if not 8 digits long (shouldn't happen)
        }


        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application,
                Visible = true,
                Text = "Enclave Overlay"
            };

            var contextMenu = new ContextMenu();
            contextMenu.MenuItems.Add("Exit", (s, e) =>
            {
                UnregisterHotKey(this.Handle, HOTKEY_ID);
                trayIcon.Dispose();
                Application.Exit();
            });

            trayIcon.ContextMenu = contextMenu;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                this.Visible = !this.Visible;
            }
            base.WndProc(ref m);
        }
    }
}

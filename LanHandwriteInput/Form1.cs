using QRCoder;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LanHandwriteInput
{
    public partial class Form1 : Form
    {
        private const int DefaultPort = 18787;

        private LocalWebServer? _server;
        private PictureBox _qrPictureBox = null!;
        private TextBox _logTextBox = null!;
        private CheckBox _appendEnterCheckBox = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUi();
            StartServer();
        }

        private void BuildUi()
        {
            SuspendLayout();
            Controls.Clear();

            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(24, 22, 24, 22)
            };
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 2),
                Text = "局域网手写输入"
            };

            var hintLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8),
                Text = "手机和电脑连同一个 Wi-Fi，扫码打开页面。发送前请把电脑光标放到要输入文字的位置。"
            };

            _appendEnterCheckBox = new CheckBox
            {
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8),
                Text = "粘贴后自动回车"
            };

            var contentLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 12, 0, 0),
                RowCount = 1
            };
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 256));
            contentLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            contentLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var qrPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 20, 0),
                MinimumSize = new Size(240, 240)
            };

            _qrPictureBox = new PictureBox
            {
                Size = new Size(240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Top,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            qrPanel.Controls.Add(_qrPictureBox);

            var rightLayout = new TableLayoutPanel
            {
                ColumnCount = 1,
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                RowCount = 2
            };
            rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var qrHintLabel = new Label
            {
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 12),
                Text = "如果手机打不开：\r\n1. 确认手机和电脑在同一局域网。\r\n2. 允许 Windows 防火墙访问此程序。\r\n3. 重启本程序后重新扫码。"
            };

            _logTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Multiline = true,
                MinimumSize = new Size(260, 120),
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            rightLayout.Controls.Add(qrHintLabel, 0, 0);
            rightLayout.Controls.Add(_logTextBox, 0, 1);
            contentLayout.Controls.Add(qrPanel, 0, 0);
            contentLayout.Controls.Add(rightLayout, 1, 0);

            mainLayout.Controls.Add(titleLabel, 0, 0);
            mainLayout.Controls.Add(hintLabel, 0, 1);
            mainLayout.Controls.Add(_appendEnterCheckBox, 0, 2);
            mainLayout.Controls.Add(contentLayout, 0, 3);

            void UpdateWrapping()
            {
                hintLabel.MaximumSize = new Size(Math.Max(320, mainLayout.ClientSize.Width - mainLayout.Padding.Horizontal), 0);
                qrHintLabel.MaximumSize = new Size(Math.Max(260, rightLayout.ClientSize.Width), 0);
            }

            mainLayout.SizeChanged += (_, _) => UpdateWrapping();
            rightLayout.SizeChanged += (_, _) => UpdateWrapping();

            Controls.Add(mainLayout);
            UpdateWrapping();
            ResumeLayout(true);
        }

        private void StartServer()
        {
            StopServer();

            var port = DefaultPort;
            var ipAddress = GetBestLocalIPv4Address();
            if (ipAddress is null)
            {
                AppendLog("未找到局域网 IPv4 地址。");
                return;
            }

            var url = $"http://{ipAddress}:{port}/";

            try
            {
                _server = new LocalWebServer(port, HandlePhoneText);
                _server.Start();
                RenderQrCode(url);
                AppendLog("服务已启动。");
            }
            catch (Exception ex)
            {
                AppendLog($"启动失败：{ex.Message}");
                AppendLog($"启动失败：{ex}");
            }
        }

        private void StopServer()
        {
            _server?.Dispose();
            _server = null;
        }

        private void HandlePhoneText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            BeginInvoke(() =>
            {
                AppendLog($"收到：{text}");
                try
                {
                    WindowsTextInput.PasteText(text);
                    if (_appendEnterCheckBox.Checked)
                    {
                        WindowsTextInput.SendEnter();
                    }
                    AppendLog(_appendEnterCheckBox.Checked ? "已发送输入事件和回车。" : "已发送输入事件。");
                }
                catch (Exception ex)
                {
                    AppendLog($"输入失败：{ex.Message}");
                }
            });
        }

        private void RenderQrCode(string text)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var pngQrCode = new PngByteQRCode(data);
            var bytes = pngQrCode.GetGraphic(12);
            using var stream = new MemoryStream(bytes);
            _qrPictureBox.Image?.Dispose();
            _qrPictureBox.Image = Image.FromStream(stream);
        }

        private void AppendLog(string message)
        {
            _logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }

        private static string? GetBestLocalIPv4Address()
        {
            var addresses = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up)
                .Where(ni => ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => ip.Address.ToString())
                .Where(ip => !ip.StartsWith("169.254.", StringComparison.Ordinal))
                .ToList();

            return addresses.FirstOrDefault(ip => ip.StartsWith("192.168.", StringComparison.Ordinal))
                ?? addresses.FirstOrDefault(ip => ip.StartsWith("10.", StringComparison.Ordinal))
                ?? addresses.FirstOrDefault(ip => ip.StartsWith("172.", StringComparison.Ordinal))
                ?? addresses.FirstOrDefault();
        }
    }
}






using QRCoder;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace LanHandwriteInput
{
    public partial class Form1 : Form
    {
        private const int DefaultPort = 8787;

        private LocalWebServer? _server;
        private Label _statusLabel = null!;
        private TextBox _addressTextBox = null!;
        private PictureBox _qrPictureBox = null!;
        private Button _startButton = null!;
        private Button _stopButton = null!;
        private NumericUpDown _portBox = null!;
        private TextBox _logTextBox = null!;

        public Form1()
        {
            InitializeComponent();
            BuildUi();
            StartServer();
        }

        private void BuildUi()
        {
            var titleLabel = new Label
            {
                AutoSize = true,
                Font = new Font(Font.FontFamily, 18, FontStyle.Bold),
                Location = new Point(24, 22),
                Text = "局域网手写输入"
            };

            var hintLabel = new Label
            {
                AutoSize = false,
                Location = new Point(26, 62),
                Size = new Size(700, 48),
                Text = "手机和电脑连同一个 Wi-Fi，扫码打开页面。发送前请把电脑光标放到要输入文字的位置。"
            };

            var portLabel = new Label
            {
                AutoSize = true,
                Location = new Point(26, 124),
                Text = "端口"
            };

            _portBox = new NumericUpDown
            {
                Location = new Point(66, 120),
                Minimum = 1024,
                Maximum = 65535,
                Value = DefaultPort,
                Width = 92
            };

            _startButton = new Button
            {
                Location = new Point(176, 118),
                Size = new Size(92, 30),
                Text = "启动"
            };
            _startButton.Click += (_, _) => StartServer();

            _stopButton = new Button
            {
                Location = new Point(278, 118),
                Size = new Size(92, 30),
                Text = "停止"
            };
            _stopButton.Click += (_, _) => StopServer();

            _statusLabel = new Label
            {
                AutoSize = false,
                Location = new Point(390, 124),
                Size = new Size(330, 24),
                Text = "未启动"
            };

            var addressLabel = new Label
            {
                AutoSize = true,
                Location = new Point(26, 170),
                Text = "手机访问地址"
            };

            _addressTextBox = new TextBox
            {
                Location = new Point(26, 196),
                Size = new Size(460, 30),
                ReadOnly = true
            };

            var copyButton = new Button
            {
                Location = new Point(500, 195),
                Size = new Size(86, 30),
                Text = "复制"
            };
            copyButton.Click += (_, _) =>
            {
                if (!string.IsNullOrWhiteSpace(_addressTextBox.Text))
                {
                    Clipboard.SetText(_addressTextBox.Text);
                    AppendLog("已复制地址。");
                }
            };

            _qrPictureBox = new PictureBox
            {
                Location = new Point(26, 244),
                Size = new Size(240, 240),
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom
            };

            var qrHintLabel = new Label
            {
                AutoSize = false,
                Location = new Point(286, 246),
                Size = new Size(420, 82),
                Text = "如果手机打不开：\r\n1. 确认手机和电脑在同一局域网。\r\n2. 允许 Windows 防火墙访问此程序。\r\n3. 换一个端口后重新启动。"
            };

            _logTextBox = new TextBox
            {
                Location = new Point(286, 344),
                Size = new Size(420, 140),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical
            };

            Controls.AddRange(new Control[]
            {
                titleLabel,
                hintLabel,
                portLabel,
                _portBox,
                _startButton,
                _stopButton,
                _statusLabel,
                addressLabel,
                _addressTextBox,
                copyButton,
                _qrPictureBox,
                qrHintLabel,
                _logTextBox
            });
        }

        private void StartServer()
        {
            StopServer();

            var port = (int)_portBox.Value;
            var ipAddress = GetBestLocalIPv4Address();
            if (ipAddress is null)
            {
                SetStoppedUi("未找到局域网 IPv4 地址");
                return;
            }

            var url = $"http://{ipAddress}:{port}/";

            try
            {
                _server = new LocalWebServer(port, HandlePhoneText);
                _server.Start();
                _addressTextBox.Text = url;
                RenderQrCode(url);
                _statusLabel.Text = $"已启动：{DateTime.Now:HH:mm:ss}";
                _startButton.Enabled = false;
                _stopButton.Enabled = true;
                _portBox.Enabled = false;
                AppendLog($"服务已启动：{url}");
            }
            catch (Exception ex)
            {
                SetStoppedUi($"启动失败：{ex.Message}");
                AppendLog($"启动失败：{ex}");
            }
        }

        private void StopServer()
        {
            _server?.Dispose();
            _server = null;
            SetStoppedUi("已停止");
        }

        private void SetStoppedUi(string status)
        {
            _statusLabel.Text = status;
            _startButton.Enabled = true;
            _stopButton.Enabled = false;
            _portBox.Enabled = true;
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
                WindowsTextInput.PasteText(text);
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



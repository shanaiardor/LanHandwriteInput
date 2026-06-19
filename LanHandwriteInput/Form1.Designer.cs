namespace LanHandwriteInput
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _server?.Dispose();
                _qrToastForm?.Dispose();
                _qrCodeImage?.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;
            ClientSize = new Size(820, 620);
            MinimumSize = new Size(720, 560);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "局域网手写输入";
        }
    }
}

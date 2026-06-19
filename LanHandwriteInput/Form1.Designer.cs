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
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(760, 560);
            MinimumSize = new Size(680, 520);
            StartPosition = FormStartPosition.CenterScreen;
            Text = "局域网手写输入";
        }
    }
}

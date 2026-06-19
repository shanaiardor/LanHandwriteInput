using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LanHandwriteInput
{
    internal sealed class LocalWebServer : IDisposable
    {
        private readonly int _port;
        private readonly Action<string> _onTextReceived;
        private WebApplication? _app;

        public LocalWebServer(int port, Action<string> onTextReceived)
        {
            _port = port;
            _onTextReceived = onTextReceived;
        }

        public void Start()
        {
            var builder = WebApplication.CreateSlimBuilder();
            builder.WebHost.UseUrls($"http://0.0.0.0:{_port}");
            builder.Services.ConfigureHttpJsonOptions(options =>
            {
                options.SerializerOptions.PropertyNameCaseInsensitive = true;
            });

            _app = builder.Build();
            MapRoutes(_app);
            _app.StartAsync().GetAwaiter().GetResult();
        }

        private void MapRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/", () => Results.Content(BuildPageHtml(), "text/html; charset=utf-8"));

            app.MapPost("/send", (SendTextRequest request) =>
            {
                var text = request.Text ?? string.Empty;
                if (text.Length > 0)
                {
                    _onTextReceived(text);
                }

                return Results.Json(new { ok = true });
            });
        }

        public void Dispose()
        {
            if (_app is null)
            {
                return;
            }

            _app.StopAsync(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
            _app.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _app = null;
        }

        private static string BuildPageHtml()
        {
            return """
<!doctype html>
<html lang="zh-CN">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1,viewport-fit=cover">
<title>手写输入</title>
<style>
:root { color-scheme: light dark; font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif; }
body { margin: 0; padding: 18px; background: #f6f7f9; color: #17191c; }
main { max-width: 620px; margin: 0 auto; }
h1 { font-size: 24px; margin: 12px 0 16px; }
textarea { width: 100%; min-height: 190px; box-sizing: border-box; border: 1px solid #c9ced6; border-radius: 10px; padding: 14px; font-size: 34px; line-height: 1.35; resize: vertical; background: white; color: #111; }
.row { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-top: 12px; }
button { min-height: 48px; border: 0; border-radius: 10px; font-size: 18px; font-weight: 650; }
button:disabled { opacity: .65; }
.primary { background: #1769e0; color: white; }
.secondary { background: #e5e8ef; color: #17191c; }
.quick { grid-template-columns: repeat(3, 1fr); }
p { color: #586070; font-size: 14px; line-height: 1.6; }
#toast { min-height: 24px; margin-top: 12px; font-size: 15px; color: #17613a; }
@media (prefers-color-scheme: dark) {
  body { background: #111316; color: #f4f6f8; }
  textarea { background: #1b1f24; border-color: #3a424d; color: #f4f6f8; }
  .secondary { background: #2a3038; color: #f4f6f8; }
  p { color: #a7b0bd; }
}
</style>
</head>
<body>
<main>
<h1>手写输入</h1>
<textarea id="text" autofocus placeholder="用手机输入法在这里手写一个字或一段文字"></textarea>
<div class="row">
  <button class="primary" id="send">发送到电脑</button>
  <button class="secondary" id="clear">清空</button>
</div>
<div class="row quick">
  <button class="secondary" data-text="\n">换行</button>
  <button class="secondary" data-text=" ">空格</button>
  <button class="secondary" id="backspace">退格</button>
</div>
<p>发送前，把电脑光标放到目标输入框。发送后文字会输入到电脑当前活动窗口。</p>
<div id="toast"></div>
</main>
<script>
const text = document.getElementById('text');
const sendButton = document.getElementById('send');
const toast = document.getElementById('toast');
async function send(value) {
  if (!value) return false;
  toast.textContent = '发送中...';
  const res = await fetch('/send', {
    method: 'POST',
    headers: {'Content-Type': 'application/json'},
    body: JSON.stringify({ text: value })
  });
  if (!res.ok) throw new Error('send failed');
  toast.textContent = '已发送：' + value.replace(/\n/g, '换行');
  return true;
}
sendButton.addEventListener('click', async () => {
  const value = text.value;
  sendButton.disabled = true;
  try {
    if (await send(value)) text.value = '';
    text.focus();
  } catch {
    toast.textContent = '发送失败，请刷新页面重试';
  } finally {
    sendButton.disabled = false;
  }
});
document.getElementById('clear').addEventListener('click', () => { text.value = ''; text.focus(); });
document.querySelectorAll('[data-text]').forEach(button => {
  button.addEventListener('click', async () => {
    button.disabled = true;
    try { await send(button.dataset.text || ''); } catch { toast.textContent = '发送失败'; }
    finally { button.disabled = false; }
  });
});
document.getElementById('backspace').addEventListener('click', async () => {
  const button = document.getElementById('backspace');
  button.disabled = true;
  try { await send('\b'); } catch { toast.textContent = '发送失败'; }
  finally { button.disabled = false; }
});
</script>
</body>
</html>
""";
        }

        private sealed record SendTextRequest(string? Text);
    }
}


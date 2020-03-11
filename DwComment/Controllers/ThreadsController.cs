using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DwComment.Models;
using static DwComment.Config;
using System.Net.Mail;
using System.Text;

namespace DwComment.Controllers
{
    [Produces("application/json")]
    [Route("Threads")]
    public class ThreadsController : Controller
    {
        private readonly DwCommentContext _context;

        public ThreadsController(DwCommentContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetThread([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var comments = await _context.Comment
                .AsNoTracking()
                .Where(c => c.ThreadId == id)
                .Select(c => new
                {
                    id = c.Id,
                    nickname = c.Nickname,
                    link = c.Link,
                    content = c.Content,
                    time = c.Time,
                    forwardTo = c.ForwardTo
                })
                .ToListAsync();

            return Ok(comments);
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> PostComment([FromRoute] int id, [FromBody] Comment comment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (Encoding.UTF8.GetByteCount(comment.Nickname) > App.Config.Limitation.NicknameLength ||
                Encoding.UTF8.GetByteCount(comment.Content) > App.Config.Limitation.ContentLength ||
                (comment.Link != null && Encoding.UTF8.GetByteCount(comment.Link) > App.Config.Limitation.LinkLength))
            {
                return BadRequest("Some input fields are too long.");
            }

            var forwardTo = comment.ForwardTo > 0
                ? await _context.Comment.SingleOrDefaultAsync(c => c.Id == comment.ForwardTo)
                : null;
            if (comment.ForwardTo > 0 && forwardTo == null)
            {
                return BadRequest("The comment you are forwarding to does not exist.");
            }

            comment.ThreadId = id;
            comment.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            var replyTo = forwardTo?.Mail;

            new Task(async () =>
            {
                var url = App.Config.Mail.LinkTemplate
                    .Replace("{threadId}", comment.ThreadId.ToString())
                    .Replace("{id}", comment.Id.ToString());
                var body = $@"
<pre>{comment.Content.Replace("<", "&lt;").Replace(">", "&gt;")}</pre>
<hr>
请勿直接回复此邮件，你的回复将不会被对方收到，也不会公开发表在评论区。
<br>
邮件内未支持 Markdown，你现在看到的是未经格式化的原始内容。
<br>
<a href=""{url}"">转到网站评论区以回复或查看格式化后的内容</a>"; // TODO: 递归加载对话

                if (App.Config.ForwardNotification != null && replyTo != null)
                {
                    var subject = $"来自 {comment.Nickname} 的回复";

                    await App.Config.ForwardNotification.SendAsync(subject, body, new MailAddress(replyTo));
                }

                if (App.Config.SiteAdminNotification != null &&
                    App.Config.SiteAdminNotification.To.Address != replyTo)
                {
                    var subject = $"来自 {comment.Nickname} 的评论";

                    await App.Config.SiteAdminNotification.SendAsync(subject, body);
                }
            }).Start();

            return Ok();
        }
    }
}
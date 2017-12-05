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

            comment.ThreadId = id;
            comment.Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _context.Comment.Add(comment);
            await _context.SaveChangesAsync();

            var replyTo = (await _context.Comment
                .SingleOrDefaultAsync(c => c.Id == comment.ForwardTo))
                ?.Mail;

            new Task(async () =>
            {
                var url = LinkTemplate
                    .Replace("{threadId}", comment.ThreadId.ToString())
                    .Replace("{id}", comment.Id.ToString());
                var body = $"{comment.Content}<hr><a href=\"{url}\">转到网站</a>"; // TODO: 递归加载对话

                if (ForwardNotification != null && replyTo != null)
                {
                    var subject = $"来自 {comment.Nickname} 的回复";

                    await ForwardNotification.SendWait(subject, body, new MailAddress(replyTo));
                }

                if (SiteAdminNotification != null && SiteAdminNotification.To.Address != replyTo)
                {
                    var subject = $"来自 {comment.Nickname} 的评论";

                    await SiteAdminNotification.SendWait(subject, body);
                }
            }).Start();

            return Ok();
        }
    }
}
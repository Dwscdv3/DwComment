using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using DwComment.Models;
using static DwComment.Config;

namespace DwComment
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var mailSection = Configuration.GetSection("Mail");
            if (mailSection != null)
            {
                var host = mailSection.GetSection("Server").GetValue<string>("Host");
                var port = mailSection.GetSection("Server").GetValue<int>("Port");
                string username = mailSection.GetSection("Credential").GetValue<string>("Username");
                string password = mailSection.GetSection("Credential").GetValue<string>("Password");
                var displayAddress = mailSection.GetSection("Display").GetValue<string>("Address");
                var displayName = mailSection.GetSection("Display").GetValue<string>("Name");

                ForwardNotification = new MailNotification(
                    new SmtpClient
                    {
                        Host = host,
                        Port = port,
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    },
                    new MailAddress(displayAddress, displayName)
                );

                SiteAdminNotification = new MailNotification(
                    new SmtpClient
                    {
                        Host = host,
                        Port = port,
                        Credentials = new NetworkCredential(username, password),
                        EnableSsl = true
                    },
                    new MailAddress(displayAddress, displayName)
                )
                {
                    To = new MailAddress(mailSection.GetSection("SiteAdmin").GetValue<string>("Address"))
                };
            }
            LinkTemplate = mailSection.GetValue<string>("LinkTemplate");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddDbContext<DwCommentContext>(options =>
                    //options.UseSqlServer(Configuration.GetConnectionString("DwCommentContextSqlServer")));
                    options.UseSqlite(Configuration.GetConnectionString("DwCommentContextSqlite")));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}

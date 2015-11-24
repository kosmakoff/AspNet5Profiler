using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using AspNet5Profiler;
using Microsoft.Framework.Logging;
using Microsoft.AspNet.StaticFiles;
using System.Collections.Generic;
using System.Linq;

namespace ProfilerDemo
{
    public class Startup
    {
        private static readonly Random R = new Random();
        private ILogger _logger;

        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddProfiler();
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Debug;
            loggerFactory.AddConsole(LogLevel.Debug);

            _logger = loggerFactory.CreateLogger<Startup>();

            app.UseResponseBuffering(); // to allow buffering the response, which is vital to adding headers during arbitrary moment of time
            app.UseSendFileFallback(); // to allow simple send file implementation

            app.UseDeveloperExceptionPage();

            app.UseProfiler();

            app.Map("/somepath", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                    await context.Response.WriteAsync("Some Path");
                });
            });

            app.Map("/favicon.ico", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    using (context.GetProfiler().Step("FAVICON"))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));

                        context.Response.StatusCode = 200;
                        context.Response.ContentType = "image/ico";

                        context.Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                        context.Response.Headers.Add("Pragma", "no-cache");
                        context.Response.Headers.Add("Expires", "0");

                        await context.Response.SendFileAsync(@"C:\Temp\favicon.ico");

                        await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));
                    }
                });
            });

            app.Run(async context =>
            {
                using (context.GetProfiler().Step("MAIN"))
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/html; charset=utf-8";

                    using (context.GetProfiler().Step("SOMETHING TO DO PRIOR TO FULL PAGE RENDER"))
                        await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));

                    await context.Response.WriteAsync("<!DOCTYPE html>\n");
                    await context.Response.WriteAsync(" <html>\n");
                    await context.Response.WriteAsync(" <head>\n");
                    await context.Response.WriteAsync("   <title> ASPNET5 Profiler Demo </title>\n");
                    await context.Response.WriteAsync("    </head>\n");
                    await context.Response.WriteAsync("    <body>\n");
                    await context.Response.WriteAsync("      <h1>This is a page</h1>\n");
                    await context.Response.WriteAsync("      <h2>a simple page</h2>\n");
                    await context.Response.WriteAsync($"      <p>This page was generated at {DateTime.Now.ToString()}</p>\n");
                    await SomeWork(context);

                    var strings = new List<string>
                    {
                        "http://bash.im/rss/",
                        "http://ithappens.me/rss",
                        "http://zadolba.li/rss"
                    };

                    string[] results;

                    using (context.GetProfiler().Step("RUNNING 3 TASKS IN PARALLEL"))
                    {
                        var tasksToDo = strings.Select(uri => GetSomeRandomTextWithDelays(context, uri)).ToList();

                        results = await Task.WhenAll(tasksToDo);

                        _logger.LogInformation($"Got some data: {results.Length} entries total:");

                        foreach (var result in results)
                        {
                            _logger.LogInformation($"   > {result}");
                        }
                    }

                    await context.Response.WriteAsync("       <h3>Some results:</h3>");

                    await PrintList(context, results);

                    using (context.GetProfiler().Step("SOME REAL ASYNCS"))
                    {
                        await Task.WhenAll(
                            WaitForExactDelays(context, "TASK1", TimeSpan.FromMilliseconds(200), TimeSpan.FromMilliseconds(400)),
                            WaitForExactDelays(context, "TASK2", TimeSpan.FromMilliseconds(400), TimeSpan.FromMilliseconds(200)));
                    }

                    await context.Response.WriteAsync("       <p>&copy; 2015 Oleg K.</p>");
                    await context.Response.WriteAsync("</body>\n");
                    await context.Response.WriteAsync("</html>\n");

                    using (context.GetProfiler().Step("SOMETHING TO DO AFTER FULL PAGE RENDER"))
                        await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));
                }
            });
        }

        private async Task<string> GetSomeRandomTextWithDelays(HttpContext context, string input)
        {
            using (context.GetProfiler().Step($"CALCULATING STRING OF '{input}'"))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 1500 + 100));

                var result = $"String '{input}' has lengths of {input.Length} characters.";

                _logger.LogInformation($"Results for '{input}' are ready, but we need to wait more");

                await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 1500 + 100));

                return result;
            }
        }

        private async Task SomeWork(HttpContext context)
        {
            using (context.GetProfiler().Step("SOMETHING TO DO BEFORE"))
                await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));

            using (context.GetProfiler().Step("SOME WORK"))
            {
                await context.Response.WriteAsync("<ul>");

                for (int i = 1; i <= 5; i++)
                {
                    using (context.GetProfiler().Step("RENDER <LI>"))
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));
                        await context.Response.WriteAsync($"<li>Item #{i}</li>");
                    }
                }
            }

            using (context.GetProfiler().Step("SOMETHING TO DO AFTER"))
                await Task.Delay(TimeSpan.FromMilliseconds(R.NextDouble() * 150 + 50));

            await context.Response.WriteAsync("</ul>");
        }

        private async Task PrintList(HttpContext context, IEnumerable<string> strings)
        {
            await context.Response.WriteAsync("<ol>");

            foreach (var s in strings)
            {
                await context.Response.WriteAsync($"<li>{s}</li>");
            }

            await context.Response.WriteAsync("</ol>");
        }

        private async Task WaitForExactDelays(HttpContext context, string title,  TimeSpan delay1, TimeSpan delay2)
        {
            using (context.GetProfiler().Step($"TITLE: '{title}', CONTAINER"))
            {
                using (context.GetProfiler().Step($"TITLE: '{title}', First delay"))
                {
                    await Task.Delay(delay1);
                }

                using (context.GetProfiler().Step($"TITLE: '{title}', Second delay"))
                {
                    await Task.Delay(delay2);
                }
            }
        }
    }
}

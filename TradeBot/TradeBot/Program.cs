using System;
using Elmah.Io.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace TradeBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();

            //WebHost.CreateDefaultBuilder(args)
            //.UseStartup<Startup>()
            //.ConfigureLogging((ctx, logging) =>
            //{
            //    logging.AddElmahIo(options =>
            //    {
            //        options.ApiKey = "66b1e296f0d14e4d8a4a8a9b6e509663";
            //        options.LogId = new Guid("d1855ad8-d7b1-421b-b861-626ce394c2ef");
            //    });
            //    logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Trace);
            //})
            //.Build();

            
        }

        //public static IWebHost BuildWebHost(string[] args) =>
        //    WebHost.CreateDefaultBuilder(args)
        //        .UseStartup<Startup>()
        //        .Build();


        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((ctx, logging) =>
            {
                logging.AddElmahIo(options =>
                {
                    options.ApiKey = "66b1e296f0d14e4d8a4a8a9b6e509663";
                    options.LogId = new Guid("d1855ad8-d7b1-421b-b861-626ce394c2ef");
                });
                logging.AddFilter<ElmahIoLoggerProvider>(null, LogLevel.Trace);
            })
            .Build();

    }
}

namespace Website
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    using Hangfire;

    /// <summary>Executes the scheduled tasks in independent threads automatically.</summary>
    [EscapeGCop("Auto generated code.")]
    public partial class TaskManager : BackgroundJobsPlan
    {
        /// <summary>
        /// This will start the scheduled activities.<para/>
        /// It should be called once in Application_Start global event.<para/>
        /// </summary>
        public static void Run()
        {
            //RecurringJob.AddOrUpdate("Clean old temp uploads", () => CleanOldTempUploads(), Cron.MinuteInterval(10));
        }

        public override void Initialize()
        {

        }

        /// <summary>Clean old temp uploads</summary>

    }
}
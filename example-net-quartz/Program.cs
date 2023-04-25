using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace example_net_quartz
{
    internal class Program
    {
        //https://www.freeformatter.com/cron-expression-generator-quartz.html

        static async Task Main(string[] args)
        {
            Console.WriteLine("---------- Begin Initialization ----------");

            IScheduler scheduler = await new StdSchedulerFactory().GetScheduler();

            Console.WriteLine("---------- End Initialization ----------");

            Console.WriteLine("---------- Begin Scheduling Jobs ----------");

            foreach (var job in GetJobs())
                await Job(scheduler, job);

            Console.WriteLine("---------- End Scheduling Jobs ----------");

            Console.WriteLine("---------- Starting Scheduler ----------");

            await scheduler.Start();

            Console.WriteLine("------- Waiting Scheduler ------------");

            await Task.Delay(-1);

            Console.WriteLine("------- Ending Scheduler ---------------------");

            await scheduler.Shutdown(true);

            Console.WriteLine("------- Metadata -----------------");

            SchedulerMetaData metaData = await scheduler.GetMetaData();

            Console.WriteLine($"Executed {metaData.NumberOfJobsExecuted} jobs.");
        }

        static async Task Job(IScheduler scheduler, JobMetadata jobMetadata)
        {
            IJobDetail job = JobBuilder.Create<JoExample>()
                .WithIdentity("job-" + jobMetadata.Id)
                .Build();

            ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger-" + +jobMetadata.Id)
                .WithCronSchedule(jobMetadata.Schedule)
                .Build();

            job.JobDataMap.Put(JoExample.Parameter, jobMetadata.Parameter);

            await scheduler.ScheduleJob(job, trigger);

            //DateTimeOffset ft = await scheduler.ScheduleJob(job, trigger);

            //Console.WriteLine(job.Key + " has been scheduled to run at: " + ft + " and repeat based on expression: " + trigger.CronExpressionString);
        }

        static List<JobMetadata> GetJobs()
        {
            return new List<JobMetadata>()
            {
                new JobMetadata(1, "* * * ? * *", "Every second"),
                new JobMetadata(2, "0 * * ? * *", "Every minute"),
                new JobMetadata(3, "0/2 * * ? * * *", "Every 2 seconds starting at :00 second after the minute"),
                new JobMetadata(4, "0/5 * * ? * * *", "Every 5 seconds starting at :00 second after the minute")
            };
        }
    }

    public class JobMetadata
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public string Parameter { get; set; }

        public JobMetadata(int id, string schedule, string parameter)
        {
            Id = id;
            Schedule = schedule;
            Parameter = parameter;
        }
    }

    public class JoExample : IJob
    {
        public const string Parameter = "Parameter";

        public virtual Task Execute(IJobExecutionContext context)
        {
            JobKey jobKey = context.JobDetail.Key;

            JobDataMap jobData = context.JobDetail.JobDataMap;

            var parameter = jobData.GetString(Parameter);

            Console.WriteLine($"execute - job '{context.JobDetail.Key.Name}' - date '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}' - parameter '{parameter}'");

            return default;
        }
    }
}

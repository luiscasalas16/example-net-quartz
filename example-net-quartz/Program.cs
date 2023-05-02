using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace example_net_quartz
{
    internal class Program
    {
        //https://en.rakko.tools/tools/88/
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
            IJobDetail job = JobBuilder.Create<Job>()
                .WithIdentity("job-" + jobMetadata.Id)
                .DisallowConcurrentExecution(!jobMetadata.Concurrent)
                .Build();

            ICronTrigger trigger = (ICronTrigger)TriggerBuilder.Create()
                .WithIdentity("trigger-" + +jobMetadata.Id)
                .WithCronSchedule(jobMetadata.Schedule)
                .Build();

            job.JobDataMap.Put(example_net_quartz.Job.Parameter, jobMetadata.Parameter);

            await scheduler.ScheduleJob(job, trigger);
        }

        static List<JobMetadata> GetJobs()
        {
            return new List<JobMetadata>()
            {
                new JobMetadata(1, "* * * ? * *", true, "Every second"),
                new JobMetadata(2, "0 * * ? * *", true, "Every minute"),
                new JobMetadata(3, "0/2 * * ? * * *", true, "Every 2 seconds starting at :00 second after the minute"),
                new JobMetadata(4, "0/5 * * ? * * *", true, "Every 5 seconds starting at :00 second after the minute")
            };
        }
    }

    public class JobMetadata
    {
        public int Id { get; set; }
        public string Schedule { get; set; }
        public bool Concurrent { get; set; }
        public string Parameter { get; set; }

        public JobMetadata(int id, string schedule, bool concurrent, string parameter)
        {
            Id = id;
            Schedule = schedule;
            Concurrent = concurrent;
            Parameter = parameter;
        }
    }

    public class Job : IJob
    {
        public const string Parameter = "Parameter";

        public virtual Task Execute(IJobExecutionContext context)
        {
            JobKey jobKey = context.JobDetail.Key;

            JobDataMap jobData = context.JobDetail.JobDataMap;

            var parameter = jobData.GetString(Parameter);

            Console.WriteLine($"starting - job: '{context.JobDetail.Key.Name}' - date: '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}' - parameter: '{parameter}'");

            Thread.Sleep(5000);

            Console.WriteLine($"ending - job: '{context.JobDetail.Key.Name}' - date: '{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}' - parameter: '{parameter}'");

            return default;
        }
    }
}

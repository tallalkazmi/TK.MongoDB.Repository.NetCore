using fm.Extensions.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TK.MongoDB.Data;
using TK.MongoDB.Interfaces;
using TK.MongoDB.Tests.Classes;
using TK.MongoDB.Tests.Interfaces;
using TK.MongoDB.Tests.Models;
using MongoDB.Driver;
using System.Collections.Generic;

namespace TK.MongoDB.Tests
{
    [TestClass]
    public class InsertUpdateDeleteTest : ServiceTests
    {
        protected override void ConfigureConfiguration([NotNull] IConfigurationBuilder configuration)
        {
            configuration.AddJsonFile("appsettings.json").Build();
        }

        protected override void ConfigureServices([NotNull] IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.AddTransient(x => new Connection(Configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IDependencyTracker, DependencyTracker>();
            services.AddTransient(typeof(IRepository<>), typeof(Repository<>));
        }

        [TestMethod]
        public async Task InsertUpdateDelete()
        {
            Settings.Configure<Activity>(new Connection(Configuration.GetConnectionString("DefaultConnection")),
                200000,
                new CreateCollectionOptions()
                {
                    Capped = true,
                    MaxSize = 1024
                });

            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity result = await ActivityRepository.InsertAsync(activity);
            Console.WriteLine($"Inserted:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            if (ActivityRepository.IsCollectionCapped())
            {
                result.Name = "abc-00000000000000000000";
                await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                    async () => await ActivityRepository.UpdateAsync(result),
                    "Cannot change the size of a document in a capped collection.");

                await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                    async () => await ActivityRepository.DeleteAsync(result.Id, false),
                    "Cannot change the size of a document in a capped collection.");
            }
            else
            {
                result.Name = "abc-0000";
                bool isUpdated = await ActivityRepository.UpdateAsync(result);
                Console.WriteLine($"Updated: {isUpdated}");
                Assert.IsTrue(isUpdated);

                bool isDeleted = await ActivityRepository.DeleteAsync(result.Id, false);
                Assert.IsTrue(isDeleted);
            }

            await ActivityRepository.DropCollectionAsync();
        }

        [TestMethod]
        public async Task BulkInsert()
        {
            Settings.Configure<Activity>(new Connection(Configuration.GetConnectionString("DefaultConnection")),
                200000,
                new CreateCollectionOptions()
                {
                    Capped = true,
                    MaxSize = 1024
                });

            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            List<Activity> Activities = new List<Activity>();
            for (int i = 0; i < 10; i++)
            {
                Activities.Add(new Activity() { Name = $"abc-{i}-{DateTime.UtcNow.GetTimestamp()}" });
            }

            var result = await ActivityRepository.BulkInsertAsync(Activities);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.IsAcknowledged);
            Assert.AreEqual(result.InsertedCount, Activities.Count);

            await ActivityRepository.DropCollectionAsync();
        }
    }
}

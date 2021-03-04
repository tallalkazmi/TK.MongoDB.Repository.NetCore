using fm.Extensions.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using TK.MongoDB.Data;
using TK.MongoDB.Interfaces;
using TK.MongoDB.Tests.Classes;
using TK.MongoDB.Tests.Interfaces;
using TK.MongoDB.Tests.Models;

namespace TK.MongoDB.Tests
{
    [TestClass]
    public class GetTest : ServiceTests
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
        public async Task GetById()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            Activity result = await ActivityRepository.FindAsync(insert_result.Id);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public async Task Get()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            var result = await ActivityRepository.GetAsync(1, 10, x => x.Name.Contains("abc") && x.Deleted == false);
            Console.WriteLine($"Output:\nTotal: {result.Item2}\n{JToken.Parse(JsonConvert.SerializeObject(result.Item1)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public async Task GetNonPaged()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            var result = ActivityRepository.Get(x => x.Name.Contains("abc") && x.Deleted == false);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public async Task In()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            List<string> names = new List<string> { "abc", "def", "ghi" };
            var result = ActivityRepository.GetIn(x => x.Name, names);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(result)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }
    }
}

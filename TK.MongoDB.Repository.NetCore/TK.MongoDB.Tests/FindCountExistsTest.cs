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

namespace TK.MongoDB.Tests
{
    [TestClass]
    public class FindCountExistsTest : ServiceTests
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
        public async Task Find()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            Activity result = await ActivityRepository.FindAsync(x => x.Id == insert_result.Id);
            Console.WriteLine($"Output:\n{JToken.Parse(JsonConvert.SerializeObject(insert_result)).ToString(Formatting.Indented)}");
            Assert.IsNotNull(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(result.Id, false);
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public async Task Count()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            long result = await ActivityRepository.CountAsync();
            Console.WriteLine($"Count: {result}");
            Assert.AreEqual(result, 1);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public async Task Exists()
        {
            IRepository<Activity> ActivityRepository = this.GetRequiredService<IRepository<Activity>>();

            Activity activity = new Activity() { Name = $"abc-{DateTime.UtcNow.GetTimestamp()}" };
            Activity insert_result = await ActivityRepository.InsertAsync(activity);
            Assert.IsNotNull(insert_result);

            bool result = await ActivityRepository.ExistsAsync(x => x.Name.Contains("abc"));
            Console.WriteLine($"Exists: {result}");
            Assert.IsTrue(result);

            bool isDeleted = await ActivityRepository.DeleteAsync(insert_result.Id, false);
            Assert.IsTrue(isDeleted);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Nemo.Attributes;
using Nemo.Configuration;
using Nemo.Configuration.Mapping;
using Nemo.Extensions;
using Yarn;
using Yarn.Data.NemoProvider;
using Yarn.Extensions;
using Yarn.Queries;
using Yarn.Specification;

namespace NemoTest
{
    public class Logger : ILogProvider
    {
        public void Configure()
        {
            
        }

        public void Configure(string configFile)
        {
            
        }

        public void Write(string message)
        {
            Console.WriteLine(message);
        }

        public void Write(Exception exception, string id)
        {
            Console.WriteLine(exception.Message);
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            var nemoConfig = ConfigurationFactory.CloneCurrentConfiguration();
            nemoConfig.SetDefaultCacheRepresentation(Nemo.CacheRepresentation.None).SetLogProvider(new Logger()).SetLogging(true).SetAutoTypeCoercion(true);

            ObjectContainer.Current.Register<ITestRepo>(() => new TestRepo(new Yarn.Data.NemoProvider.RepositoryOptions { UseStoredProcedures = false, Configuration = nemoConfig }, new Yarn.Data.NemoProvider.DataContextOptions { ConnectionName = "ExcitContext" }), "Nemo");

            var repo = ObjectContainer.Current.Resolve<ITestRepo>("Nemo");

            var loadedTestModel = repo.LoaderGetDataStoreAsync(166).ToList();

            var getDataStoreByIdQuery = new GetDataStoreById(166);
            var queryResult = getDataStoreByIdQuery.Execute(repo);

            var getDataStoreByIdQueryWithPrices = new GetDataStoreByIdWithPrices(166);
            var queryResultWithPrices = getDataStoreByIdQueryWithPrices.Execute(repo).Items;

            //var testModel1 = (await repo.GetDataStoreAsync()).ToList();
            //var testModel2 = (await repo.GetInstanceStoreAsync()).ToList();
        }
    }

    public class GetDataStoreById : IQuery<TestModel1>
    {
        public GetDataStoreById(long id)
        {
            DataStoreId = id;
        }

        public long DataStoreId { get; set; }

        public IQueryResult<TestModel1> Execute(IRepository repository)
        {
            var query = new QueryBuilder<TestModel1>().Where(new Specification<TestModel1>(x => x.Id == DataStoreId))
                .Build(repository);
            return new QueryResult<TestModel1>(query.FirstOrDefault());
        }
    }

    public class GetDataStoreByIdWithPrices : IQuery<IEnumerable<TestModel1>>
    {
        public GetDataStoreByIdWithPrices(long id)
        {
            DataStoreId = id;
        }

        public long DataStoreId { get; set; }

        public IQueryResult<IEnumerable<TestModel1>> Execute(IRepository repository)
        {
            var loader = repository.As<ILoadServiceProvider>();
            var result = loader.Load<TestModel1>().Include(o => o.Prices).FindAll(x => x.Id == DataStoreId).ToList();
            
            return new QueryResult<IEnumerable<TestModel1>>(result);
        }
    }

    public class GetDataStoreByIdWithPricesQb : IQuery<IEnumerable<TestModel1>>
    {
        public GetDataStoreByIdWithPricesQb(long id)
        {
            DataStoreId = id;
        }

        public long DataStoreId { get; set; }

        public IQueryResult<IEnumerable<TestModel1>> Execute(IRepository repository)
        {
            var query = new QueryBuilder<TestModel1>().Where(new Specification<TestModel1>(x => x.Id == DataStoreId))
                .Build(repository);

            return new QueryResult<IEnumerable<TestModel1>>(query);
        }
    }

    public interface ITestRepo : IRepositoryAsync
    {
        Task<IEnumerable<TestModel1>> GetDataStoreAsync();
        Task<IEnumerable<TestModel2>> GetInstanceStoreAsync();

        IEnumerable<TestModel1> LoaderGetDataStoreAsync(long id);

    }

    public class TestRepo : Yarn.Data.NemoProvider.RepositoryAsync, ITestRepo
    {
        public TestRepo(IDataContextAsync<DbConnection> context) : base(context)
        {
        }

        public TestRepo(RepositoryOptions options, DataContextOptions dataContextOptions) : base(options, dataContextOptions)
        {
        }

        public TestRepo(RepositoryOptions options, DataContextOptions dataContextOptions, DbTransaction transaction) : base(options, dataContextOptions, transaction)
        {
        }
        

        public async Task<IEnumerable<TestModel1>> GetDataStoreAsync()
        {
            return await FindAllAsync<TestModel1>(x => x.Id != 0);
        }

        public async Task<IEnumerable<TestModel2>> GetInstanceStoreAsync()
        {
            return await FindAllAsync<TestModel2>(x=>x.Id != 0);
        }

        public IEnumerable<TestModel1> LoaderGetDataStoreAsync(long id)
        {
            var loader = this.As<ILoadServiceProvider>();
            var result = loader.Load<TestModel1>().Include(o=>o.Prices).FindAll(x => x.Id == id);
            return result;
        }
    }

    [Nemo.Attributes.Table("WebCrawlerDataStore")]
    public class TestModel
    {
        [PrimaryKey]
        public long Id { get; set; }
        [MapColumn("ProductName")]
        public string Name { get; set; }
    }

    [Nemo.Attributes.Table("WebCrawlerDataStore")]
    public class TestModel1
    {
        //[PrimaryKey]
        public long Id { get; set; }
        //[MapColumn("ProductName")]
        public string Name { get; set; }
        public IList<Pricing> Prices { get; set; } = new List<Pricing>();
    }
    
    [Nemo.Attributes.Table("WebCrawlerDataStorePricingDetail")]
    public class Pricing
    {
        //[PrimaryKey]
        public long Id { get; set; }
        //[References(typeof(TestModel1))]
        public long ParentId { get; set; }
        public TestModel1 TestModel1 { get; set; }
        public string CreateSource { get; set; }
    }

    [Nemo.Attributes.Table("WebCrawlerInstanceProductDetail")]
    public class TestModel2 : TestModel
    {

    }

    public class TestModel1Map : EntityMap<TestModel1>
    {
        public TestModel1Map()
        {
            TableName = "WebCrawlerDataStore";
            Property(p => p.Id).PrimaryKey();
            Property(p => p.Name).Column("ProductName");
        }
    }

    public class PricingMap : EntityMap<Pricing>
    {
        public PricingMap()
        {
            TableName = "WebCrawlerDataStorePricingDetail";
            Property(p => p.Id).PrimaryKey();
            Property(p => p.ParentId).Column("DetailId");
            Property(p => p.ParentId).References<TestModel1>();
        }
    }
}

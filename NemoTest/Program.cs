using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using Nemo.Configuration;
using Yarn;
using Yarn.Data.NemoProvider;

namespace NemoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var nemoConfig = ConfigurationFactory.CloneCurrentConfiguration();
            nemoConfig.SetDefaultCacheRepresentation(Nemo.CacheRepresentation.None);

            ObjectContainer.Current.Register<IRepository>(() => new TestRepo(new Yarn.Data.NemoProvider.RepositoryOptions { UseStoredProcedures = false, Configuration = nemoConfig }, new Yarn.Data.NemoProvider.DataContextOptions { ConnectionName = "ExcitContext" }), "Nemo");

            var repo = ObjectContainer.Current.Resolve<IRepository>("Nemo");
        }
    }
    
    public class TestRepo : Yarn.Data.NemoProvider.Repository
    {
        public TestRepo(IDataContext<DbConnection> context) : base(context)
        {
        }

        public TestRepo(RepositoryOptions options, DataContextOptions dataContextOptions) : base(options, dataContextOptions)
        {
        }

        public TestRepo(RepositoryOptions options, DataContextOptions dataContextOptions, DbTransaction transaction) : base(options, dataContextOptions, transaction)
        {
        }
    }
}

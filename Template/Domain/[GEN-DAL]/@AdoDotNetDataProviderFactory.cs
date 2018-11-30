namespace AppData
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Olive;
    using Olive.Entities;
    using Olive.Entities.Data;
    /// <summary>A factory that can instantiate Data Provider objects for MY.MICROSERVICE.NAME</summary>
    [EscapeGCop("Auto generated code.")]
    public class AdoDotNetDataProviderFactory : IDataProviderFactory
    {
        ICache Cache;
        string ConnectionStringKey;
        public string ConnectionString {get; private set;}
        
        /// <summary>Initializes a new instance of AdoDotNetDataProviderFactory.</summary>
        public AdoDotNetDataProviderFactory(DatabaseConfig.Provider provider)
        {
            ConnectionString = provider.ConnectionString.Or(DataAccess.GetCurrentConnectionString());
            ConnectionStringKey = provider.ConnectionStringKey;
            Cache = Context.Current.GetService<ICache>();
        }
        
        public IDataAccess GetAccess() => new DataAccess<SqlConnection>();
        
        /// <summary>Gets a data provider instance for the specified entity type.</summary>
        public virtual IDataProvider GetProvider(Type type)
        {
            IDataProvider result = null;
            
            if (type.IsInterface) result = new InterfaceDataProvider(type);
            
            if (result == null)
            {
                throw new NotSupportedException(type + " is not a data-supported type.");
            }
            else if (this.ConnectionString.HasValue())
            {
                result.ConnectionString = this.ConnectionString;
            }
            else if (this.ConnectionStringKey.HasValue())
            {
                result.ConnectionStringKey = this.ConnectionStringKey;
            }
            
            return result;
        }
        
        /// <summary>Determines whether this data provider factory handles interface data queries.</summary>
        public virtual bool SupportsPolymorphism() => true;
    }
}
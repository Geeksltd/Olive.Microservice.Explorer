using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using System.Threading.Tasks;

namespace Domain
{
    public class ReferenceData : IReferenceData
    {
        IDatabase Database;
        public ReferenceData(IDatabase database) => Database = database;
        async Task<T> Create<T>(T item) where T : IEntity
        {
            await Context.Current.Database().Save(item, SaveBehaviour.BypassAll);
            return item;
        }

        public async Task Create()
        {
            // ...
        }
    }
}
using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using System;
using System.Threading.Tasks;

namespace Domain
{
    public class ReferenceData
    {
        static async Task<T> Create<T>(T item) where T : IEntity
        {
            await Context.Current.Database().Save(item, SaveBehaviour.BypassAll);
            return item;
        }

        public static async Task Create()
        {
            // ...
        }
    }
}
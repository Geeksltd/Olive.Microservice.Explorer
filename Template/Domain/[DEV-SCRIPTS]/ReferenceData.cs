using System;
using System.Threading.Tasks;
using Olive;
using Olive.Entities;
using Olive.Entities.Data;

namespace Domain
{
    public class ReferenceData : IReferenceData
    {
        public ReferenceData()
        {
        }

        static Task<T> Create<T>(T item) where T : IEntity
            => Context.Current.Database().Save(item, SaveBehaviour.BypassAll).ContinueWith(x => item);

        public async Task Create()
        {
            // ...
        }
    }
}
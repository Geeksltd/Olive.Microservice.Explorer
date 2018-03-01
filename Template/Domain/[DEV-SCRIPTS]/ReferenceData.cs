using System;
using System.Threading.Tasks;
using Olive;
using Olive.Entities;
using Olive.Entities.Data;
using Olive.Security;

namespace Domain
{
    public class ReferenceData
    {
        static Task<T> Create<T>(T item) where T : IEntity
            => Database.Instance.Save(item, SaveBehaviour.BypassAll).ContinueWith(x => item);

        public static async Task Create()
        {
            // ...
        }
    }
}
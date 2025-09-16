using DataEntities;
using Microsoft.Extensions.VectorData;

namespace VectorEntities
{
    public class ProductVector : Product
    {
        [VectorStoreKey]
        public override int Id { get => base.Id; set => base.Id = value; }

        [VectorStoreData]
        public override string? Name { get => base.Name; set => base.Name = value; }

        [VectorStoreData]
        public override string? Description { get => base.Description; set => base.Description = value; }

        [VectorStoreData]
        public override decimal Price { get => base.Price; set => base.Price = value; }

        [VectorStoreVector(384)]
        public ReadOnlyMemory<float> Vector { get; set; }
    }
}

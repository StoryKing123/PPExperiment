using System;

namespace PPConcurrentTest.Models
{
    public class Test2Entity : BaseEntity
    {
        public string? NewColumn { get; set; }
        public string? Remark { get; set; }
        public Guid? TestLookupId { get; set; }
        public virtual TestEntity? TestLookup { get; set; }
    }
}
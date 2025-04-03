using System;
using System.Collections.Generic;

namespace PPConcurrentTest.Models
{
    public class TestEntity : BaseEntity
    {
        public string? Name { get; set; }
        
        // Navigation property for related Test2 entities
        public virtual ICollection<Test2Entity>? RelatedTest2Records { get; set; }
    }
}

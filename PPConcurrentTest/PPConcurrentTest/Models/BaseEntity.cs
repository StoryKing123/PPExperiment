using System;

namespace PPConcurrentTest.Models
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        // 抽象属性，强制子类必须实现
        public static  string EntityLogicalName { get; }
    }
}

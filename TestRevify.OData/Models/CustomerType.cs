using System;

namespace TestRevify.OData.Models
{
    [Flags]
    public enum CustomerType
    {
        None = 1,
        Premium = 2,
        VIP = 4
    }
}

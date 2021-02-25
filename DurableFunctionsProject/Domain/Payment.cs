using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctionsProject.Domain
{
    public class Payment
    {
        public Guid Id { get; set; }
        public double Amount { get; set; }
        public Customer Customer { get; set; }
    }
}

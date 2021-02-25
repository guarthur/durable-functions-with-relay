using System;
using System.Collections.Generic;
using System.Text;

namespace DurableFunctionsProject.Domain
{
    public class CustomerNotification : Notification
    {
        public Customer Customer { get; set; }        

        public CustomerNotification(Customer customer, string message) : base(message)
        {
            Customer = customer;
        }
    }
}

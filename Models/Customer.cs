using System;

namespace McbeevCommerceBot.Models
{
    [Serializable]
    public class Customer
    {
        public int CustomerID { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
    }
}
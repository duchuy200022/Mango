﻿using System.ComponentModel.DataAnnotations.Schema;
namespace Mango.Web.Models
{
    public class OrderDetailsDto
    {
        public int orderDetailsId { get; set; }
        public int OrderHeaderId { get; set; }
        public int ProductId { get; set; }
        public ProductDto? Product { get; set; }
        public int Count { get; set; }
        public string ProductName { get; set; }
        public double Price { get; set; }
    }
}

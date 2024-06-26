﻿namespace Domain.Events.Products
{
    public class ProductCreatedEvent : BaseEvent
    {
        public ProductCreatedEvent(Product product)
        {
            Product = product;
        }

        public Product Product { get; }
    }
}

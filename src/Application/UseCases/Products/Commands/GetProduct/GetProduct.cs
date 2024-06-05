﻿using Application.Common.Interfaces;
using Application.Common.Models;
using Ardalis.GuardClauses;
using AutoMapper;
using AutoMapper.QueryableExtensions;

namespace Application.UseCases.Products.Commands.GetProduct
{
    /// <summary>
    /// Request for getting an existing product.
    /// </summary>
    public class GetProductCommand : IRequest<ProductBriefDto>
    {
        public required int Id { get; init; }
    }

    /// <summary>
    /// Request handler for getting an existing product.
    /// </summary>
    public class GetProductCommandHandler : IRequestHandler<GetProductCommand, ProductBriefDto>
    {
        private readonly IApplicationDbContext dbContext;
        private readonly IMapper mapper;

        public GetProductCommandHandler(IApplicationDbContext _context, IMapper _mapper)
        {
            dbContext = _context;
            mapper = _mapper;
        }

        public async Task<ProductBriefDto> Handle(GetProductCommand request, CancellationToken cancellationToken)
        {
            var product = dbContext.Products.Where(p => p.Id == request.Id).FirstOrDefault();

            // Checks if the Product exists. If not, throws an exception
            Guard.Against.NotFound(request.Id, product);

            return mapper.Map<Product, ProductBriefDto>(product);
        }
    }
}


using ECO.WebApi.Application.Identity.Users;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Products;
public class GetProductRequest : IRequest<ProductDto>
{
    public Guid Id { get; set; }

    public GetProductRequest(Guid id)
    {
        Id = id;
    }
}


public class GetProductRequestValidator : AbstractValidator<GetProductRequest>
{
    public GetProductRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

//Handler
public class GetProductRequestHandler : IRequestHandler<GetProductRequest, ProductDto>
{
    private readonly IRepository<Product> _repository;
    private readonly IUserService _userService;

    public GetProductRequestHandler(IRepository<Product> repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    public async Task<ProductDto> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        var product = await _repository.FirstOrDefaultAsync(new ProductByIdSpec(request.Id),cancellationToken)
              ?? throw new NotFoundException($"Product with ID {request.Id} was not found.");

        var productDto = product.Adapt<ProductDto>();
        productDto.Creator = await _userService.GetFullName(product.CreatedBy);
        return productDto;
    }
}

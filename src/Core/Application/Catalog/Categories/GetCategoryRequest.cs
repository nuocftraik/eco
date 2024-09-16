using ECO.WebApi.Application.Identity.Users;
using ECO.WebApi.Domain.Catalog;
using Mapster;

namespace ECO.WebApi.Application.Catalog.Categories;
public class GetCategoryRequest : IRequest<CategoryDto>
{
    public Guid Id { get; set; }

    public GetCategoryRequest(Guid id)
    {
        Id = id;
    }
}

public class GetCategoryRequestValidator : AbstractValidator<GetCategoryRequest>
{
    public GetCategoryRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}

//Handler
public class GetCategoryRequestHandler : IRequestHandler<GetCategoryRequest, CategoryDto>
{
    private readonly IRepository<Category> _repository;
    private readonly IUserService _userService;

    public GetCategoryRequestHandler(IRepository<Category> repository, IUserService userService)
    {
        _repository = repository;
        _userService = userService;
    }

    public async Task<CategoryDto> Handle(GetCategoryRequest request, CancellationToken cancellationToken)
    {
        var category = await _repository.FirstOrDefaultAsync(new CategoryByIdSpec(request.Id),cancellationToken)
              ?? throw new NotFoundException($"Category with ID {request.Id} was not found.");

        var categoryDto = category.Adapt<CategoryDto>();
        categoryDto.Creator = await _userService.GetFullName(category.CreatedBy);
        return categoryDto;
    }
}

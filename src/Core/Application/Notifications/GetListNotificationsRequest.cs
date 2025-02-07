
using ECO.WebApi.Domain.Notifications;
using Mapster;

namespace ECO.WebApi.Application.Notifications;
public class GetListNotificationsRequest : PaginationFilter, IRequest<PaginationResponse<NotificationDto>>
{
    public string? ReceiverId { get; set; }
    public bool? IsRead { get; set; }
}

public class GetListNotificationsRequestSpec : EntitiesByPaginationFilterSpec<Notification>
{
    public GetListNotificationsRequestSpec(GetListNotificationsRequest request) : base(request)
    {
        Query.Where(x => x.ReceiverId.Equals(request.ReceiverId))
             .Where(x => x.IsRead == request.IsRead!.Value, request.IsRead.HasValue)
             .OrderByDescending(n => n.CreatedOn);
    }
}

public class GetListNotificationsRequestHandler : IRequestHandler<GetListNotificationsRequest, PaginationResponse<NotificationDto>>
{
    private readonly IReadRepository<Notification> _repository;
    private readonly ICurrentUser _currentUser;

    public GetListNotificationsRequestHandler(IReadRepository<Notification> repository, ICurrentUser currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<PaginationResponse<NotificationDto>> Handle(GetListNotificationsRequest request, CancellationToken cancellationToken)
    {
        request.ReceiverId = _currentUser.GetUserId().ToString();
        var notifications =  await _repository.ListAsync(new GetListNotificationsRequestSpec(request));
        var dto = notifications.Adapt<List<NotificationDto>>();
        int count = await _repository.CountAsync(new GetListNotificationsRequestSpec(request));
        return new PaginationResponse<NotificationDto>(dto,count, request.PageNumber, request.PageSize);
    }
}


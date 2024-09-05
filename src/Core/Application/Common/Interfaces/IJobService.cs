using System.Linq.Expressions;

namespace ECO.WebApi.Application.Common.Interfaces;
public interface IJobService : ITransientService
{
    // Phương thức Enqueue cho phép đẩy một job vào hàng đợi để thực thi ngay lập tức.
    // Ở đây sử dụng biểu thức `Expression<Action>` để chỉ định phương thức sẽ được gọi khi job thực thi.
    string Enqueue(Expression<Action> methodCall);

    // Tương tự Enqueue nhưng dành cho các phương thức bất đồng bộ (`Task`).
    string Enqueue(Expression<Func<Task>> methodCall);

    // Enqueue dành cho các job có tham số kiểu T.
    string Enqueue<T>(Expression<Action<T>> methodCall);

    // Enqueue cho các job bất đồng bộ có tham số kiểu T.
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);

    // Phương thức Schedule lên lịch một job để thực thi sau khoảng thời gian `delay`.
    string Schedule(Expression<Action> methodCall, TimeSpan delay);

    // Schedule một job bất đồng bộ với thời gian trì hoãn `delay`.
    string Schedule(Expression<Func<Task>> methodCall, TimeSpan delay);

    // Lên lịch một job để thực thi vào thời điểm chính xác `enqueueAt`.
    string Schedule(Expression<Action> methodCall, DateTimeOffset enqueueAt);

    // Lên lịch một job bất đồng bộ để thực thi vào thời điểm `enqueueAt`.
    string Schedule(Expression<Func<Task>> methodCall, DateTimeOffset enqueueAt);

    // Schedule một job với tham số kiểu T và trì hoãn thực thi.
    string Schedule<T>(Expression<Action<T>> methodCall, TimeSpan delay);

    // Schedule một job bất đồng bộ với tham số kiểu T và thời gian trì hoãn.
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    // Lên lịch một job có tham số kiểu T để thực thi vào thời điểm chính xác `enqueueAt`.
    string Schedule<T>(Expression<Action<T>> methodCall, DateTimeOffset enqueueAt);

    // Lên lịch một job bất đồng bộ có tham số kiểu T để thực thi vào thời điểm chính xác `enqueueAt`.
    string Schedule<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);

    // Xóa một job khỏi hệ thống dựa trên jobId.
    bool Delete(string jobId);

    // Xóa job khỏi hệ thống, xác định từ trạng thái nào (`fromState`).
    bool Delete(string jobId, string fromState);

    // Đưa job trở lại hàng đợi để thực thi lại.
    bool Requeue(string jobId);

    // Đưa job trở lại hàng đợi từ trạng thái cụ thể (`fromState`).
    bool Requeue(string jobId, string fromState);
}

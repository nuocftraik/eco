using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System;

namespace ECO.WebApi.Infrastructure.Persistence.Initialization;
internal class CustomSeederRunner
{
    private readonly ICustomSeeder[] _seeders;

    public CustomSeederRunner(IServiceProvider serviceProvider) =>
        _seeders = serviceProvider.GetServices<ICustomSeeder>().ToArray();

    public async Task RunSeedersAsync(CancellationToken cancellationToken)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.InitializeAsync(cancellationToken);
        }
    }
}

//Mục đích: CustomSeederRunner là một lớp chịu trách nhiệm chạy tất cả các "seeder" đã được đăng ký trong hệ thống thông qua Dependency Injection(DI).

//Thuộc tính _seeders: Đây là một mảng chứa các đối tượng thực thi giao diện ICustomSeeder.Mảng này được khởi tạo bằng cách sử dụng IServiceProvider để lấy tất cả các dịch vụ được đăng ký cho ICustomSeeder trong Dependency Injection container.

//serviceProvider.GetServices<ICustomSeeder>().ToArray() : Phương thức GetServices<T> lấy tất cả các dịch vụ đã được đăng ký với kiểu ICustomSeeder từ DI container và chuyển đổi chúng thành một mảng.
//Constructor CustomSeederRunner:

//Constructor này nhận vào một IServiceProvider, là một công cụ cung cấp các dịch vụ (service) đã được đăng ký.Constructor này sử dụng serviceProvider để lấy tất cả các đối tượng ICustomSeeder đã được đăng ký và lưu chúng vào mảng _seeders.
//Phương thức RunSeedersAsync:

//Đây là phương thức không đồng bộ có nhiệm vụ chạy tất cả các seeder.Nó lặp qua từng seeder trong mảng _seeders và gọi phương thức InitializeAsync của từng seeder.
//Phương thức await seeder.InitializeAsync(cancellationToken); đảm bảo rằng quá trình khởi tạo cho mỗi seeder được thực hiện tuần tự và không chồng chéo(các seeder được chạy một cách không đồng bộ nhưng theo trình tự).
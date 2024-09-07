
using ECO.WebApi.Application.Common.Interfaces;

namespace ECO.WebApi.Application.Common.Mailing;
public interface IEmailTemplateService : ITransientService
{
    string GenerateEmailTemplate<T>(string templateName, T mailTemplateModel);
}

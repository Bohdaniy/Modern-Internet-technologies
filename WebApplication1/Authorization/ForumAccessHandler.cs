using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace WebApplication1.Authorization
{
    public class ForumAccessHandler : AuthorizationHandler<ForumAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ForumAccessRequirement requirement)
        {
            // Перевіряємо, чи є у користувача ХОЧА Б ОДНЕ з потрібних тверджень
            // Вимога з лабораторної: IsMentor АБО IsVerifiedUser АБО HasForumAccess
            bool hasAccess = context.User.HasClaim(c => c.Type == "IsMentor") ||
                             context.User.HasClaim(c => c.Type == "IsVerifiedUser") ||
                             context.User.HasClaim(c => c.Type == "HasForumAccess");

            if (hasAccess)
            {
                // Якщо хоча б одна умова виконана - даємо доступ
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
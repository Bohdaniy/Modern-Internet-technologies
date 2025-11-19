using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Authorization
{
    public class MinHoursHandler : AuthorizationHandler<MinHoursRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, MinHoursRequirement requirement)
        {
            // 1. Шукаємо у користувача твердження (Claim) з назвою "WorkingHours"
            var hoursClaim = context.User.FindFirst("WorkingHours");

            // 2. Якщо таке твердження є і воно є числом
            if (hoursClaim != null && int.TryParse(hoursClaim.Value, out int hours))
            {
                // 3. Перевіряємо, чи годин достатньо (>= 100)
                if (hours >= requirement.MinHours)
                {
                    // Успіх! Дозволяємо доступ
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Authorization
{

    public class MinHoursRequirement : IAuthorizationRequirement
    {
        public int MinHours { get; }

        public MinHoursRequirement(int minHours)
        {
            MinHours = minHours;
        }
    }
}
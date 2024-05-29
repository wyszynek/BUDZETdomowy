using HomeBudget.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow.PointsToAnalysis;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

//jednak nie korzystamy z tego ale zostawiam bo moze sie przydac
namespace HomeBudget.Models
{
    public class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            // Utwórz kategorię "Work" dla każdego użytkownika
            foreach (var user in context.Users)
            {
                var workCategory = new Category
                {
                    CategoryName = "Work",
                    Icon = "&#128184;",
                    Type = "Income",
                    UserId = user.Id
                };
                context.Categories.Add(workCategory);
            }

            await context.SaveChangesAsync();
        }
    }
}

//jak chcemy uzyc tego to do program.c:
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;
//    var context = services.GetRequiredService<ApplicationDbContext>();

//    foreach (var user in context.Users)
//    {
//        if (!context.Categories.Any(c => c.UserId == user.Id && c.CategoryName == "Work"))
//        {
//            var workCategory = new Category
//            {
//                CategoryName = "Work",
//                Icon = "&#128184;",
//                Type = "Income",
//                UserId = user.Id
//            };
//            context.Categories.Add(workCategory);
//        }
//    }
//    await context.SaveChangesAsync();
//}

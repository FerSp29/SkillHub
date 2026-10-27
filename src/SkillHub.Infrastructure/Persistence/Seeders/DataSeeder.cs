using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities;
using SkillHub.Infrastructure.Identity;

namespace SkillHub.Infrastructure.Persistence.Seeders;

public static class DataSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        string excelPath)
    {
        Console.WriteLine($"[DataSeeder] Checking path: {excelPath}");

        if (!File.Exists(excelPath))
        {
            Console.WriteLine($"[DataSeeder] ERROR: Excel file NOT found at: {excelPath}");
            return;
        }

        // 1. Roles
        string[] roles = { "Employee", "Participant" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"[DataSeeder] Role '{role}' created.");
            }
        }

        using var workbook = new XLWorkbook(excelPath);

        // 2. Instructors (Employees)
        if (workbook.TryGetWorksheet("Instructors", out var instructorsSheet))
        {
            var rows = instructorsSheet.RowsUsed().Skip(1);
            int count = 0;
            foreach (var row in rows)
            {
                var fullName = row.Cell(1).GetString().Trim();
                var email = row.Cell(2).GetString().Trim();
                var password = row.Cell(3).GetString().Trim();
                var specialty = row.Cell(4).GetString().Trim();

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var instructor = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        Specialty = specialty,
                        EmailConfirmed = true
                    };

                    var res = await userManager.CreateAsync(instructor, password);
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(instructor, "Employee");
                        count++;
                    }
                    else
                    {
                        Console.WriteLine($"[DataSeeder] Failed creating instructor {email}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                    }
                }
            }
            Console.WriteLine($"[DataSeeder] Instructors seeded: {count}");
        }

        // 3. Participants
        if (workbook.TryGetWorksheet("Participants", out var participantsSheet))
        {
            var rows = participantsSheet.RowsUsed().Skip(1);
            int count = 0;
            foreach (var row in rows)
            {
                var fullName = row.Cell(1).GetString().Trim();
                var email = row.Cell(2).GetString().Trim();
                var password = row.Cell(3).GetString().Trim();

                if (await userManager.FindByEmailAsync(email) == null)
                {
                    var participant = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = fullName,
                        EmailConfirmed = true
                    };

                    var res = await userManager.CreateAsync(participant, password);
                    if (res.Succeeded)
                    {
                        await userManager.AddToRoleAsync(participant, "Participant");
                        count++;
                    }
                    else
                    {
                        Console.WriteLine($"[DataSeeder] Failed creating participant {email}: {string.Join(", ", res.Errors.Select(e => e.Description))}");
                    }
                }
            }
            Console.WriteLine($"[DataSeeder] Participants seeded: {count}");
        }

        // 4. Workshops
        if (workbook.TryGetWorksheet("Workshops", out var workshopsSheet))
        {
            var rows = workshopsSheet.RowsUsed().Skip(1);
            int count = 0;
            foreach (var row in rows)
            {
                var code = row.Cell(1).GetString().Trim();
                var title = row.Cell(2).GetString().Trim();
                var description = row.Cell(3).GetString().Trim();
                var instructorEmail = row.Cell(4).GetString().Trim();
                var startDateTime = row.Cell(5).GetDateTime();
                var endDateTime = row.Cell(6).GetDateTime();
                
                int capacity = 0;
                if (row.Cell(7).TryGetValue(out int cap))
                {
                    capacity = cap;
                }
                else
                {
                    int.TryParse(row.Cell(7).GetString().Trim(), out capacity);
                }

                var existing = await context.Workshops.FirstOrDefaultAsync(w => w.Code == code);
                if (existing == null)
                {
                    var instructor = await userManager.FindByEmailAsync(instructorEmail);
                    if (instructor != null)
                    {
                        var workshop = new Workshop
                        {
                            Code = code,
                            Title = title,
                            Description = description,
                            InstructorId = instructor.Id,
                            StartDateTime = DateTime.SpecifyKind(startDateTime, DateTimeKind.Utc),
                            EndDateTime = DateTime.SpecifyKind(endDateTime, DateTimeKind.Utc),
                            Capacity = capacity
                        };

                        await context.Workshops.AddAsync(workshop);
                        count++;
                    }
                }
            }
            await context.SaveChangesAsync();
            Console.WriteLine($"[DataSeeder] Workshops seeded: {count}");
        }
    }
}
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;

if (Console.IsInputRedirected)
{
    Console.Error.WriteLine("Ejecute esta herramienta desde una terminal interactiva para introducir las credenciales de forma segura.");
    return 1;
}

var builder = Host.CreateApplicationBuilder(args);
if (string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("Portfolio")))
{
    Console.Error.WriteLine("No está configurada la clave ConnectionStrings:Portfolio. Compruebe DOTNET_ENVIRONMENT y la configuración local sin mostrar su valor.");
    return 1;
}

builder.Services.AddPortfolioDatabase(builder.Configuration);
builder.Services.AddPortfolioProvisioningIdentity();

var currentStage = "construcción del host";
try
{
    using var host = builder.Build();
    await using var scope = host.Services.CreateAsyncScope();
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<PortfolioDbContext>();
    currentStage = "comprobación de migraciones";
    var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

    if (pendingMigrations.Any())
    {
        Console.Error.WriteLine("La base de datos tiene migraciones pendientes. Aplíquelas antes de aprovisionar el administrador.");
        return 1;
    }

    var userManager = services.GetRequiredService<UserManager<PortfolioUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    currentStage = "lectura y validación de credenciales";
    var email = ReadEmail();

    if (!new EmailAddressAttribute().IsValid(email))
    {
        Console.Error.WriteLine("La dirección de correo no es válida.");
        return 1;
    }

    var password = ReadHidden("Contraseña: ");
    var confirmation = ReadHidden("Repita la contraseña: ");

    if (!string.Equals(password, confirmation, StringComparison.Ordinal))
    {
        Console.Error.WriteLine("Las contraseñas no coinciden.");
        return 1;
    }

    currentStage = "apertura de transacción";
    await using var transaction = await dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

    currentStage = "comprobación o creación del rol administrador";
    if (!await roleManager.RoleExistsAsync(PortfolioAuthorization.AdministratorRole))
    {
        var roleResult = await roleManager.CreateAsync(
            new IdentityRole(PortfolioAuthorization.AdministratorRole));

        if (!roleResult.Succeeded)
        {
            PrintIdentityErrors(roleResult);
            return 1;
        }
    }

    currentStage = "comprobación de administradores existentes";
    if ((await userManager.GetUsersInRoleAsync(PortfolioAuthorization.AdministratorRole)).Count != 0)
    {
        Console.Error.WriteLine("Ya existe un usuario administrador. No se ha creado ninguna cuenta.");
        return 1;
    }

    currentStage = "comprobación del correo existente";
    if (await userManager.FindByEmailAsync(email) is not null)
    {
        Console.Error.WriteLine("Ya existe una cuenta con ese correo. No se ha modificado ninguna cuenta.");
        return 1;
    }

    var user = new PortfolioUser
    {
        UserName = email,
        Email = email
    };
    currentStage = "creación del usuario";
    var createResult = await userManager.CreateAsync(user, password);

    if (!createResult.Succeeded)
    {
        PrintIdentityErrors(createResult);
        return 1;
    }

    currentStage = "asignación del rol administrador";
    var assignRoleResult = await userManager.AddToRoleAsync(user, PortfolioAuthorization.AdministratorRole);
    if (!assignRoleResult.Succeeded)
    {
        await userManager.DeleteAsync(user);
        PrintIdentityErrors(assignRoleResult);
        return 1;
    }

    currentStage = "confirmación de la transacción";
    await transaction.CommitAsync();
    Console.WriteLine("La cuenta de administrador se ha creado correctamente.");
    return 0;
}
catch (Exception exception)
{
    var sqlException = EnumerateExceptions(exception).OfType<SqlException>().FirstOrDefault();
    var diagnostic = sqlException is not null
        ? $"SQL Server {sqlException.Number}"
        : string.Join(" → ", EnumerateExceptions(exception)
            .Select(innerException => innerException.GetType().Name)
            .Distinct()
            .Take(5));
    Console.Error.WriteLine($"No se completó el aprovisionamiento. Etapa: {currentStage}. Diagnóstico seguro: {diagnostic}. No comparta credenciales ni registros sensibles.");
    return 1;
}

static IEnumerable<Exception> EnumerateExceptions(Exception exception)
{
    var pending = new Stack<Exception>();
    pending.Push(exception);

    while (pending.TryPop(out var current))
    {
        yield return current;

        if (current is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                pending.Push(innerException);
            }
        }
        else if (current.InnerException is not null)
        {
            pending.Push(current.InnerException);
        }
    }
}

static string ReadEmail()
{
    Console.Write("Correo del administrador: ");
    return Console.ReadLine()?.Trim() ?? string.Empty;
}

static string ReadHidden(string prompt)
{
    Console.Write(prompt);
    var characters = new List<char>();

    while (true)
    {
        var key = Console.ReadKey(intercept: true);
        if (key.Key == ConsoleKey.Enter)
        {
            Console.WriteLine();
            return new string(characters.ToArray());
        }

        if (key.Key == ConsoleKey.Backspace)
        {
            if (characters.Count > 0)
            {
                characters.RemoveAt(characters.Count - 1);
            }

            continue;
        }

        if (!char.IsControl(key.KeyChar))
        {
            characters.Add(key.KeyChar);
        }
    }
}

static void PrintIdentityErrors(IdentityResult result)
{
    Console.Error.WriteLine("Identity rechazó la operación:");
    foreach (var error in result.Errors)
    {
        Console.Error.WriteLine(error.Code);
    }
}

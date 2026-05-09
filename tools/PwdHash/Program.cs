using Microsoft.AspNetCore.Identity;

var password = args.Length > 0 ? args[0] : "ChangeMe!123";
var hasher = new PasswordHasher<IdentityUser>();
var hash = hasher.HashPassword(new IdentityUser(), password);
Console.WriteLine(hash);

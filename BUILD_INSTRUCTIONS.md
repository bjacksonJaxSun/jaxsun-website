# Build Instructions for JaxSun.Web

## Fix NuGet Package Restore Issue

The project file has been corrected. Follow these steps in Visual Studio to resolve the build issue:

### Step 1: Clean and Restore
1. **Clean Solution:** In Visual Studio, go to `Build` → `Clean Solution`
2. **Restore NuGet Packages:** Right-click on the solution in Solution Explorer → `Restore NuGet Packages`
3. **Rebuild Solution:** Go to `Build` → `Rebuild Solution`

### Step 2: Alternative Command Line (if .NET CLI is available)
```bash
cd C:\Development\JaxSun.us\JaxSun.Web
dotnet clean
dotnet restore
dotnet build
```

### Step 3: If Still Having Issues
1. **Delete bin and obj folders:**
   - Navigate to `C:\Development\JaxSun.us\JaxSun.Web\`
   - Delete the `bin` and `obj` folders
   - Rebuild the solution

2. **Check .NET Version:**
   - Ensure .NET 8.0 SDK is installed
   - In Visual Studio, check `Help` → `About Microsoft Visual Studio`
   - Look for ".NET" in the installed products

### Step 4: Package Manager Console (Alternative)
If the above doesn't work, try the Package Manager Console in Visual Studio:
1. Go to `Tools` → `NuGet Package Manager` → `Package Manager Console`
2. Run: `Update-Package -reinstall`

## Project File Changes Made
- Removed incompatible `Microsoft.AspNetCore.Mvc` version 2.2.0
- Removed unnecessary explicit package references (they're included in .NET 8.0 SDK)
- Kept only `Microsoft.EntityFrameworkCore.SQLite` for future database support

## What Should Work After Fix
- Project should build successfully
- All ASP.NET Core MVC features should work
- Email functionality should work (after configuring SMTP settings)
- All views and controllers should be accessible

## Next Steps After Successful Build
1. Configure email settings in `appsettings.json`
2. Add required images to `wwwroot/images/` folder
3. Run the application (F5 in Visual Studio or `dotnet run`)
4. Test the website functionality

## Troubleshooting
If you continue to have issues:
1. Check that Visual Studio is updated to support .NET 8.0
2. Verify the .NET 8.0 SDK is installed on your system
3. Try creating a new ASP.NET Core MVC project to test your setup
4. Contact support if the issue persists

The project structure and code are correct - this is purely a NuGet package restoration issue that should resolve with the above steps.
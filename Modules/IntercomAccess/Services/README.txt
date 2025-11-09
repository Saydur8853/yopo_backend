This module was added for intercom access control. Remember to register the service in DI in Program.cs and add migrations:
- Register: builder.Services.AddScoped<YopoBackend.Modules.IntercomAccess.Services.IIntercomAccessService, YopoBackend.Modules.IntercomAccess.Services.IntercomAccessService>();
- Run EF migrations: dotnet ef migrations add AddIntercomAccessTables && dotnet ef database update
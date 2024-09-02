# eco

-Add Migration :  dotnet ef migrations add Initial --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext -o Migrations/Application
-Update database :  dotnet ef database update --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext
-Remove Migration : dotnet ef migration remove --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext
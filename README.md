# eco

-Add Migration :  dotnet ef migrations add Initial --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext -o Migrations/Application
-Update database :  dotnet ef database update --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext
-Remove Migration : dotnet ef migration remove --project ../../Migrators/Migrators.MSSQL/ --context ApplicationDbContext


# Run docker
1. Open docker desktop
2. root project run cmd
    ```
    docker-compose -f docker/docker-compose.yml -f docker/docker-compose.override.yml up --build
    ```
3. link swagger: http://localhost:8000/swagger/index.html#/
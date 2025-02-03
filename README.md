REST API with ASP.NET Core to learn C#
Task manager API with CRUD
Using Swagger for testing

=> Controllers, dependency injection, database interaction using Entity Framework Core

=> Use Model View Controller (MVC) using ASP.NET Core for organization

Models: Defines the data structure - blueprint. Defines what a "task" looks like in db
DbContext: Handles database interactions and communication, store and retrieve tasks
Controllers: Handle API requests and return responses (define API endpoints: lets users interact with API with GET tasks, POST new tasks, etc) (e.g., /api/tasks)


Data Transfer Objects (DTO): Control what data is sent or recieved from API -> don't want to expose full model with sensitive fields
Service layer (Business logic): keeps controllers clean, we write logic inside to service class instead of inside controllers


Database Injection: allows us to inject services instead of manually creating DI's
Middleware: Handle requests and responses. Middleware are components that process requests BEFORE reaching controller, or AFTER response is sent=> helps with logging, auth, CORS, and error handling
Authentication and Authorization: For security, we use JWT Authentication here for securing API
CORS: Cross-Origin Requests, for API to be accessed from frontend app (vue, react, etc.) to make API requests
Logging: For tracking errors and debugging
Unit Testing: unit tests normal



First start with Models, DbContext, and Controllers to create basic API
Second add DTOs and Services to improve the structure
Secure API with Authentication and CORS
Implement logging and testing


# After edits done: rebuild and run API
```bash
docker build -t taskapi .
docker run --rm -it -v $(pwd):/app -p 5000:5000 taskapi
```

# Setup, Models, DbContext, Controllers, Running:
1. Install .NET sdk (not needed if you will be using my given Dockerfile => recommended)
https://learn.microsoft.com/en-us/dotnet/core/install/linux

I am using Docker container for .NET environment since my fedora 37 isn't supported
Ensure that Docker is installed


2. Run terminal inside .NET container (I am running mounted volume so I can work on files in local system)

Mounted volume: (use mounted volume to run docker terminal)
```bash
docker run --rm -it -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 /bin/bash
```

Non mounted volume (can use after setup is complete, for now use mounted volume):
```bash
docker run --rm -it mcr.microsoft.com/dotnet/sdk:8.0 /bin/bash
```

3. Create project inside mounted directory

```bash
dotnet new webapi -o TaskApi
cd TaskApi
```

4. Create Model directory inside TaskApi, and create TaskModels.cs => Represents task in database (use [key], [Required] to enforce rules)

5. Create Data directory inside TaskApi, and create AppDbContext.cs => 1st. Connect DbContext to connect to database, & 2nd. Define task table using DbSet<TaskModel>

6. Configure Database in appsettings.json (Using sqlite) => create and add database connection "ConnectionStrings"

7. Register Database in current Program.cs. Register AppDbContext so ASP.NET can use it, and enable Swagger UI for testing API, edit current Program.cs to use SQLite (// Register SQLite Database Context)

8. Create Task Controller (TasksControllers.cs) inside created Controllers/ directory. Has CRUD endpoints (GET tasks => get all tasks, GET tasks/id => Get single task with id, POST task => Create new task, PUT tasks/id => Update task with id, DELETE tasks/id => Delete a task with id)

9. Apply Database Migrations (Should only need to do once, persists from Dockerfile line: ENV PATH="/root/.dotnet/tools:$PATH"
)

# Create Tasks table in DB (if doesn't work, see below):
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
Fixing errors:
(Might need to install dotnet.ef tool and add to path):

To run docker container terminal: inside of TaskApi dir run: 
```bash
docker run --rm -it -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 /bin/bash
```

Inside docker container install dotnet.ef run:
```bash
dotnet tool install --global dotnet-ef
```
For adding to path:
```bash
export PATH="$PATH:/root/.dotnet/tools"
```
Confirm installation is working:
```bash
dotnet ef --version
```

(Might need to install Microsoft.EntityFrameworkCore.Design in local terminal INSIDE of docker container terminal):
```bash
dotnet add package Microsoft.EntityFrameworkCore.Design
```
Check installation:
```bash
dotnet list package
```

Finally after installing, retry these two INSIDE of docker container terminal:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Containers reset when restarted, so need to nodify Dockerfile to install dotnet-ef when container starts, or use persistent volume for db files

------------------------------------------

10. Run & Test API:
<!-- 
```bash
docker run
``` -->
(Make sure you are exited outside of Docker)
```bash
docker build -t taskapi .
```

or if changes made use this to rebuild:
```bash
docker build --no-cache -t taskapi .
```

then:
```bash
docker run --rm -it -v $(pwd):/app -v $(pwd)/data:/root/.dotnet/tools -p 5000:5000 taskapi
```

=> Swagger opens at https://localhost:5000/swagger

when done developing:

```bash
docker run --rm -it -v $(pwd):/app -p 5000:5000 taskapi
```

Transfer permissions:

ls -lah

sudo chown -R $USER:$USER .

sudo chmod -R u+rwx .

maybe restart vscode 

exit  # If you're inside Docker, exit first
code .  # Reopen your project



# DTOs and Service Layer
11. Create DTOs/TaskDTO.cs => controls what data is sent and recieved by API, prevents exposing sensitive fields & allows for more structured API responses 
=> Prevent exposing 'Id' field (or other sensitve data), helps mapping data between different layers

12. Create Services Layer (Services/TaskService.cs)=> Separate logic from controllers (TaskControllers.cs), code is reusable, better for testing.
Need to modify current TaskControllers to use new Services from TaskService.cs

Need to register new TaskService in current Program.cs
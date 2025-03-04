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

For JWT Authentication in Program.cs run:
```bash
docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi
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

# when done developing:
```bash
docker run --rm -it -v $(pwd):/app -p 5000:5000 taskapi
```

For JWT Authentication in Program.cs run:
```bash
docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi
```


# Transfer permissions to save/modify files:

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
Need to modify current TaskControllers to use new Services from TaskService.cs (done)

Need to register new TaskService in current Program.cs (done)

# Dependency Injection for Database
13. Modify Program.cs to make sure db connection properly injected

# Custom Middleware
14. Create Middleware/CustomMiddleware.cs for handling logging, error handling, or modifying req before reaching controllers (done), register in Program.cs (#13)
// Logs request body, logs response body, ensures stream positions reset so pipeline cont normally, also tracks time taken for tasks to execute

# JWT Authentication and Authorization
15. Added JWT Auth in Program.cs, use secret key in Dockerfile run instead of storing inside of program.cs
docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi

Need to do one time: Install required packages for JWT
Run interactive session
```bash
docker run --rm -it -v $(pwd):/app -w /app mcr.microsoft.com/dotnet/sdk:8.0 /bin/bash
```
Then install package versions for JWT Auth:
```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.2
dotnet add package Microsoft.IdentityModel.Tokens --version 8.0.2
```
=> Implemented JWT token generation

=> Configured JWT authentication in Program.cs

=> Modified TasksController to require authentication - Protected Routes in TaskController with [authorize]

=> Fixed JWT Secret key issues (32 chars + pass as env variable when running as docker cmd: docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi)

=> Fixed data protection warnings about persistent encryption keys only in production 
- ensure persistent key storage in Docker by mounting volume when running: docker run --rm -it -v $(pwd)/data-protection:/app/DataProtection-Keys -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi

# Cors
16. For now CORS allows all origins, methods, and headers (modify later)

=======> add Role-Based Authorization (RBAC) using JWT claims next 
=======> add frontend and modify cors accordingly


# Frontend
17. Created Vue frontend
=> Changes CORS settings in program.cs to allow frontend

=> Created Services/api.js in Vue project for API url

=> Use .env for api url

=> Might need to install sqlite3 inside of docker container
i. Run frontend: docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi

ii. docker ps

Take container id from docker ps and put in below
iii. docker exec -it <container_id> /bin/sh

iv. check if db exists:
ls -l taskdb.sqlite

v: check if Tasks table has data
sqlite3 taskdb.sqlite

=> if sqlite error run this currently where you are inside docker:

```bash
apt update && apt install -y sqlite3
```

vi: Then run: SELECT * FROM Tasks;
If no tasks show, manually insert: 
INSERT INTO Tasks (Title, Description, Status, DueDate) VALUES ('Test Task', 'This is a test', 'Pending', '2025-12-31');


18. Finish setting up frontend and connecting to backend

-Set up API authentication in src/services/api.js

-Modify Task.vue to fetch tasks using JWT token

-App.vue now includes TaskView

=> Manually test setting token in developer console:
i.
```bash
curl -X POST http://localhost:5000/api/auth/login \
-H "Content-Type: application/json" \
-d '{"username": "admin", "password": "password"}'
```
ii.
Open dev tools -> console -> paste this with token in here
```bash
localStorage.setItem("token", "your_jwt_token_here");
```

(can check if went through in -> application -> local storage -> http://localhost:5173 -> token (should be here))






to run 

frontend: npm run dev

backend:
docker build --no-cache -t taskapi .
docker run --rm -it -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi 



docker build --no-cache -t taskapi .
docker run --rm -it -v $(pwd):/app -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi
<!-- docker run --rm -it -p 5000:5000 -e JwtSettings__Secret="YourVeryStrongSecretKeyWithAtLeast32Characters!" taskapi -->


to get token:
curl -X POST http://localhost:5000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username": "admin", "password": "password"}'

paste token inside YOUR_TOKEN_HERE

curl -H "Authorization: Bearer YOUR_TOKEN_HERE" http://localhost:5000/api/tasks

![Image](https://github.com/user-attachments/assets/9eab41eb-0911-4e34-8482-8ff4802a0591)


runs on http://127.0.0.1:5000/swagger/index.html

----------------------------

19. Setting up JWT token with logins automatically instead of manually

=> first had to edit Login in AuthController to return JWT token as cookie instead of 
response body and storing in local storage -> which is not as safe

=> Modify Program.cs to extract JWT from cookies, since by default it gets it from Authorization headers which we don't want to do

=> Modify services/api.js to not need manual token passed anymore

=> Add Login : Logout for storing : clearing JWT token in cookies in services/api.js

=> Now that JWT is set up so that cookies are sent automatically, removed manually passing authorization headers, added login() and logout() to handle authentication, getTasks() no longer needs token as an argument
=> We now can modify TaskView.vue to login/logout, and axios automatically includes JWT cookies on request, logout clears cookie and user sessions

=> Not using vue router, so just include login/logout/task fetching all in TaskView.vue
=> Need .AllowCredentials() in Program.cs CORS configuration builder.Services.AddCors() to allow credentials (cookies, authorization headers, ...), and app.UseCors("AllowFrontend");, also need 'withCredentials: true' in api.js

=> Default login is admin : password

20. Storing auth state using Pinia
Current flow to be changes: 
Backend: JWT authentication -> JWT sent as HTTP only cookie when logging in -> cookie automatically included in requests
Frontend: API requests use withCredentials: true so browser automatically sends JWT cookie with each requests, no state management yet for authentication status

TODO:
Need state management to store and manage JWT token in memory instead of local storage
Components can access user info without redundant api calls (gives global access)
App UI updates automatically (at login/logout/etc)
Better security, prevents XSS atks

21. Implement Vue router
=> Added router vue 'npm install vue-router@4' 
-Created src/router/index.js 
-Modify main.js to include Vue router
-Modify App.vue to include <router-view>

Change TaskView to not be single page, and created LoginView.vue separate from TaskView.vue

Update TaskView to check authentication, ensure that authenticated users can only see tasks



22. Implementing Pinia
=> Modify main.js to include Pinia

=> Added router vue 'npm install vue-router@4' 
Created src/router/index.js 
Modify main.js to include Vue router


=> Create Pinia store to manage authentication state new 'store/auth.js'
The state managements are the login, logout, fetching user 

=> 'services/api.js' already has 'withCredentials: true' from 19. storing jwt 

=> create 'src/views/LoginView.vue' for login form


# 23. Persistent states

npm install pinia-plugin-persistedstate

=> Modify auth.js to use persistent states 'persist: { ...}

=> Modify main.js to enable Pinia persistence "pinia.use(piniaPersist);"


# 24. Rate limiting for API (did it for create task and get tasks)
OLDER RATE LIMITING, NOT USING:
(Could use advanced rate limiting features from AspNetCoreRateLimit such as IP limits or user limits, for now just do built in .NET limiting)

=> (ONLY NEEDED FOR .NET 6 AND BELOW) Add dotnet add package Microsoft.AspNetCore.RateLimiting in Dockerfile

=> Modify Program.cs to configure rate limiting "builder.Services.AddRateLimiter" & "app.UseRateLimiter();"

=> Apply configured rate limiting in controllers. Ex. "TasksController.cs" apply [EnableRateLimiting]

Test rate limiting:
For GetTasks()
for i in {1..15}; do curl -X GET http://localhost:5000/api/tasks; echo ""; done

Tried using Redis rate limiting. Wasted 5 hours. Moving on

For CreateTask()
for i in {1..15}; do curl -X POST -H "Content-Type: application/json" -d '{"title": "New Task"}' http://localhost:5000/api/tasks; echo ""; done

# 25. Email Verification
=> Create User model to add email confirmation for verification: (models/UserModel.cs)
-Track verification, Unique token for verification, and Expiration for verification token

=> Add User model to database (update AppDbContext.cs to include User model) -> Need to run migrations to update database like before:
Run inside docker:
i.docker ps
ii. docker exec -it <container_id> /bin/sh
iii. 
```bash
dotnet ef migrations add AddUserTable
dotnet ef database update
```

=> Edit AuthController.cs

For local development:
Set up SMTP environment variables to persist across sessions in shell profile
using bash or zsh add:

```bash
export SMTP_HOST="smtp.gmail.com"
export SMTP_PORT="587"
export SMTP_USER="your-email@gmail.com"
export SMTP_PASS="your-secure-password"
export FROM_EMAIL="your-email@gmail.com"
export FROM_NAME="Task API"
```

then run to reload shell profile:

```bash
source ~/.bashrc
```

For deployment pass environment variables to container:
Use .env file, add variables there, and modify Dockerfile to load .env file then add .env to run file
I am going to use .env with Docker to load the variables in Docker container, and also going to use appsettings.json as a fall back

!!! Modify 'Program.cs' to load environment variables, and also fallback on appsettings.json

Add to docker:
ENV SMTP_HOST=$SMTP_HOST
ENV SMTP_PORT=$SMTP_PORT
ENV SMTP_USER=$SMTP_USER
ENV SMTP_PASS=$SMTP_PASS
ENV FROM_EMAIL=$FROM_EMAIL
ENV FROM_NAME=$FROM_NAME

build and run: (have to add .env)
*dont need to manually pass .env file inside docker run if using docker-compose.yml*

docker build -t taskapi .
'docker run --env-file .env -p 5000:5000 taskapi'

Since this is the case, I will create a docker-compose.yml file
=> run with:
```bash
docker-compose up --build
```
Start using PostgreSQL instead of SQLite
need to add: RUN dotnet add package Microsoft.EntityFrameworkCore.Pgsql
need to edit appsettings.json to use PostgreSQL instead of SQLite as well
need to modify Program.cs to read PostgreSQL instead of SQLite
need to install "dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL" inside docker container:
i. docker ps

Take container id from docker ps and put in below
ii. docker exec -it <container_id> /bin/sh

iii. dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

Make sure postgresql is running:
sudo systemctl start postgresql

=> Rebuild backend

=> Run Migrations for PostgreSQL instead of SQLite
docker compose -f 'docker-compose.yml' up -d --build 'database' 
docker exec -it taskapi_backend dotnet ef migrations add InitPostgres
docker exec -it taskapi_backend dotnet ef database update

Can test logs:
docker logs -f taskapi_backend

=> Start everything:
docker-compose up -d


https://jwt.io/
had to debug jwt not working for 3 days. the whole time it was invalid signature and this site proved it
ex invalid: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMSIsIm5hbWUiOiJ1c2VyMSIsImp0aSI6ImYwNGQyNDU1LWEwNjEtNDRhZi05N2QzLWVkZmYyMjg2YThiZCIsImV4cCI6MTczOTI5NDM5MH0.Bcp_sc2dECCJdUbB-FDUj0oEl3tjGhocoozOxH3GMIA

http://localhost:5173/



rehash password manually:
print(bcrypt.hashpw(b"test123", bcrypt.gensalt()).decode())


delete all docker volumes:
docker volume rm $(docker volume ls -q)


------------------------------
# After code changes: 
docker compose restart 

# => To manually get into database:
(docker backend has to be running obviously)
docker exec -it taskapi_db psql -U taskuser -d taskdb

# => Restart Docker:
sudo systemctl restart docker

# => Stop all services:
docker stop taskapi_backend taskapi_frontend taskapi_db

# => Remove all services:
docker rm taskapi_backend taskapi_frontend taskapi_db


# => Build AND Start all services (frontend, backend, database): 
docker compose -f 'docker-compose.yml' up -d --build 

[+] Running 9/9
 ✔ frontend 8 layers [⣿⣿⣿⣿⣿⣿⣿⣿]      0B/0B      Pulled                                                                                                                                                                                27.2s 
   ✔ a492eee5e559 Pull complete                                                                                                                                                                                                        1.6s 
   ✔ 32b550be6cb6 Pull complete                                                                                                                                                                                                        1.1s 
   ✔ 35af2a7690f2 Pull complete                                                                                                                                                                                                        2.2s 
   ✔ 7576b00d9bb1 Pull complete                                                                                                                                                                                                        6.6s 
   ✔ d55bd4ee018b Pull complete                                                                                                                                                                                                        2.1s 
   ✔ 699c77750bbf Pull complete                                                                                                                                                                                                        3.6s 
   ✔ a90d998fbd35 Pull complete                                                                                                                                                                                                        2.8s 
   ✔ 0d7872d72f6b Pull complete                                                                                                                                                                                                        3.4s 
[+] Building 0.7s (22/22) FINISHED 

=> Start database docker-compose.yml:
docker compose -f 'docker-compose.yml' up -d --build 'database' 

[+] Running 1/0
 ✔ Container taskapi_db  Running 

=> Start backend AND database docker:
docker compose -f 'docker-compose.yml' up -d --build 'backend' 

[+] Running 2/2
 ✔ Container taskapi_db               Running                                                                                                                                                                                          0.0s 
 ✔ Container csharpproject-backend-1  Started   

=> Start Frontend only
docker compose -f 'docker-compose.yml' up -d --build 'frontend' 

 --------------------------------------

Register new user to test:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "testuser@example.com",
    "password": "SecurePassword123"
  }'
```

---------------
To get JWT secret:

curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username": "admin", "password": "test123"}' \
  -c cookies.txt

--------------

---------------

Show AuthToken in: 
dev console -> application -> cookies


test:
----
admin
test123


test:

admin
test123
----

user1 
password123

user2
securepass456

---
To check logs:

docker logs -f <container_id>



database schema:
CREATE TABLE "Users" ( 
    "Id" SERIAL PRIMARY KEY,
    "Username" VARCHAR(255) NOT NULL UNIQUE,
    "Email" VARCHAR(255) NOT NULL UNIQUE,
    "PasswordHash" TEXT NOT NULL,
    "EmailConfirmed" BOOLEAN DEFAULT FALSE
);

CREATE TABLE "Tasks" ( 
    "Id" SERIAL PRIMARY KEY,
    "Title" VARCHAR(255) NOT NULL,
    "Description" TEXT,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "DueDate" TIMESTAMP,
    "UserId" INT REFERENCES "Users"("Id") ON DELETE CASCADE -- Assigns task to a user
);


$2b$12$ctDEmd5rvOeYvy.JXf2GieN6Aq.suW4yDzz3DA33AdfmsmUgtFKnS

manually hash password: inside python: run: 

i. python 

ii. paste
import bcrypt

# Enter your password here
password = "test123" 

# Hash the password
hashed = bcrypt.hashpw(password.encode(), bcrypt.gensalt(12))

print("Hashed Password:", hashed.decode())  # Print hashed password for manual database insert


check if hashpass word is same as real password:

import bcrypt

hashed_password = b"$2b$12$CLGLI.eRm4ia6hriniKpLOYSVHOGBK7xrsj1qhAo8SpsTCTqaldVm"
password_attempt = "password123"

# Check if password matches
if bcrypt.checkpw(password_attempt.encode(), hashed_password):
    print("✅ Password matches!")
else:
    print("❌ Invalid password")




# Python script to verify token sig

import jwt

secret = "YourVeryStrongSecretKeyWithAtLeast32Characters!"
token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyMSIsIm5hbWUiOiJ1c2VyMSIsImp0aSI6ImVmNTM1ODY2LTI4NjItNDQ4Yy1iOTg1LTg1ZjczYjU2OTZhMyIsImV4cCI6MTczOTI5NTE1OH0.RoYjUI_69_ORppo8mTiMovdPm0QPPdDlhW0Y6YC-Zo8"

try:
    decoded = jwt.decode(token, secret, algorithms=["HS256"])
    print("✅ Token is valid:", decoded)
except jwt.ExpiredSignatureError:
    print("❌ Token has expired")
except jwt.InvalidTokenError:
    print("❌ Invalid token signature")


=----

timestamp decoding
https://www.unixtimestamp.com/index.php
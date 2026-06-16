# Virus Scanner Backend

## About the Project
> Virus Scanner is a secure, high-performance web application designed for scanning files and analyzing potential security threats. The project aims to provide a centralized hub where users can upload files, check real-time scan histories, and manage their security configurations. The backend ensures robust user authentication via secure cookies, dynamic database-backed profile management, and scalable file upload processing to deliver a seamless and safe user experience.

---

## Created by:
- **Fülöp Attila Ákos** (Backend, SQL Database, Frontend)

---

## Database Schema (MySQL)


### users
- `Id` (uint / int, PK)
- `Email` (string)
- `UserName` (string)
- `Password` (string, Hashed)
- `Role` (string)
- `Profile_Pic` (string)

### viruses
- `Id` (int, PK)
- `FileName` (string)
- `VirusName` (string)
- `virusType` (string)
- `userId` (int, FK to users.Id)

---

### postman test : 
https://documenter.getpostman.com/view/48108190/2sBXwtppHo



## Project Structure

```markdown
├── Controllers/
│   ├── AuthController.cs
│   └── UploadController.cs
├── models/
│   ├── AppDbContext.cs
│   ├── AuthDtos.cs
│   ├── ScanModels.cs
│   ├── UpdateUserNameDto.cs
│   ├── User.cs
│   └── Viruses.cs
├── wwwroot/
│   └── uploads/
├── Program.cs
├── appsettings.json
└── WebApplication1.sln
Technologies & Packages UsedBackend DependenciesMicrosoft.AspNetCore.Authentication.JwtBearer (Token validation)Microsoft.EntityFrameworkCore.MySql (Database connection via Pomelo)BCrypt.Net-Next (Secure password hashing)System.IdentityModel.Tokens.Jwt (JWT token generation)Frontend Dependenciesreact (Frontend UI library)react-router-dom (Client-side routing & navigation)bootstrap (Styling framework & responsive navbar layouts)axios / api-client (Handling HTTP requests with credentials)Development ToolsVisual StudioPostmanMySQL / PhpMyAdminGitHubAPI & Postman DocumentationThe backend API endpoints are fully mapped, secured, and tested. You can find the interactive documentation below:Virus Scanner Postman DocumentationCore Endpoints SummaryMethodEndpointAuthenticationDescriptionPOST/api/auth/registerNoneRegisters a new user with default settings.POST/api/auth/loginNoneAuthenticates user, sets X-Auth-Token HttpOnly cookie.POST/api/auth/logoutNoneClears the X-Auth-Token cookie instantly.GET/api/auth/whoami[Authorize]Fetches active user state directly from the database.PUT/api/auth/updateUserName[Authorize]Updates profile username and refreshes auth data.POST/api/auth/uploadPfp[Authorize]Uploads a profile picture to wwwroot/uploads/.GET/api/auth/history[Authorize]Returns the file scanning history for the logged-in user.



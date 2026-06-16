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




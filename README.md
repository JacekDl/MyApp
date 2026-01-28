# PharmCare

PharmCare is a web application created as part of an undergraduate thesis.  
The system supports communication between patients and pharmacists and helps manage pharmacotherapy and treatment plans.

## Features
- User authentication and authorization (ASP.NET Core Identity)
- Login via external provider (Google OAuth 2.0)
- Role-based access (Patient, Pharmacist, Administrator)
- Treatment plan and medication management
- Secure data storage and access control

## Technology Stack
- ASP.NET Core MVC
- Entity Framework Core
- SQLite
- ASP.NET Core Identity
- MediatR
- Bootstrap

## Architecture
The application is designed according to Clean Architecture and the hexagonal (ports and adapters) architecture pattern, ensuring separation of concerns and high maintainability.

## Purpose
The goal of the project is to improve pharmaceutical care by enabling easier contact between patients and pharmacists and supporting safe and effective pharmacotherapy.

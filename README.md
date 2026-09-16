# UniFind - University Lost & Found

UniFind is a university lost-and-found website built with ASP.NET Core MVC. It gives students and staff one place to report missing items, report things they have found, search campus listings, and arrange a safe return.

## What it can do

- Browse lost and found reports without signing in
- Search by keyword, item type, category, location, and date
- Create and manage lost-item and found-item reports
- Submit ownership claims for found items
- Tell the owner about an item found from a lost-item post
- Review claims and provide collection or handover instructions
- Show promised rewards and collection details after approval
- Send in-app notifications about new claims and decisions
- Manage a profile, including name and password changes
- Provide an administrator dashboard for campus-wide oversight
- Manage item categories and campus locations

The interface uses a calm beige-and-brown theme and Bootstrap Icons to keep the site friendly, readable, and focused on the lost-and-found workflow.

## Technology

- ASP.NET Core MVC
- .NET 8
- Entity Framework Core
- ASP.NET Core Identity
- SQLite by default
- SQL Server support
- Razor views
- Bootstrap 5 and Bootstrap Icons

## Running the project

### Requirements

- .NET 8 SDK
- A browser

### Start locally

From the project folder, run:

```bash
dotnet restore
dotnet run
```

Open the local URL shown in the terminal.

The application creates the local SQLite database and seeds the initial roles, sample data, and demo accounts on startup.

## Demo accounts

The seeded demo accounts are:

```text
Student
Email: student@university.edu
Password: Student123!

Administrator
Email: admin@university.edu
Password: Admin123!
```

Change or remove these credentials before using the application outside a local demonstration.

## Main user roles

### Regular user

A regular user can report items, search the catalog, submit claims, respond when they find a lost item, review their own reports, and track claim decisions.

### Administrator

The administrator provides oversight for the whole campus. They can review claims, view all posts, manage categories and locations, and help resolve cases where the finder and owner need official assistance.

## Project structure

```text
Controllers/   Request handling and application workflows
Data/          Database context and seed data
Models/        Items, claims, users, categories, and locations
Services/      Supporting services such as image uploads
ViewModels/    Data used by forms and pages
Views/         Razor page templates
wwwroot/       CSS, JavaScript, and uploaded static assets
```

## Database configuration

SQLite is the default provider and stores data in `UniversityLostAndFound.db`. SQL Server can be selected through the `DatabaseProvider` setting in `appsettings.json`.

This is a university project intended for learning and demonstration. For production use, add stronger account policies, secure deployment configuration, proper email or push notifications, and a managed database.

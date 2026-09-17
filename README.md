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

## Deploying to Render

The repository includes a [`Dockerfile`](./Dockerfile) and [`render.yaml`](./render.yaml) for deployment. The Blueprint uses SQLite on a Render persistent disk so the database and uploaded images survive restarts and redeploys. The `starter` plan is required because persistent disks are not available on Render's free web service plan.

### Before deploying

1. Push this repository, including `Dockerfile` and `render.yaml`, to GitHub or GitLab.
2. Create strong, unique values for the four seed-account environment variables. Do not use the demo passwords from this README:
   - `AdminSeed__Email`
   - `AdminSeed__Password`
   - `StudentSeed__Email`
   - `StudentSeed__Password`
3. If you need free hosting, use an external managed database and object storage instead of the included SQLite/persistent-disk setup. Render's free filesystem is temporary.

### Step-by-step Render deployment

1. Sign in at [render.com](https://render.com) and connect the Git provider that contains this repository.
2. Select **New +** and choose **Blueprint**.
3. Select the repository and branch, then click **Apply**. Render reads `render.yaml`, creates the web service, and attaches the persistent disk.
4. When Render asks for values marked **sync: false**, enter the four unique seed-account values. These become secrets in the service environment and are not stored in Git.
5. Wait for the first Docker build and deployment to finish. The first startup creates the SQLite schema and seeds roles, categories, locations, sample items, and the two configured accounts.
6. Open the service URL shown on the Render service page. Confirm the home page loads, register or sign in, upload an image, and verify that the image appears after refreshing the page.
7. Sign in with the configured administrator account and immediately change its password from the profile page. Remove seeded sample items if this is a real deployment.
8. In Render, open **Settings** and confirm the disk is mounted at `/var/data`. Never remove or recreate this disk unless you have a backup.
9. For a custom domain, open **Settings > Custom Domains**, add the domain, create the DNS record Render shows, and wait for TLS provisioning. Then test the HTTPS URL.

### Updating the application

Push changes to the connected branch. Render rebuilds the Docker image and redeploys automatically. The persistent disk remains attached, so the SQLite database and uploaded images remain available.

### Important production limitations

- SQLite is suitable for a small demonstration or low-traffic deployment. For multiple application instances or higher traffic, add PostgreSQL support and migrate the data.
- Uploaded files are stored on the attached disk. Back up the disk regularly; a disk is not a substitute for backups.
- The application currently seeds demo data on an empty database. Review and remove sample records before inviting real users.

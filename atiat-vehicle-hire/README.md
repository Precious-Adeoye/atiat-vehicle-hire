# ATIAT Vehicle Hire Digital Request & Operations System

A submission-ready MVP for ATIAT that adds a structured online vehicle-hire request channel while keeping human review, WhatsApp follow-up and operational control.

## Stack
- Backend: C# / ASP.NET Core 8 Minimal API
- Database: SQLite + Entity Framework Core
- Frontend: HTML, CSS, JavaScript
- Backend deployment: Render (Docker)
- Frontend deployment: Vercel or Netlify

## Features
- Customer vehicle-hire request form
- Automatic request reference number
- Request validation
- Staff dashboard
- Request status tracking
- Basic fleet availability display
- Summary metrics
- Green/white ATIAT-style UI
- CORS support
- Admin key for dashboard API endpoints

## Local run
Backend:
```bash
dotnet restore
dotnet run --urls http://localhost:5000
```
Frontend can be opened with VS Code Live Server or another static server on port 5500. Set `frontend/config.js` to `http://localhost:5000`.

Demo admin key: `ATIAT-DEMO-2026`

## Render
Create a new Web Service from the GitHub repository. Select Docker and set the root directory to `backend` (or deploy using the included render.yaml).
Set environment variables:
- `AdminKey` = a private value
- `FrontendUrl` = your Vercel/Netlify URL

After deployment, copy the Render API URL into `frontend/config.js`:
`window.ATIAT_API_URL = "https://YOUR-API.onrender.com";`

## Vercel / Netlify
Deploy the `frontend` folder as a static site. No build command is required.

## Important demo note
SQLite on a free Render web service uses the service filesystem. For a real production system, move the database to a persistent PostgreSQL/MySQL provider and add proper staff authentication/authorization. This MVP is intended for the ATIAT graduate trainee project demonstration.

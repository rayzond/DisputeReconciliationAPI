# Dispute Reconciliation API

A lightweight ASP.NET Core Web API for reconciling dispute records against internal data and generating detailed summary reports.

## Overview

This service receives dispute records (CSV, XML, or JSON) from external providers, compares them against an in-memory (or EF Core InMemory) database of internal disputes, and outputs a formatted text report with two sections:

1. **Audit**: A log of every incoming dispute with key details.
2. **Alerts**: High‑severity issues detected (ID mismatches, transaction mismatches, amount differences, status mismatches).

## Features

- **Multi‑format input**: CSV, XML, and JSON parsing via `DisputeFileParser`.
- **Entity Framework Core**: InMemory database seeded with mock disputes.
- **Parallelized comparison**: Efficiently compares incoming disputes and internal records.
- **Severity‑based alerts**: Flags amount discrepancies, missing records, and more with bold/high‑severity formatting.
- **Pagination endpoint**: `GET /api/dispute/internal?page={n}` returns 50 records per page.
- **Downloadable reports**: `POST /api/dispute/compare/file` and `POST /api/dispute/compare/json` endpoints return a `.txt` report for download.
- **Clean architecture**: Separation of concerns between Controllers, Services, Parsers, and Data Access (DAO).

## Tech Stack

- **.NET 8 / ASP.NET Core Web API**
- **Entity Framework Core InMemory**
- **NUnit** for unit testing
- **Swagger UI / OpenAPI**
- **Docker** for containerization

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (optional)
- Visual Studio 2022 or Visual Studio Code (with C# extension)

### Running Locally with Docker

1. **Build the Docker image**
   ```bash
   docker build -t dispute-reconciliation-api .
   ```
2. **Run the container**
   ```bash
   docker run -d -p 5000:80 --name dispute-api dispute-reconciliation-api
   ```
3. **Access Swagger UI**
   Open http://localhost:5000/swagger in your browser.

### Running Locally with Visual Studio

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/dispute-reconciliation-api.git
   cd dispute-reconciliation-api
   ```
2. Open the `.sln` in Visual Studio 2022.
3. Set `DisputeReconciliation.App` as the startup project.
4. Press **F5** to build and run. Swagger UI will open automatically.

### Running Locally with VS Code

1. Clone and open the folder:
   ```bash
   git clone https://github.com/yourusername/dispute-reconciliation-api.git
   code dispute-reconciliation-api
   ```
2. Install the **C#** extension when prompted.
3. In the Debug panel, select **.NET Core Launch (web)** and press **F5**.
4. Open http://localhost:5000/swagger.

## API Endpoints

- `GET /api/dispute/internal?page={n}`: Returns paged internal disputes (50 per page).
- `POST /api/dispute/compare/file`: Upload a CSV, XML and download a `.txt` summary report.
- `POST /api/dispute/compare/json`: Send a JSON array of disputes and download a `.txt` summary report.

---

*This project is provided under the MIT License.*


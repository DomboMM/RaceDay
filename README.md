# RaceDay Event Management System

## Project Overview

RaceDay is an event management system designed for South African road-running, walking and cycling events. The system provides a structured way for organisers to manage events and for participants to enrol in events and view their results.

## Project Purpose

The purpose of RaceDay is to provide a centralised system for managing road events, participants, event categories, enrolments and race results.

## User Roles

### Organiser

Organisers can:
- Create and manage events.
- Specify the event type, date, location and distance.
- Manage event categories.
- Manage participant enrolments.
- Record race results.

### Participant

Participants can:
- View available events.
- Enrol in events.
- Select an event category.
- View their race results.

## Database

The RaceDay database is implemented using Microsoft SQL Server.

The database contains six main tables:

- Users
- EventTypes
- Events
- Categories
- Enrolments
- Results

The database script is available in the `docs` folder.

## Entity Relationship Diagram

The Entity Relationship Diagram (ERD) showing the database entities, primary keys, foreign keys and relationships is available in the `docs` folder.

## API Endpoint Plan

The RESTful API endpoint plan describes the planned API operations for the RaceDay system. It is available in the `docs` folder.

## Part 1 Documentation

The following Part 1 documents are available in the `docs` folder:

- `RaceDay_ERD.pdf`
- `RaceDay_API_Endpoint_Plan.pdf`
- `RaceDay_Database.sql`

## Technologies

- C#
- ASP.NET Core Web API
- Microsoft SQL Server
- Entity Framework Core
- GitHub
- GitHub Actions

-    ## Setup Instructions
   1. Clone this repository: `git clone <repo-url>`
   2. Open SQL Server Management Studio (SSMS)
   3. Open `docs/RaceDay_Database.sql`
   4. Execute the script against a fresh database to create all tables and seed data
   5. Review `docs/RaceDay_ERD.pdf` for the full data model
   6. Review `docs/RaceDay_API_Endpoint_Plan.pdf` for the planned API structure


         ## User Roles

   **Organiser**
   - Creates, edits, and deletes events
   - Manages event categories
   - Captures participant results
   - Views all enrolments for their events

   **Participant**
   - Registers for an account
   - Browses available events
   - Enrols in an event by selecting a category
   - Views their own enrolments and results history

      
     ## CI/CD Status
   ![CI Success](docs/ci-success.png.jpg)

     ## Tech Stack
   - Database: Microsoft SQL Server (SSMS)
   - Planning Tools: draw.io (ERD), Markdown (API documentation)
   - Version Control: Git & GitHub
   - CI/CD: GitHub Actions

     ## Video Presentation

The Part 1 video presentation for the RaceDay project is available below:

[Watch the RaceDay Part 1 Video Presentation](https://youtu.be/B5_M79is2sk)

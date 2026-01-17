[![Mustackable](https://avatars.githubusercontent.com/u/200509271?s=96&v=4)](https://mustackable.dev)

<!-- TOC -->
  * [Intro](#intro)
  * [Features](#features)
  * [Getting Started](#getting-started)
    * [1. Create a Discord Webhook](#1-create-a-discord-webhook)
    * [2. Configure environment variables](#2-configure-environment-variables)
    * [3. Run with Docker Compose](#3-run-with-docker-compose)
  * [Authentication](#authentication)
  * [API Overview](#api-overview)
  * [Tech Stack](#tech-stack)
  * [License](#license)
<!-- TOC -->

## Intro

**Botyo** is a lightweight Discord notification bot with a REST API and SQLite persistence. It was built in 12 hours as a coding challenge.

## Features

- Schedule unlimited Discord notifications
- Cron-based scheduling
- REST API with Swagger UI
- API key authentication
- SQLite persistence
- Designed to run in Docker

## Getting Started

### 1. Create a Discord Webhook
You’ll need a Discord **channel webhook URL**.

Follow Discord’s official guide:  
https://support.discord.com/hc/en-us/articles/228383668-Intro-to-Webhooks

### 2. Configure environment variables

Create a `.env` file:

```env
ApiKey=your-secure-api-key
Discord__WebHook=https://discord.com/api/webhooks/...
```

### 3. Run with Docker Compose

```bash
docker compose up -d
```

The API will be available at:

- Swagger UI: http://localhost:55421/swagger
- API Base URL: http://localhost:55421


## Authentication

All endpoints are secured via an API key.

Send the key in the request header:

```http
ApiKey: your-secure-api-key
```

## API Overview

Main endpoints:

- `POST /Notifications` – Create a notification
- `GET /Notifications` – List notifications
- `GET /Notifications/{id}` – Get a notification
- `PUT /Notifications/{id}` – Update a notification
- `DELETE /Notifications/{id}` – Delete a notification
- `PATCH /Notifications/{id}/Start` – Enable notification
- `PATCH /Notifications/{id}/Stop` – Disable notification
- `POST /Notifications/{id}/Run` – Run immediately

All scheduling is done using cron expressions with **UTC time**!

## Tech Stack

- .NET 10
- ASP.NET Core
- SQLite
- Docker / Docker Compose
- Swagger (OpenAPI)

## License

MIT — feel free to use, modify, and improve.


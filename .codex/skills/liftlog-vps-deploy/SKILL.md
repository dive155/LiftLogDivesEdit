---
name: liftlog-vps-deploy
description: Deploy or update LiftLog's self-hosted backend and share-link web app on Dima's VPS. Use when working on LiftLog production hosting, VPS deployment, domains, or releases; not for ordinary app or backend development.
---

# LiftLog VPS deployment

Deploy the user's self-hosted LiftLog instance deliberately, with a traceable source version and an explicit production-release step.

## Current server layout

- VPS: `dima@138.249.8.32`; use the `dima-vps-connect` skill for every connection.
- Applications belong under `/projects`, which is owned by `dima`. Reserve `/projects/LiftLog` for this deployment.
- `divefrom.space` already resolves to this VPS and is served by Nginx. Its homepage is `/projects/HomePage`; do not overwrite it.
- Existing `/mealplan/` and `/exercise/` routes are reverse proxies and must remain working.
- The self-hosted instance uses existing-domain paths: `/liftlog/` for the web app and `/liftlog-api/` for the API. Do not change the existing homepage or its `/mealplan/` and `/exercise/` routes.

## Release model

Keep source control and production deployment separate:

1. Make, test, commit, and push backend changes from the developer's local repository to their chosen Git remote.
2. Build and publish a container image from that commit in CI. Use an immutable image tag or digest, rather than relying on a mutable `latest` tag.
3. On the VPS, update the Compose image reference to the selected release, pull it, and recreate only the affected service.
4. Verify the service health and a real HTTPS request before declaring the release complete.

The repository's existing `api-publish.yml` is an upstream GitHub Container Registry workflow. Before using it for the self-hosted instance, adapt it to the user's own fork/repository and registry image name.

Do not treat a local uncommitted working tree, a copied source directory, or `git pull` on the VPS as the production release mechanism. For this small personal deployment, building on the VPS from `/projects/LiftLog/source` is acceptable only after the revision has been committed and pushed; fetch and check out the exact commit SHA before building.

## Safety and state

- Read-only server inspection is allowed to establish the current layout. Obtain explicit approval immediately before creating directories, changing Nginx, DNS, containers, services, firewall rules, certificates, databases, secrets, or deploying a release.
- Run the API with PostgreSQL. Keep the database volume and all secrets outside the image; do not print secret values in terminal output or responses.
- Back up the database before schema migrations or destructive maintenance. Never delete volumes, images, or existing sites merely to make a deployment work.
- Keep the API private behind Nginx except for the required HTTPS endpoint; do not expose PostgreSQL publicly.

## Initial deployment order

Confirm the subdomain/DNS plan, prepare the versioned image build, create the deployment configuration under `/projects/LiftLog`, then add Nginx and TLS routing. Do not build or deploy until the user explicitly authorizes that phase.

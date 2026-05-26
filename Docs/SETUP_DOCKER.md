# SETUP DOCKER

1. `cp .env.example .env`
2. `docker compose up -d --build`
3. `docker compose ps`
4. `docker compose logs -f api`
5. `docker compose logs -f admin-web`
6. `docker compose logs -f kiosk-web`
7. `docker compose down -v`
8. Acesse:
   - API: http://localhost:8080/swagger
   - Admin: http://localhost:8081
   - Totem: http://localhost:8083

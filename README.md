# EduTrack (HTTTQL)

## Cấu trúc dự án

- `EduTrack.API/`: ASP.NET Core 8 Web API (JWT + EF Core + SQL Server)
- `edutrack-frontend/`: Vue 3 + Vite + Element Plus + Pinia + Axios

## Chạy Backend (API)

```bash
dotnet run --project EduTrack.API
```

Mặc định Swagger: `https://localhost:7xxx/swagger` (port do .NET tự cấp).

## Chạy Frontend

Tạo file `.env` từ mẫu:

```bash
cd edutrack-frontend
copy .env.example .env
```

Chạy dev:

```bash
npm run dev
```

## Login mẫu (skeleton)

- `admin` / `admin` → role `Admin`
- username khác bất kỳ / password bất kỳ → role `Teacher`

API login: `POST /api/auth/login`


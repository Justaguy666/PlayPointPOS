# START GUIDE - PlayPointPOS

Tai lieu nay danh cho nguoi moi clone source va chua build lan nao.

## 1) Yeu cau moi truong

Can cai dat truoc:

- Windows 10/11
- Visual Studio 2022 (co workload `.NET desktop development` + WinUI)
- .NET SDK `10.0.x`
- Node.js `22.x`
- Corepack (di kem Node 22) + Yarn `4.9.2`
- PostgreSQL `14+`

Kiem tra nhanh:

```powershell
dotnet --version
node --version
corepack --version
psql --version
```

## 2) Clone va cai dependency

Tai folder goc repo (`PlayPointPOS`):

```powershell
corepack enable
corepack prepare yarn@4.9.2 --activate
```

Cai dependency backend:

```powershell
cd API
yarn install
cd ..
```

## 3) Cau hinh database + env cho API

### 3.1 Tao database Postgres

Tao DB vi du:

- Database: `playpointpos`
- User: `postgres`
- Password: `postgres` (hoac tuy ban)

### 3.2 Tao file env

Trong folder `API`:

```powershell
copy .env.example .env
```

Mo `API/.env` va sua toi thieu:

- `DATABASE_URL=postgresql://user:password@localhost:5432/playpointpos`
- `JWT_ACCESS_SECRET=...`
- `JWT_REFRESH_SECRET=...`
- `SMTP_*` (can cho OTP qua email)

Luu y:

- Neu bo trong Cloudinary thi server van chay, chi upload anh bi `503`.
- OTP can SMTP hop le de gui mail.

## 4) Chay migration + seed du lieu test

Trong folder `API`:

```powershell
yarn migration:run
yarn seed:realistic
```

Tai khoan seed de login:

- Email: `seed.playpoint@demo.local`
- Password: `Seed@123456`

## 5) Run API

Trong folder `API`:

```powershell
yarn server
```

Mac dinh API chay o port `4000`.

## 6) Build/Run WinUI

Tu folder goc repo:

```powershell
dotnet build "WinUI/WinUI.csproj" -c Debug -p:Platform=x64
```

Run bang Visual Studio:

1. Mo solution `PlayPointPOS.slnx`
2. Chon startup project: `WinUI`
3. Chon Platform: `x64` (khong chon AnyCPU)
4. Bam Run

## 7) Cau hinh app lan dau

Khi app mo lan dau:

1. Vao config dialog
2. Dat:
   - Server Address: `http://localhost`
   - Port: `4000`
3. Save
4. Login bang account seed

## 8) Kich ban smoke test nhanh (5 phut)

Sau khi login:

1. Mo Dashboard, kiem tra co du lieu (khong phai mock)
2. Vao Product/Game/Member/Transaction, thu search
3. Mo Settings, kiem tra thong tin shop load duoc
4. Thu sua shop profile va bam Apply
5. Thu mo 1 area session va checkout

Neu cac buoc tren OK, he thong da chay dung.

## 9) Loi thuong gap va cach xu ly

### Loi `Packaged .NET applications ... AnyCPU`

Nguyen nhan: build WinUI bang AnyCPU.

Cach sua:

- Build voi `-p:Platform=x64`
- Hoac trong Visual Studio chon `x64`

### Loi lock file `Application.dll ... being used by another process`

Cach sua:

1. Tat app dang chay
2. Clean solution
3. Build lai
4. Neu van lock: restart Visual Studio

### Loi `TS6059 ... scripts/seedRealisticData.mts is not under rootDir`

Da fix trong `API/tsconfig.json` (include/exclude ro rang). Neu gap lai:

1. Kiem tra `tsconfig.json` con:
   - `"include": ["src/**/*.ts"]`
   - `"exclude": ["scripts", "dist", "node_modules"]`
2. Chay lai `yarn build`

## 10) Lenh tong hop cho nguoi moi

```powershell
# Tai root
corepack enable
corepack prepare yarn@4.9.2 --activate

cd API
yarn install
copy .env.example .env
# (sua .env)
yarn migration:run
yarn seed:realistic
yarn server

# Mo terminal moi tai root
cd ..
dotnet build "WinUI/WinUI.csproj" -c Debug -p:Platform=x64
```

Done. Ban da co backend + desktop app chay duoc tren may moi.

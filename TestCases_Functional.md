# Test Cases Kiem Thu Chuc Nang - PlayPointPOS

## Thong tin chung

- **Phien ban app**: Dien khi test (vd: `v1.0.0`)
- **Moi truong**: Windows 10/11, API + PostgreSQL
- **Nguoi test**: Dien ten
- **Ngay test**: Dien ngay

## Thang danh gia

- **Pass**: Dung ket qua mong doi
- **Fail**: Sai ket qua mong doi
- **Blocked**: Khong test duoc do loi he thong/phu thuoc

## Danh sach test case

| ID | Chuc nang | Tien dieu kien | Buoc thuc hien | Du lieu test | Ket qua mong doi | Thuc te | Trang thai |
|---|---|---|---|---|---|---|---|
| TC_AUTH_001 | Dang nhap thanh cong | Co tai khoan hop le | 1) Mo Login 2) Nhap email+password dung 3) Bam Dang nhap | `seed.playpoint@demo.local` / `Seed@123456` | Dang nhap thanh cong, vao Dashboard, hien thong bao thanh cong |  | Not Run |
| TC_AUTH_002 | Dang nhap sai mat khau | Co tai khoan hop le | 1) Mo Login 2) Nhap sai password 3) Bam Dang nhap | Email dung, password sai | Khong dang nhap, hien thong bao loi "Invalid email or password" |  | Not Run |
| TC_AUTH_003 | Dang nhap thieu thong tin | Khong | 1) Mo Login 2) Bo trong email hoac password | Email rong / Password rong | Nut Dang nhap bi vo hieu hoa hoac hien loi validation |  | Not Run |
| TC_OTP_001 | Gui OTP doi mat khau | Email da ton tai | 1) Mo Forgot Password 2) Nhap email 3) Bam Gui OTP | Email hop le | OTP duoc gui, hien thong bao thanh cong |  | Not Run |
| TC_OTP_002 | Cooldown nut Gui OTP | Dang o man OTP reset password | 1) Bam nut Gui 2) Quan sat nut | OTP mode reset password | Nut bi disable va dem nguoc 60s, het 60s thi bam lai duoc |  | Not Run |
| TC_OTP_003 | Reset password thanh cong | Da co OTP hop le | 1) Nhap OTP dung 2) Nhap mat khau moi 3) Bam xac nhan | OTP dung + password moi | Doi mat khau thanh cong, co thong bao thanh cong |  | Not Run |
| TC_OTP_004 | Reset password voi OTP sai | Da vao man OTP | 1) Nhap OTP sai 2) Bam xac nhan | OTP sai | Khong doi mat khau, hien thong bao OTP khong hop le |  | Not Run |
| TC_AREA_001 | Mo session voi member | Co area Available, co member | 1) Chon area Available 2) Mo session 3) Chon member 4) Xac nhan | GuestCount hop le | Tao session thanh cong, area chuyen sang Rented, co StartTime |  | Not Run |
| TC_AREA_002 | Chan mo session khi area dang Rented | Area da co session mo | 1) Thu mo session moi tren cung area | Area dang Rented | Khong tao session moi, hien thong bao loi phu hop |  | Not Run |
| TC_AREA_003 | Checkout session + extras | Area dang Rented, co san pham | 1) Mo Payment 2) Them extras 3) Chon payment method 4) Xac nhan | Product/Game extras + Cash/Banking | Tao transaction thanh cong, session dong, total tinh dung |  | Not Run |
| TC_DASH_001 | Dashboard dong bo du lieu backend | Da co du lieu transaction/member/area | 1) Mo Dashboard 2) Doi chieu voi du lieu backend | Du lieu seed | So lieu card, chart, trending hien dung theo backend, khong mock |  | Not Run |
| TC_DASH_002 | Export XLSX tu Dashboard | Co du lieu transaction | 1) Bam Export 2) Chon noi luu 3) Mo file | Khoang thoi gian bat ky | Tao file `.xlsx` thanh cong, co sheet tong quan/chi tiet theo ky |  | Not Run |
| TC_SEARCH_001 | Full-text search Game bo dau | Co game ten co dau (vd: "Ma soi") | 1) Mo Game page 2) Tim voi tu khong dau | `ma soi` | Van tim ra game "Ma soi" |  | Not Run |
| TC_SEARCH_002 | Full-text search Product theo nhieu tu | Co product "Ca phe sua da" | 1) Mo Product page 2) Tim nhieu tu | `ca phe da` | Tim ra dung san pham, khong phu thuoc dau/hoa-thuong |  | Not Run |
| TC_SEARCH_003 | Full-text search Member | Co member ten co dau | 1) Mo Member page 2) Tim theo ten/sdt/ma | `nguyen`, `0903...`, `#0001` | Tim duoc member theo cac truong tim kiem |  | Not Run |
| TC_SEARCH_004 | Full-text search Transaction | Co transaction + line item | 1) Mo Transaction page 2) Tim theo ma/ten khach/item | `TXN`, ten khach, ten mon | Tra ve danh sach transaction dung dieu kien |  | Not Run |
| TC_SETTINGS_001 | Settings load profile tu backend | Da dang nhap | 1) Mo Settings 2) Quan sat thong tin shop | Shop profile da luu tren backend | ShopName/Address/Email/Phone duoc do len tu backend |  | Not Run |
| TC_SETTINGS_002 | Settings save profile len backend | Dang o Settings | 1) Sua thong tin shop 2) Bam Apply 3) Mo lai Settings | Ten/SDT/Dia chi moi | Luu thanh cong, mo lai van thay du lieu moi, du lieu backend da cap nhat |  | Not Run |
| TC_SETTINGS_003 | Validation email/phone Settings | Dang o Settings | 1) Nhap email/phone sai format 2) Bam Apply | Email/Phone sai | Hien loi validation, khong cho luu khi du lieu sai |  | Not Run |
| TC_LOCALE_001 | Hien thi "Nuoc uong" tieng Viet co dau | Ngon ngu vi-VN | 1) Mo cac man co ProductType Drink | vi-VN | Chuoi Drink hien thi "Nuoc uong" co dau dung quy uoc du an |  | Not Run |

## Ghi chu thuc thi

- Nen chay seed truoc: `cd API && yarn seed:realistic`.
- Neu test WinUI build local: dung cau hinh `x64` de tranh loi AnyCPU packaging.
- Moi test case sau khi chay can dien cot **Thuc te** va **Trang thai** de nop bao cao.

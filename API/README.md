# API

Express + Apollo GraphQL on one port (default `4000`).

## Image upload (Cloudinary)

`POST /upload/image` with multipart field `file`. Response: `{ "url": "https://..." }`.

Optional environment variables (when unset, upload returns `503`):

- `CLOUDINARY_CLOUD_NAME`
- `CLOUDINARY_API_KEY`
- `CLOUDINARY_API_SECRET`
- `CLOUDINARY_UPLOAD_FOLDER` (default `playpointpos`)

The WinUI client uploads picked images to this endpoint and stores the returned URL in product/game `imageUri`.

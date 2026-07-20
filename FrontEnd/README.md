# AgriVision Frontend

Angular frontend for AgriVision.

## Run

```bash
npm install
npm start
```

Then open:

```text
http://127.0.0.1:4200
```

## Build

```bash
npm run build
```

## Notes

- Static assets live under `public/assets`.
- Pages live under `src/app/pages`.
- Shared dashboard layout lives under `src/app/layout`.
- Clean Angular routes are used, for example `/auth/login` and `/dashboard/analytics`.
- Old HTML-style URLs redirect to the new Angular routes for compatibility.

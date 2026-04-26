# NotesCool Frontend (CMS)

This is the frontend part of the NotesCool project, built with:
- **React 19**
- **TypeScript**
- **Vite**
- **Tailwind CSS 4**

## Folder Structure
- `src/components`: Reusable UI components
- `src/pages`: Page-level components (Login, Register, Dashboard, etc.)
- `src/hooks`: Custom React hooks (e.g., `useAuth`)
- `src/services`: API services using Axios
- `src/store`: State management via Context API/Providers
- `src/utils`: Helper functions (e.g., `cn` for Tailwind class merging)
- `src/constants`: Environment variables and configuration

## Development

### Prerequisites
- Node.js (v18+)
- npm or yarn

### Installation
```bash
cd src/frontend
npm install
```

### Environment Setup
Create a `.env` file from the example:
```bash
cp .env.example .env
```
Adjust `VITE_API_BASE_URL` to point to your local backend API.

### Available Scripts
- `npm run dev`: Start local development server with HMR
- `npm run build`: Build production-ready assets
- `npm run lint`: Run ESLint to check code quality
- `npm run test`: Run unit tests with Vitest

## Technical Decisions
- **Tailwind v4**: Using the latest CSS-first configuration.
- **React 19**: Utilizing new features like Actions and improved hooks.
- **Axios**: Centralized API layer in `src/services/api.ts`.
- **Context API**: Authentication state managed in `src/store/AuthContext.tsx`.

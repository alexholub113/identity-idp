# Identity Provider Web Frontend

This is the frontend application for the Identity Provider login page built with:

- **Vite** - Fast build tool and dev server
- **TypeScript** - Type-safe JavaScript
- **Tailwind CSS** - Utility-first CSS framework

## Getting Started

### Prerequisites

- Node.js (version 18 or higher)
- npm or yarn

### Installation

1. Navigate to the web frontend directory:
   ```bash
   cd src/IdentityProvider.Web
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm run dev
   ```

4. Open your browser and navigate to `http://localhost:3000`

### Available Scripts

- `npm run dev` - Start the development server
- `npm run build` - Build for production
- `npm run preview` - Preview the production build
- `npm run lint` - Run ESLint

### API Integration

The login page is configured to communicate with the ASP.NET Core API running on `http://localhost:5043`. Make sure the API server is running when testing the login functionality.

### Features

- Responsive login form
- Form validation
- Loading states
- Error handling
- Remember me functionality
- Modern, accessible design with Tailwind CSS

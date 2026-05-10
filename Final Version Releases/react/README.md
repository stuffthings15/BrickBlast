# BrickBlast — React Web Wrapper

Wraps the HTML5 game in a Create React App shell so it can be deployed to any web host.

## Quick Start

```bash
npm install
npm start        # dev server at http://localhost:3000
npm run build    # production build → build/
```

## Deployment

Upload the `build/` folder to any static host (Netlify, Vercel, GitHub Pages, S3, etc.).

The game runs inside an `<iframe>` that points to `public/game/index.html`.

## Structure

```
react/
  public/
    index.html       React HTML entry
    game/            Full HTML5 game (copied from web/)
  src/
    index.js         React bootstrap
    App.jsx          Full-screen iframe wrapper
  package.json
```

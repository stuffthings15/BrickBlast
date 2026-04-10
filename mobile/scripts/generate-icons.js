// generate-icons.js - Generates simple app icons as SVG-based PNGs
// Run: node scripts/generate-icons.js
const fs = require('fs');
const path = require('path');

const iconsDir = path.resolve(__dirname, '..', 'www', 'icons');
if (!fs.existsSync(iconsDir)) fs.mkdirSync(iconsDir, { recursive: true });

const sizes = [72, 96, 128, 144, 152, 192, 384, 512];

// Generate an SVG icon and save as SVG (browsers/Capacitor can use SVG)
function generateIconSVG(size) {
    const pad = size * 0.08;
    const brickH = size * 0.07;
    const brickW = size * 0.18;
    const gap = size * 0.02;
    const startY = size * 0.22;
    const startX = size * 0.12;
    const colors = ['#ff3c50','#ff8c32','#ffdc32','#32dc64','#32b4ff','#8250ff','#ff50c8'];
    
    let bricks = '';
    for (let r = 0; r < 5; r++) {
        for (let c = 0; c < 4; c++) {
            const x = startX + c * (brickW + gap);
            const y = startY + r * (brickH + gap);
            const col = colors[r % colors.length];
            bricks += `<rect x="${x}" y="${y}" width="${brickW}" height="${brickH}" rx="${size*0.01}" fill="${col}" opacity="0.95"/>`;
        }
    }
    
    // Ball
    const ballR = size * 0.04;
    const ballX = size * 0.55;
    const ballY = size * 0.72;
    
    // Paddle
    const padW = size * 0.35;
    const padH = size * 0.04;
    const padX = (size - padW) / 2;
    const padY = size * 0.82;

    return `<svg xmlns="http://www.w3.org/2000/svg" width="${size}" height="${size}" viewBox="0 0 ${size} ${size}">
  <defs>
    <linearGradient id="bg" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="#000028"/>
      <stop offset="100%" stop-color="#000014"/>
    </linearGradient>
    <linearGradient id="title" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0%" stop-color="#ffc850"/>
      <stop offset="100%" stop-color="#ff6432"/>
    </linearGradient>
    <linearGradient id="paddle" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0%" stop-color="#50b4ff"/>
      <stop offset="100%" stop-color="#2864c8"/>
    </linearGradient>
  </defs>
  <rect width="${size}" height="${size}" rx="${size*0.15}" fill="url(#bg)"/>
  <text x="${size/2}" y="${size*0.16}" text-anchor="middle" font-family="Arial,sans-serif" font-weight="bold" font-size="${size*0.1}" fill="url(#title)">BB</text>
  ${bricks}
  <circle cx="${ballX}" cy="${ballY}" r="${ballR}" fill="white"/>
  <circle cx="${ballX-ballR*0.3}" cy="${ballY-ballR*0.3}" r="${ballR*0.3}" fill="rgba(255,255,255,0.6)"/>
  <rect x="${padX}" y="${padY}" width="${padW}" height="${padH}" rx="${padH/2}" fill="url(#paddle)"/>
</svg>`;
}

for (const size of sizes) {
    const svg = generateIconSVG(size);
    const filename = `icon-${size}.svg`;
    fs.writeFileSync(path.join(iconsDir, filename), svg, 'utf8');
    // Also save as the PNG filename (Capacitor will use these)
    // For actual PNG conversion, use Android Studio / Xcode asset catalogs
    fs.writeFileSync(path.join(iconsDir, `icon-${size}.png`), svg, 'utf8');
    console.log(`Generated: ${filename} (${size}x${size})`);
}

// Generate Android adaptive icon resources
const androidResDir = path.resolve(__dirname, '..', 'resources', 'android');
fs.writeFileSync(path.join(androidResDir, 'icon-foreground.svg'), generateIconSVG(512), 'utf8');
fs.writeFileSync(path.join(androidResDir, 'icon-background.svg'), 
    `<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512"><rect width="512" height="512" fill="#000014"/></svg>`, 'utf8');

// Generate splash screen SVG
const splashSVG = `<svg xmlns="http://www.w3.org/2000/svg" width="2732" height="2732" viewBox="0 0 2732 2732">
  <rect width="2732" height="2732" fill="#000014"/>
  <text x="1366" y="1200" text-anchor="middle" font-family="Arial,sans-serif" font-weight="bold" font-size="180" fill="#ffc850">BRICK BLAST</text>
  <text x="1366" y="1400" text-anchor="middle" font-family="Arial,sans-serif" font-size="80" fill="#64c8ff">Team Fast Talk</text>
  <text x="1366" y="1550" text-anchor="middle" font-family="Arial,sans-serif" font-size="50" fill="#9696aa">Loading...</text>
</svg>`;
fs.writeFileSync(path.join(androidResDir, 'splash.svg'), splashSVG, 'utf8');
const iosResDir = path.resolve(__dirname, '..', 'resources', 'ios');
if (!fs.existsSync(iosResDir)) fs.mkdirSync(iosResDir, { recursive: true });
fs.writeFileSync(path.join(iosResDir, 'splash.svg'), splashSVG, 'utf8');

console.log('\nIcon generation complete!');
console.log('NOTE: For production, convert SVGs to PNGs using:');
console.log('  Android: Use Android Studio Image Asset Studio');
console.log('  iOS: Use Xcode Asset Catalog or @capacitor/assets');

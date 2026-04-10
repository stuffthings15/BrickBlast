// copy-web.js - Copies web game files into the www directory for Capacitor
const fs = require('fs');
const path = require('path');

const srcDir = path.resolve(__dirname, '..', '..', 'web');
const destDir = path.resolve(__dirname, '..', 'www');

// Ensure www directory exists
if (!fs.existsSync(destDir)) {
    fs.mkdirSync(destDir, { recursive: true });
}

// Copy all files from web/ to www/
const files = fs.readdirSync(srcDir);
for (const file of files) {
    const srcPath = path.join(srcDir, file);
    const destPath = path.join(destDir, file);
    if (fs.statSync(srcPath).isFile()) {
        fs.copyFileSync(srcPath, destPath);
        console.log(`Copied: ${file}`);
    }
}

// Read the index.html and inject mobile-specific initialization
let html = fs.readFileSync(path.join(destDir, 'index.html'), 'utf8');

// Add Capacitor runtime and mobile initialization before closing </script>
const mobileInit = `
// --- Capacitor Mobile Integration ---
(async function initMobile(){
 try {
  if(window.Capacitor){
   const{SplashScreen}=await import('@capacitor/splash-screen');
   const{StatusBar,Style}=await import('@capacitor/status-bar');
   const{ScreenOrientation}=await import('@capacitor/screen-orientation');
   const{App}=await import('@capacitor/app');
   // Hide status bar for fullscreen gaming
   try{await StatusBar.setStyle({style:Style.Dark})}catch(e){}
   try{await StatusBar.setBackgroundColor({color:'#000014'})}catch(e){}
   try{await StatusBar.hide()}catch(e){}
   // Lock to landscape
   try{await ScreenOrientation.lock({orientation:'landscape'})}catch(e){}
   // Hide splash after game loads
   try{await SplashScreen.hide()}catch(e){}
   // Handle back button on Android
   App.addListener('backButton',({canGoBack})=>{
    if(state===States.Playing){state=States.Paused}
    else if(state===States.Paused){state=States.Playing}
    else if(state===States.Options){state=prevState}
    else if(state===States.Menu){App.exitApp()}
   });
   console.log('Capacitor mobile init complete');
  }
 }catch(e){console.log('Not running in Capacitor:',e)}
})();
`;

// Inject before the closing </script> tag
html = html.replace('</script>', mobileInit + '\n</script>');

// Update the manifest link for mobile
html = html.replace(
    '<link rel="manifest" href="manifest.json">',
    '<link rel="manifest" href="manifest.json">\n<link rel="apple-touch-icon" href="icons/icon-192.png">'
);

// Add viewport-fit=cover for iPhone notch support
html = html.replace(
    'width=device-width,initial-scale=1,maximum-scale=1,user-scalable=no',
    'width=device-width,initial-scale=1,maximum-scale=1,user-scalable=no,viewport-fit=cover'
);

fs.writeFileSync(path.join(destDir, 'index.html'), html, 'utf8');
console.log('Mobile index.html written with Capacitor integration');

// Update manifest for mobile
const manifest = {
    name: "Brick Blast - Team Fast Talk",
    short_name: "Brick Blast",
    description: "2D Brick Breaker arcade game by Team Fast Talk",
    start_url: "./index.html",
    display: "fullscreen",
    orientation: "landscape",
    background_color: "#000014",
    theme_color: "#000014",
    categories: ["games", "entertainment"],
    icons: [
        { src: "icons/icon-72.png", sizes: "72x72", type: "image/png" },
        { src: "icons/icon-96.png", sizes: "96x96", type: "image/png" },
        { src: "icons/icon-128.png", sizes: "128x128", type: "image/png" },
        { src: "icons/icon-144.png", sizes: "144x144", type: "image/png" },
        { src: "icons/icon-152.png", sizes: "152x152", type: "image/png" },
        { src: "icons/icon-192.png", sizes: "192x192", type: "image/png" },
        { src: "icons/icon-384.png", sizes: "384x384", type: "image/png" },
        { src: "icons/icon-512.png", sizes: "512x512", type: "image/png" }
    ]
};
fs.writeFileSync(path.join(destDir, 'manifest.json'), JSON.stringify(manifest, null, 2), 'utf8');
console.log('Mobile manifest.json written');
console.log('\\nWeb build complete! Run "npx cap sync" to push to native projects.');

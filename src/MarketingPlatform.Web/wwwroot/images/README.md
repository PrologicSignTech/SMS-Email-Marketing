# Landing Page Image Assets

This directory contains placeholder image assets for the landing page. The landing page has SVG fallbacks built-in, so it will work even without these images, but adding them will eliminate 404 errors.

## Directory Structure

```
images/
├── logos/              # Company logos (5 files)
│   ├── company-1.svg
│   ├── company-2.svg
│   ├── company-3.svg
│   ├── company-4.svg
│   └── company-5.svg
├── badges/             # Trust & certification badges (5 files)
│   ├── gdpr.svg
│   ├── iso-27001.svg
│   ├── soc2.svg
│   ├── hipaa.svg
│   └── ssl.svg
├── use-cases/          # Industry use case images (4 files)
│   ├── ecommerce.svg
│   ├── healthcare.svg
│   ├── realestate.svg
│   └── retail.svg
├── product-demo.gif    # Fallback GIF for video
└── video-poster.jpg    # Video thumbnail
```

## Image Specifications

### Company Logos (`logos/`)
- **Format:** SVG (recommended) or PNG
- **Dimensions:** 120x40px (or proportional)
- **Style:** Monochrome or grayscale (CSS will apply grayscale filter)
- **Background:** Transparent

### Trust Badges (`badges/`)
- **Format:** SVG (recommended) or PNG
- **Dimensions:** 80x80px
- **Style:** Official certification badge designs
- **Background:** Transparent or white

### Use Case Images (`use-cases/`)
- **Format:** SVG (recommended) or PNG/JPG
- **Dimensions:** 600x400px
- **Style:** Illustrations or screenshots showing industry use cases
- **Background:** Light gray (#f3f4f6) or transparent

### Video Assets
- **product-demo.gif:** Animated GIF showing product features (fallback if video fails)
  - Dimensions: 640x360px or larger
  - Duration: 10-30 seconds
  - File size: Keep under 5MB

- **video-poster.jpg:** Static thumbnail shown before video plays
  - Dimensions: 1280x720px (16:9 aspect ratio)
  - Format: JPG or PNG
  - Quality: High quality, optimized for web

## Current Status

✅ **Directories created** - All subdirectories exist
⚠️ **SVG Fallbacks active** - Inline SVG placeholders are shown when images are missing
📝 **Action needed** - Upload actual image assets to eliminate 404 errors

## How to Add Images

1. Place your image files in the appropriate subdirectories
2. Name them exactly as shown above (case-sensitive)
3. Ensure proper dimensions and formats
4. Test the landing page to verify images load correctly

## Optional: Create Placeholder SVGs

If you want to create placeholder SVG files quickly, you can use online tools like:
- **Figma** - Create custom SVG graphics
- **Canva** - Design badges and logos
- **undraw.co** - Free SVG illustrations
- **Flaticon** - Icon SVGs (with attribution)

## Note

The landing page will function perfectly with the built-in SVG fallbacks. These show placeholder graphics with text labels. Adding real images is optional but recommended for a polished look.

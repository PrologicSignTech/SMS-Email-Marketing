# Product Demo Video Files

## Required Files

### 1. product-demo.mp4
**Location:** `/wwwroot/videos/product-demo.mp4`

**Purpose:** Main product demonstration video shown in the hero section

**Specifications:**
- Format: MP4 (H.264)
- Resolution: 1280x720px or 1920x1080px
- Duration: 30-90 seconds recommended
- File size: 5-15 MB recommended (max 25 MB)
- Content: Show key platform features and benefits

**Status:** ⚠️ **FILE MISSING** - Upload your product demo video here

---

## Fallback Files

The landing page has two fallback mechanisms if the video is missing:

### GIF Fallback
**Location:** `/wwwroot/images/product-demo.gif`
- Shown when video fails to load
- Should be 3-5 MB max
- 640x360px or larger
- 10-30 seconds duration

### Poster Image
**Location:** `/wwwroot/images/video-poster.jpg`
- Thumbnail shown before video plays
- 1280x720px (16:9 aspect ratio)
- JPG format, optimized for web

---

## Creating Demo Video

### Recommended Tools
- **OBS Studio** (Free) - Screen recording
- **Loom** - Quick screen recording
- **Camtasia** - Recording + editing
- **ScreenFlow** (Mac) - Professional screen recording

### Content Recommendations
1. Dashboard overview (5-10s)
2. Campaign creation flow (10-15s)
3. Analytics view (10-15s)
4. Multi-channel features demo (10-15s)
5. Success metrics (5-10s)

### Compression Example (FFmpeg)
```bash
# Compress video to web-optimized MP4
ffmpeg -i input.mp4 -vcodec h264 -acodec aac -b:v 2M -b:a 128k -s 1280x720 product-demo.mp4

# Create poster image from video
ffmpeg -i product-demo.mp4 -ss 00:00:02 -vframes 1 ../images/video-poster.jpg
```

---

## Current Status

✅ **Video player implemented** - HTML5 video with play/pause controls
✅ **CSP-compliant** - All event handlers externalized
✅ **Fallback support** - GIF and poster image fallbacks configured
⚠️ **Video file missing** - Upload product-demo.mp4 to enable full functionality

---

## Alternative: Temporary Placeholder

Until you have a demo video ready, the landing page will gracefully handle the missing video:
- Poster image displayed (if available)
- Play button visible but video will show "file not found" on click
- Consider using a static image or animated GIF as temporary solution

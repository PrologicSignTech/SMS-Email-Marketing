using MarketingPlatform.Application.DTOs;
using MarketingPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MarketingPlatform.Web.Controllers
{
    [Authorize]
    public class PageContentController : Controller
    {
        private readonly IPageContentService _pageContentService;
        private readonly ILogger<PageContentController> _logger;

        public PageContentController(
            IPageContentService pageContentService,
            ILogger<PageContentController> logger)
        {
            _pageContentService = pageContentService;
            _logger = logger;
        }

        // GET: PageContent/Index
        public async Task<IActionResult> Index()
        {
            var contents = await _pageContentService.GetAllAsync();
            return View(contents);
        }

        // GET: PageContent/Edit/privacy-policy
        public async Task<IActionResult> Edit(string pageKey)
        {
            if (string.IsNullOrEmpty(pageKey))
            {
                return BadRequest();
            }

            var content = await _pageContentService.GetByPageKeyAsync(pageKey);
            
            // If content doesn't exist, create a new one with defaults
            if (content == null)
            {
                content = new PageContentDto
                {
                    PageKey = pageKey,
                    Title = pageKey == "privacy-policy" ? "Privacy Policy" : "Terms of Service",
                    Content = "",
                    IsPublished = false
                };
            }

            return View(content);
        }

        // POST: PageContent/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PageContentDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await _pageContentService.SavePageContentAsync(model, userId);
                
                TempData["SuccessMessage"] = "Page content updated successfully.";
                return RedirectToAction(nameof(Edit), new { pageKey = model.PageKey });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating page content for {PageKey}", model.PageKey);
                ModelState.AddModelError("", "An error occurred while updating the page content.");
                return View(model);
            }
        }

        // POST: PageContent/UploadImage
        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file, string pageKey)
        {
            if (file == null || string.IsNullOrEmpty(pageKey))
            {
                return BadRequest(new { success = false, message = "Invalid file or page key" });
            }

            try
            {
                var imageUrl = await _pageContentService.UploadImageAsync(file, pageKey);
                return Ok(new { success = true, imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image for page {PageKey}", pageKey);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // POST: PageContent/DeleteImage
        [HttpPost]
        public async Task<IActionResult> DeleteImage(string imageUrl, string pageKey)
        {
            if (string.IsNullOrEmpty(imageUrl) || string.IsNullOrEmpty(pageKey))
            {
                return BadRequest(new { success = false, message = "Invalid image URL or page key" });
            }

            try
            {
                var result = await _pageContentService.DeleteImageAsync(imageUrl, pageKey);
                return Ok(new { success = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting image {ImageUrl} for page {PageKey}", imageUrl, pageKey);
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}

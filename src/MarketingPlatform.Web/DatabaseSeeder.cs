using MarketingPlatform.Infrastructure.Data;
using MarketingPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Web
{
    public static class DatabaseSeeder
    {
        public static async Task SeedPageContentAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

            try
            {
                // Check if page content already exists
                var existingPrivacy = await context.PageContents
                    .FirstOrDefaultAsync(p => p.PageKey == "privacy-policy");
                
                var existingTerms = await context.PageContents
                    .FirstOrDefaultAsync(p => p.PageKey == "terms-of-service");

                // Seed Privacy Policy if it doesn't exist
                if (existingPrivacy == null)
                {
                    logger?.LogInformation("Creating Privacy Policy page content...");

                    var privacy = new PageContent
                {
                    PageKey = "privacy-policy",
                    Title = "Privacy Policy",
                    MetaDescription = "Learn how we collect, use, and protect your personal information.",
                    Content = @"
<h2>1. Information We Collect</h2>
<p>We collect information that you provide directly to us, including:</p>
<ul>
    <li>Name and contact information (email address, phone number)</li>
    <li>Account credentials</li>
    <li>Payment information</li>
    <li>Communication preferences</li>
    <li>Campaign and marketing data</li>
</ul>

<h2>2. How We Use Your Information</h2>
<p>We use the information we collect to:</p>
<ul>
    <li>Provide, maintain, and improve our services</li>
    <li>Process transactions and send related information</li>
    <li>Send technical notices, updates, security alerts, and support messages</li>
    <li>Respond to your comments, questions, and customer service requests</li>
    <li>Monitor and analyze trends, usage, and activities</li>
</ul>

<h2>3. Data Security</h2>
<p>We implement appropriate technical and organizational measures to protect your personal data against unauthorized or unlawful processing, accidental loss, destruction, or damage. This includes encryption of sensitive data, regular security assessments, and access controls.</p>

<h2>4. Data Retention</h2>
<p>We retain your personal data for as long as necessary to provide our services, comply with legal obligations, resolve disputes, and enforce our agreements.</p>

<h2>5. Your Rights</h2>
<p>You have the right to:</p>
<ul>
    <li>Access your personal data</li>
    <li>Correct inaccurate data</li>
    <li>Request deletion of your data</li>
    <li>Object to processing of your data</li>
    <li>Request data portability</li>
    <li>Withdraw consent at any time</li>
</ul>

<h2>6. Contact Us</h2>
<p>If you have any questions about this Privacy Policy or our data practices, please contact us at privacy@marketingplatform.com</p>

<p><em>Last updated: January 2024</em></p>
",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                    };

                    context.PageContents.Add(privacy);
                    logger?.LogInformation("Privacy Policy page content created.");
                }
                else
                {
                    logger?.LogInformation("Privacy Policy already exists.");
                }

                // Seed Terms of Service if it doesn't exist
                if (existingTerms == null)
                {
                    logger?.LogInformation("Creating Terms of Service page content...");

                    var terms = new PageContent
                {
                    PageKey = "terms-of-service",
                    Title = "Terms of Service",
                    MetaDescription = "Read our terms of service and user agreement.",
                    Content = @"
<h2>1. Acceptance of Terms</h2>
<p>By accessing and using Marketing Platform (""the Service""), you accept and agree to be bound by the terms and provisions of this agreement. If you do not agree to these terms, please do not use the Service.</p>

<h2>2. Use License</h2>
<p>Permission is granted to access and use the Service for legitimate business purposes. This license shall automatically terminate if you violate any of these restrictions.</p>

<h2>3. Account Terms</h2>
<p>When you create an account with us, you must provide accurate and complete information. You are responsible for:</p>
<ul>
    <li>Maintaining the security of your account and password</li>
    <li>All activities that occur under your account</li>
    <li>Immediately notifying us of any unauthorized use</li>
    <li>Ensuring your use complies with all applicable laws</li>
</ul>

<h2>4. Service Availability</h2>
<p>We strive to provide a reliable service, but we do not guarantee that:</p>
<ul>
    <li>The Service will be uninterrupted, timely, secure, or error-free</li>
    <li>Any errors or defects will be corrected</li>
    <li>The Service is free of viruses or other harmful components</li>
</ul>

<h2>5. Prohibited Uses</h2>
<p>You may not use our Service:</p>
<ul>
    <li>For any unlawful purpose or to violate any laws</li>
    <li>To send spam, unsolicited messages, or illegal content</li>
    <li>To transmit malware or other harmful code</li>
    <li>To interfere with or disrupt the Service or servers</li>
    <li>To impersonate any person or entity</li>
</ul>

<h2>6. Intellectual Property</h2>
<p>The Service and its original content, features, and functionality are owned by Marketing Platform and are protected by international copyright, trademark, and other intellectual property laws.</p>

<h2>7. Termination</h2>
<p>We may terminate or suspend your account and access to the Service immediately, without prior notice or liability, for any reason, including if you breach these Terms.</p>

<h2>8. Limitation of Liability</h2>
<p>In no event shall Marketing Platform be liable for any indirect, incidental, special, consequential, or punitive damages resulting from your use of or inability to use the Service.</p>

<h2>9. Changes to Terms</h2>
<p>We reserve the right to modify or replace these Terms at any time. If a revision is material, we will provide at least 30 days' notice prior to any new terms taking effect.</p>

<h2>10. Contact Information</h2>
<p>If you have any questions about these Terms, please contact us at legal@marketingplatform.com</p>

<p><em>Last updated: January 2024</em></p>
",
                    IsPublished = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                    };

                    context.PageContents.Add(terms);
                    logger?.LogInformation("Terms of Service page content created.");
                }
                else
                {
                    logger?.LogInformation("Terms of Service already exists.");
                }

                // Save changes
                await context.SaveChangesAsync();
                logger?.LogInformation("Page content seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error occurred while seeding page content.");
                throw;
            }
        }
    }
}

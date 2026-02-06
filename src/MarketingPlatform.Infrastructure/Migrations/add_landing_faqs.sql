-- Create LandingFaqs table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LandingFaqs]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[LandingFaqs](
        [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [Question] NVARCHAR(500) NOT NULL,
        [Answer] NVARCHAR(MAX) NOT NULL,
        [IconClass] NVARCHAR(100) NOT NULL,
        [IconColor] NVARCHAR(50) NOT NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [ShowOnLanding] BIT NOT NULL DEFAULT 1,
        [Category] NVARCHAR(100) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        [IsDeleted] BIT NOT NULL DEFAULT 0
    );
    PRINT 'LandingFaqs table created successfully!';
END
ELSE
BEGIN
    PRINT 'LandingFaqs table already exists.';
END
GO

-- Seed landing FAQs
IF NOT EXISTS (SELECT * FROM LandingFaqs WHERE Question = 'What''s included in the free trial?')
BEGIN
    INSERT INTO LandingFaqs
    (Question, Answer, IconClass, IconColor, DisplayOrder, IsActive, ShowOnLanding, CreatedAt)
    VALUES
    (
        'What''s included in the free trial?',
        '<p>Our 14-day free trial gives you full access to all features of the Professional plan with no credit card required. You can:</p><ul><li>Send up to 1,000 messages (SMS, MMS, and Email)</li><li>Create unlimited campaigns and templates</li><li>Access advanced analytics and A/B testing</li><li>Test all automation features</li><li>Get full support from our team</li></ul><p class="mb-0">No charges apply until you choose a paid plan after the trial period.</p>',
        'bi-gift',
        'primary',
        1,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'What channels does the platform support?',
        '<p>Our platform supports multiple communication channels to reach your audience wherever they are:</p><ul><li><strong>SMS (Text Messages):</strong> Instant delivery with 98% open rates</li><li><strong>MMS (Multimedia Messages):</strong> Send images, videos, and rich media</li><li><strong>Email Marketing:</strong> Professional email campaigns with templates</li><li><strong>Multi-Channel Campaigns:</strong> Combine channels for maximum impact</li></ul><p class="mb-0">All channels can be managed from a single dashboard with unified analytics.</p>',
        'bi-lightning',
        'warning',
        2,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'How does pricing work? Are there any hidden fees?',
        '<p><strong>No hidden fees. Ever.</strong> Our pricing is transparent and straightforward:</p><ul><li>Monthly subscription based on your plan tier (Starter, Professional, Enterprise)</li><li>Message credits included with each plan</li><li>Additional messages available at predictable per-message rates</li><li>No setup fees, no cancellation fees</li><li>Pay-as-you-go option available for seasonal businesses</li></ul><p class="mb-0">You can upgrade, downgrade, or cancel anytime. Billing is monthly or annual (save 20% with annual billing).</p>',
        'bi-credit-card',
        'success',
        3,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Is my data secure and compliant?',
        '<p><strong>Absolutely. Security and compliance are our top priorities.</strong></p><ul><li><strong>GDPR Compliant:</strong> Full compliance with European data protection regulations</li><li><strong>HIPAA Ready:</strong> For healthcare organizations handling patient data</li><li><strong>SOC 2 Type II Certified:</strong> Industry-standard security audits</li><li><strong>256-bit SSL Encryption:</strong> All data encrypted in transit and at rest</li><li><strong>ISO 27001 Certified:</strong> Information security management</li></ul><p class="mb-0">We also provide built-in consent management, opt-out handling, and audit trails for compliance.</p>',
        'bi-shield-check',
        'info',
        4,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'Can I integrate with my existing tools?',
        '<p><strong>Yes! We integrate with popular platforms you already use:</strong></p><ul><li><strong>CRM Systems:</strong> Salesforce, HubSpot, Zoho CRM</li><li><strong>E-commerce:</strong> Shopify, WooCommerce, Magento</li><li><strong>Automation:</strong> Zapier, Make (Integromat), native webhooks</li><li><strong>Analytics:</strong> Google Analytics, Mixpanel, Segment</li><li><strong>Custom Integration:</strong> RESTful API and webhooks for custom solutions</li></ul><p class="mb-0">Our API documentation is comprehensive, and our support team can help with integration questions.</p>',
        'bi-puzzle',
        'danger',
        5,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'What kind of support do you offer?',
        '<p><strong>We''re here to help 24/7:</strong></p><ul><li><strong>Live Chat Support:</strong> Available 24/7 for all paid plans</li><li><strong>Email Support:</strong> Response within 2 hours during business hours</li><li><strong>Phone Support:</strong> Available for Professional and Enterprise plans</li><li><strong>Knowledge Base:</strong> Comprehensive guides and tutorials</li><li><strong>Video Training:</strong> On-demand webinars and training sessions</li><li><strong>Dedicated Account Manager:</strong> For Enterprise customers</li></ul><p class="mb-0">Our average response time is under 5 minutes during peak hours, and we maintain a 98% customer satisfaction rating.</p>',
        'bi-headset',
        'primary',
        6,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'How easy is it to migrate from another platform?',
        '<p><strong>We make migration simple and seamless:</strong></p><ul><li><strong>Free Migration Assistance:</strong> Our team helps you import contacts, templates, and campaign history</li><li><strong>CSV Import:</strong> Bulk import contacts from any platform</li><li><strong>API Migration:</strong> Direct migration from major platforms (Mailchimp, Twilio, etc.)</li><li><strong>Template Recreation:</strong> We''ll help recreate your existing templates</li><li><strong>Zero Downtime:</strong> Run campaigns on both platforms during transition</li></ul><p class="mb-0">Most customers complete migration in 1-3 days. Enterprise customers get a dedicated migration specialist.</p>',
        'bi-arrow-left-right',
        'success',
        7,
        1,
        1,
        GETUTCDATE()
    ),
    (
        'How can I track campaign performance?',
        '<p><strong>Comprehensive analytics built into every campaign:</strong></p><ul><li><strong>Real-Time Metrics:</strong> Delivery rates, open rates, click rates, conversions</li><li><strong>A/B Testing:</strong> Test subject lines, content, and send times automatically</li><li><strong>Engagement Tracking:</strong> See which links are clicked and when</li><li><strong>ROI Calculator:</strong> Track revenue generated per campaign</li><li><strong>Custom Reports:</strong> Export data in CSV, PDF, or via API</li><li><strong>Heatmaps:</strong> Visual representation of email engagement</li></ul><p class="mb-0">All analytics are available in real-time with historical data stored indefinitely.</p>',
        'bi-graph-up',
        'info',
        8,
        1,
        1,
        GETUTCDATE()
    );

    PRINT 'Landing FAQs seeded successfully!';
END
ELSE
BEGIN
    PRINT 'Landing FAQs already exist.';
END
GO

PRINT 'LandingFaqs table setup completed!';

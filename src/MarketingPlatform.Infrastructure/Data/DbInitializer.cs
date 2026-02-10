using MarketingPlatform.Core.Entities;
using MarketingPlatform.Core.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketingPlatform.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger? logger = null)
        {
            logger?.LogInformation("Starting database seeding process...");

            try
            {
                await SeedIdentityRolesAsync(roleManager, logger);
                await SeedCustomRolesAsync(context, logger);
                await SeedUsersAsync(context, userManager, roleManager, logger);
                await SeedFeaturesAsync(context, logger);
                await SeedSubscriptionPlansAsync(context, logger);
                await SeedPlanFeatureMappingsAsync(context, logger);
                await SeedMessageProvidersAsync(context, logger);
                await SeedChannelRoutingConfigsAsync(context, logger);
                await SeedPricingModelsAsync(context, logger);
                await SeedLandingPageSettingsAsync(context, logger);
                await SeedPageContentAsync(context, logger);
                await SeedLandingFeaturesAsync(context, logger);
                await SeedLandingFaqsAsync(context, logger);
                await SeedTestimonialsAsync(context, logger);
                await SeedTrustedCompaniesAsync(context, logger);
                await SeedSecurityBadgesAsync(context, logger);
                await SeedUseCasesAsync(context, logger);
                await SeedLandingStatsAsync(context, logger);
                await SeedFooterSettingsAsync(context, logger);
                await SeedComplianceRulesAsync(context, logger);
                await SeedFeatureTogglesAsync(context, logger);
                await SeedPlatformConfigurationsAsync(context, logger);
                await SeedFileStorageSettingsAsync(context, logger);
                await SeedTaxConfigurationsAsync(context, logger);

                logger?.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "An error occurred during database seeding.");
                throw;
            }
        }

        private static async Task SeedIdentityRolesAsync(RoleManager<IdentityRole> roleManager, ILogger? logger)
        {
            logger?.LogInformation("Seeding Identity roles...");

            try
            {
                // Seed Identity Roles
                string[] identityRoles = { "Admin", "User", "Manager", "SuperAdmin", "Analyst", "Viewer" };
                foreach (var role in identityRoles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(role));
                        if (result.Succeeded)
                        {
                            logger?.LogInformation("Identity role '{Role}' created successfully.", role);
                        }
                        else
                        {
                            logger?.LogWarning("Failed to create Identity role '{Role}': {Errors}", 
                                role, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger?.LogInformation("Identity role '{Role}' already exists.", role);
                    }
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding Identity roles.");
                throw;
            }
        }

        private static async Task SeedCustomRolesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding custom roles...");

            try
            {
                // Seed Custom Roles with Permissions
                if (!await context.CustomRoles.AnyAsync())
                {
                    logger?.LogInformation("Creating custom roles...");

                    var customRoles = new List<Role>
                {
                    new Role
                    {
                        Name = "SuperAdmin",
                        Description = "Full system access with all permissions",
                        Permissions = (long)Permission.All,
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Admin",
                        Description = "Administrator with most permissions including user/role management",
                        Permissions = (long)(Permission.ViewCampaigns | Permission.CreateCampaigns | 
                            Permission.EditCampaigns | Permission.DeleteCampaigns |
                            Permission.ViewContacts | Permission.CreateContacts | 
                            Permission.EditContacts | Permission.DeleteContacts |
                            Permission.ViewTemplates | Permission.CreateTemplates | 
                            Permission.EditTemplates | Permission.DeleteTemplates |
                            Permission.ViewAnalytics | Permission.ViewDetailedAnalytics | 
                            Permission.ExportAnalytics |
                            Permission.ViewWorkflows | Permission.CreateWorkflows | 
                            Permission.EditWorkflows | Permission.DeleteWorkflows |
                            Permission.ViewSettings | Permission.ManageSettings |
                            Permission.ViewCompliance | Permission.ManageCompliance |
                            Permission.ViewAuditLogs |
                            Permission.ViewUsers | Permission.CreateUsers |
                            Permission.EditUsers | Permission.ViewRoles |
                            Permission.ViewMessages | Permission.CreateMessages |
                            Permission.ManageProviders | Permission.ViewBilling |
                            Permission.ViewProfile),
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Manager",
                        Description = "Campaign and contact management with analytics access",
                        Permissions = (long)(Permission.ViewCampaigns | Permission.CreateCampaigns | 
                            Permission.EditCampaigns |
                            Permission.ViewContacts | Permission.CreateContacts | 
                            Permission.EditContacts |
                            Permission.ViewTemplates | Permission.CreateTemplates | 
                            Permission.EditTemplates |
                            Permission.ViewAnalytics | Permission.ViewDetailedAnalytics |
                            Permission.ViewWorkflows | Permission.CreateWorkflows |
                            Permission.EditWorkflows |
                            Permission.ViewCompliance |
                            Permission.ViewUsers |
                            Permission.ViewMessages | Permission.CreateMessages |
                            Permission.ViewProfile),
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "User",
                        Description = "Standard user with basic campaign and contact management",
                        Permissions = (long)(Permission.ViewCampaigns | Permission.CreateCampaigns |
                            Permission.ViewContacts | Permission.CreateContacts |
                            Permission.ViewTemplates | Permission.ViewAnalytics |
                            Permission.ViewMessages | Permission.ViewProfile),
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Analyst",
                        Description = "Read access with detailed analytics capabilities",
                        Permissions = (long)(Permission.ViewCampaigns | Permission.ViewContacts | 
                            Permission.ViewTemplates | Permission.ViewAnalytics | 
                            Permission.ViewDetailedAnalytics | Permission.ExportAnalytics |
                            Permission.ViewWorkflows | Permission.ViewCompliance),
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Role
                    {
                        Name = "Viewer",
                        Description = "Read-only access to campaigns and basic analytics",
                        Permissions = (long)(Permission.ViewCampaigns | Permission.ViewContacts | 
                            Permission.ViewTemplates | Permission.ViewAnalytics),
                        IsSystemRole = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    }
                    };

                    context.CustomRoles.AddRange(customRoles);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} custom roles.", customRoles.Count);
                }
                else
                {
                    logger?.LogInformation("Custom roles already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding custom roles.");
                throw;
            }
        }

        private static async Task SeedUsersAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogger? logger)
        {
            logger?.LogInformation("Seeding users...");

            try
            {
                // Seed Default Admin User
                var adminEmail = "admin@marketingplatform.com";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                
                if (adminUser == null)
                {
                    logger?.LogInformation("Creating default admin user...");

                    adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "Admin",
                    LastName = "User",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                    
                    if (result.Succeeded)
                    {
                        logger?.LogInformation("Admin user created successfully.");
                        
                        await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                        
                        // Assign SuperAdmin custom role
                        var superAdminRole = await context.CustomRoles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
                        if (superAdminRole != null)
                        {
                            var userRole = new Core.Entities.UserRole
                            {
                                UserId = adminUser.Id,
                                RoleId = superAdminRole.Id,
                                AssignedAt = DateTime.UtcNow,
                                AssignedBy = "System"
                            };
                            context.CustomUserRoles.Add(userRole);
                            await context.SaveChangesAsync();
                            logger?.LogInformation("Admin user assigned to SuperAdmin role.");
                        }
                    }
                    else
                    {
                        logger?.LogWarning("Failed to create admin user: {Errors}", 
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
                else
                {
                    logger?.LogInformation("Admin user already exists.");
                }

                // Seed Additional Test Users
                var testUsers = new List<(string email, string password, string firstName, string lastName, string role)>
                {
                    ("manager@marketingplatform.com", "Manager@123456", "John", "Manager", "Manager"),
                    ("user@marketingplatform.com", "User@123456", "Jane", "User", "User"),
                    ("analyst@marketingplatform.com", "Analyst@123456", "Bob", "Analyst", "Analyst"),
                    ("viewer@marketingplatform.com", "Viewer@123456", "Alice", "Viewer", "Viewer")
                };

                logger?.LogInformation("Creating {Count} test users...", testUsers.Count);

                foreach (var (email, password, firstName, lastName, role) in testUsers)
                {
                    var existingUser = await userManager.FindByEmailAsync(email);
                    if (existingUser == null)
                    {
                        var newUser = new ApplicationUser
                        {
                            UserName = email,
                            Email = email,
                            EmailConfirmed = true,
                            FirstName = firstName,
                            LastName = lastName,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var result = await userManager.CreateAsync(newUser, password);
                        
                        if (result.Succeeded)
                        {
                            logger?.LogInformation("Test user '{Email}' created successfully.", email);
                            
                            await userManager.AddToRoleAsync(newUser, role);
                            
                            // Assign custom role
                            var customRole = await context.CustomRoles.FirstOrDefaultAsync(r => r.Name == role);
                            if (customRole != null)
                            {
                                var userRole = new Core.Entities.UserRole
                                {
                                    UserId = newUser.Id,
                                    RoleId = customRole.Id,
                                    AssignedAt = DateTime.UtcNow,
                                    AssignedBy = "System"
                                };
                                context.CustomUserRoles.Add(userRole);
                            }
                        }
                        else
                        {
                            logger?.LogWarning("Failed to create test user '{Email}': {Errors}", 
                                email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        }
                    }
                    else
                    {
                        logger?.LogInformation("Test user '{Email}' already exists.", email);
                    }
                }
                
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding users.");
                throw;
            }
        }

        private static async Task SeedFeaturesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding features...");

            try
            {
                // Seed Features
                if (!await context.Features.AnyAsync())
                {
                    logger?.LogInformation("Creating features...");

                    var features = new List<Feature>
                    {
                        new Feature
                        {
                            Name = "SMS Messages",
                            Description = "Send SMS text messages to your contacts",
                            IsActive = true,
                            DisplayOrder = 1,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "MMS Messages",
                            Description = "Send multimedia messages with images and videos",
                            IsActive = true,
                            DisplayOrder = 2,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Email Campaigns",
                            Description = "Create and send email marketing campaigns",
                            IsActive = true,
                            DisplayOrder = 3,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Contact Management",
                            Description = "Organize and manage your contact database",
                            IsActive = true,
                            DisplayOrder = 4,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Basic Analytics",
                            Description = "View basic campaign performance metrics",
                            IsActive = true,
                            DisplayOrder = 5,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Advanced Analytics",
                            Description = "Detailed insights and custom reports",
                            IsActive = true,
                            DisplayOrder = 6,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Automation Workflows",
                            Description = "Automate your marketing campaigns",
                            IsActive = true,
                            DisplayOrder = 7,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Custom Templates",
                            Description = "Create reusable message templates",
                            IsActive = true,
                            DisplayOrder = 8,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "API Access",
                            Description = "Programmatic access to platform features",
                            IsActive = true,
                            DisplayOrder = 9,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Priority Support",
                            Description = "Fast-tracked customer support",
                            IsActive = true,
                            DisplayOrder = 10,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "24/7 Support",
                            Description = "Round-the-clock customer support",
                            IsActive = true,
                            DisplayOrder = 11,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Dedicated Account Manager",
                            Description = "Personal account management and guidance",
                            IsActive = true,
                            DisplayOrder = 12,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "White-label Options",
                            Description = "Customize the platform with your branding",
                            IsActive = true,
                            DisplayOrder = 13,
                            CreatedAt = DateTime.UtcNow
                        },
                        new Feature
                        {
                            Name = "Team Collaboration",
                            Description = "Multiple user accounts and permissions",
                            IsActive = true,
                            DisplayOrder = 14,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.Features.AddRange(features);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} features.", features.Count);
                }
                else
                {
                    logger?.LogInformation("Features already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding features.");
                throw;
            }
        }

        private static async Task SeedSubscriptionPlansAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding subscription plans...");

            try
            {
                // Seed Subscription Plans
                if (!await context.SubscriptionPlans.AnyAsync())
                {
                    logger?.LogInformation("Creating subscription plans...");

                    var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan
                    {
                        Name = "Starter",
                        Description = "Perfect for small businesses getting started with marketing automation",
                        PlanCategory = "For small businesses",
                        IsMostPopular = false,
                        PriceMonthly = 29.99m,
                        PriceYearly = 299.99m, // ~17% discount
                        SMSLimit = 1000,
                        MMSLimit = 100,
                        EmailLimit = 5000,
                        ContactLimit = 1000,
                        Features = "[\"Basic campaign management\", \"Basic analytics\", \"Email support\"]",
                        IsActive = true,
                        IsVisible = true,
                        ShowOnLanding = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SubscriptionPlan
                    {
                        Name = "Professional",
                        Description = "Advanced features for growing teams and increased reach",
                        PlanCategory = "For growing teams",
                        IsMostPopular = true,
                        PriceMonthly = 79.99m,
                        PriceYearly = 799.99m, // ~17% discount
                        SMSLimit = 10000,
                        MMSLimit = 1000,
                        EmailLimit = 50000,
                        ContactLimit = 10000,
                        Features = "[\"Advanced campaign management\", \"Workflows & automation\", \"Advanced analytics\", \"Priority support\", \"Custom templates\"]",
                        IsActive = true,
                        IsVisible = true,
                        ShowOnLanding = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new SubscriptionPlan
                    {
                        Name = "Enterprise",
                        Description = "Complete solution with unlimited power and dedicated support",
                        PlanCategory = "For large organizations",
                        IsMostPopular = false,
                        PriceMonthly = 249.99m,
                        PriceYearly = 2499.99m, // ~17% discount
                        SMSLimit = 100000,
                        MMSLimit = 10000,
                        EmailLimit = 500000,
                        ContactLimit = 100000,
                        Features = "[\"Unlimited campaigns\", \"Advanced workflows\", \"Premium analytics\", \"24/7 support\", \"Dedicated account manager\", \"API access\", \"White-label options\"]",
                        IsActive = true,
                        IsVisible = true,
                        ShowOnLanding = true,
                        CreatedAt = DateTime.UtcNow
                    }
                    };

                    context.SubscriptionPlans.AddRange(plans);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} subscription plans.", plans.Count);
                }
                else
                {
                    logger?.LogInformation("Subscription plans already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding subscription plans.");
                throw;
            }
        }

        private static async Task SeedPlanFeatureMappingsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding plan-feature mappings...");

            try
            {
                // Seed Plan-Feature Mappings
                if (!await context.PlanFeatureMappings.AnyAsync())
                {
                    logger?.LogInformation("Creating plan-feature mappings...");

                    // Get plans and features
                    var starterPlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Starter");
                    var proPlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Professional");
                    var enterprisePlan = await context.SubscriptionPlans.FirstOrDefaultAsync(p => p.Name == "Enterprise");

                    var smsFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "SMS Messages");
                    var mmsFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "MMS Messages");
                    var emailFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Email Campaigns");
                    var contactsFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Contact Management");
                    var basicAnalyticsFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Basic Analytics");
                    var advancedAnalyticsFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Advanced Analytics");
                    var automationFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Automation Workflows");
                    var templatesFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Custom Templates");
                    var apiAccessFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "API Access");
                    var prioritySupportFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Priority Support");
                    var support247Feature = await context.Features.FirstOrDefaultAsync(f => f.Name == "24/7 Support");
                    var accountManagerFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Dedicated Account Manager");
                    var whiteLabelFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "White-label Options");
                    var teamCollaborationFeature = await context.Features.FirstOrDefaultAsync(f => f.Name == "Team Collaboration");

                    var mappings = new List<PlanFeatureMapping>();

                    // Starter Plan Features
                    if (starterPlan != null)
                    {
                        if (smsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = smsFeature.Id, IsIncluded = true, FeatureValue = "1,000 messages/month", DisplayOrder = 1, CreatedAt = DateTime.UtcNow });
                        if (mmsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = mmsFeature.Id, IsIncluded = true, FeatureValue = "100 messages/month", DisplayOrder = 2, CreatedAt = DateTime.UtcNow });
                        if (emailFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = emailFeature.Id, IsIncluded = true, FeatureValue = "5,000 emails/month", DisplayOrder = 3, CreatedAt = DateTime.UtcNow });
                        if (contactsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = contactsFeature.Id, IsIncluded = true, FeatureValue = "Up to 1,000 contacts", DisplayOrder = 4, CreatedAt = DateTime.UtcNow });
                        if (basicAnalyticsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = basicAnalyticsFeature.Id, IsIncluded = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow });
                        if (templatesFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = starterPlan.Id, FeatureId = templatesFeature.Id, IsIncluded = true, DisplayOrder = 6, CreatedAt = DateTime.UtcNow });
                    }

                    // Professional Plan Features
                    if (proPlan != null)
                    {
                        if (smsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = smsFeature.Id, IsIncluded = true, FeatureValue = "10,000 messages/month", DisplayOrder = 1, CreatedAt = DateTime.UtcNow });
                        if (mmsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = mmsFeature.Id, IsIncluded = true, FeatureValue = "1,000 messages/month", DisplayOrder = 2, CreatedAt = DateTime.UtcNow });
                        if (emailFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = emailFeature.Id, IsIncluded = true, FeatureValue = "50,000 emails/month", DisplayOrder = 3, CreatedAt = DateTime.UtcNow });
                        if (contactsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = contactsFeature.Id, IsIncluded = true, FeatureValue = "Up to 10,000 contacts", DisplayOrder = 4, CreatedAt = DateTime.UtcNow });
                        if (advancedAnalyticsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = advancedAnalyticsFeature.Id, IsIncluded = true, DisplayOrder = 5, CreatedAt = DateTime.UtcNow });
                        if (automationFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = automationFeature.Id, IsIncluded = true, DisplayOrder = 6, CreatedAt = DateTime.UtcNow });
                        if (templatesFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = templatesFeature.Id, IsIncluded = true, FeatureValue = "Unlimited", DisplayOrder = 7, CreatedAt = DateTime.UtcNow });
                        if (prioritySupportFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = prioritySupportFeature.Id, IsIncluded = true, DisplayOrder = 8, CreatedAt = DateTime.UtcNow });
                        if (teamCollaborationFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = proPlan.Id, FeatureId = teamCollaborationFeature.Id, IsIncluded = true, FeatureValue = "Up to 5 users", DisplayOrder = 9, CreatedAt = DateTime.UtcNow });
                    }

                    // Enterprise Plan Features
                    if (enterprisePlan != null)
                    {
                        if (smsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = smsFeature.Id, IsIncluded = true, FeatureValue = "100,000 messages/month", DisplayOrder = 1, CreatedAt = DateTime.UtcNow });
                        if (mmsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = mmsFeature.Id, IsIncluded = true, FeatureValue = "10,000 messages/month", DisplayOrder = 2, CreatedAt = DateTime.UtcNow });
                        if (emailFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = emailFeature.Id, IsIncluded = true, FeatureValue = "500,000 emails/month", DisplayOrder = 3, CreatedAt = DateTime.UtcNow });
                        if (contactsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = contactsFeature.Id, IsIncluded = true, FeatureValue = "Up to 100,000 contacts", DisplayOrder = 4, CreatedAt = DateTime.UtcNow });
                        if (advancedAnalyticsFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = advancedAnalyticsFeature.Id, IsIncluded = true, FeatureValue = "Custom reports", DisplayOrder = 5, CreatedAt = DateTime.UtcNow });
                        if (automationFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = automationFeature.Id, IsIncluded = true, FeatureValue = "Advanced", DisplayOrder = 6, CreatedAt = DateTime.UtcNow });
                        if (templatesFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = templatesFeature.Id, IsIncluded = true, FeatureValue = "Unlimited", DisplayOrder = 7, CreatedAt = DateTime.UtcNow });
                        if (apiAccessFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = apiAccessFeature.Id, IsIncluded = true, DisplayOrder = 8, CreatedAt = DateTime.UtcNow });
                        if (support247Feature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = support247Feature.Id, IsIncluded = true, DisplayOrder = 9, CreatedAt = DateTime.UtcNow });
                        if (accountManagerFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = accountManagerFeature.Id, IsIncluded = true, DisplayOrder = 10, CreatedAt = DateTime.UtcNow });
                        if (whiteLabelFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = whiteLabelFeature.Id, IsIncluded = true, DisplayOrder = 11, CreatedAt = DateTime.UtcNow });
                        if (teamCollaborationFeature != null) mappings.Add(new PlanFeatureMapping { SubscriptionPlanId = enterprisePlan.Id, FeatureId = teamCollaborationFeature.Id, IsIncluded = true, FeatureValue = "Unlimited users", DisplayOrder = 12, CreatedAt = DateTime.UtcNow });
                    }

                    if (mappings.Any())
                    {
                        context.PlanFeatureMappings.AddRange(mappings);
                        await context.SaveChangesAsync();
                        logger?.LogInformation("Successfully created {Count} plan-feature mappings.", mappings.Count);
                    }
                    else
                    {
                        logger?.LogWarning("No plan-feature mappings created. Plans or features may be missing.");
                    }
                }
                else
                {
                    logger?.LogInformation("Plan-feature mappings already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding plan-feature mappings.");
                throw;
            }
        }

        private static async Task SeedMessageProvidersAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding message providers...");

            try
            {
                // Seed Message Providers
                if (!await context.MessageProviders.AnyAsync())
                {
                    logger?.LogInformation("Creating message providers...");

                    var providers = new List<MessageProvider>
                {
                    new MessageProvider
                    {
                        Name = "Twilio SMS",
                        Type = ProviderType.SMS,
                        IsActive = true,
                        IsPrimary = true,
                        HealthStatus = HealthStatus.Unknown,
                        CreatedAt = DateTime.UtcNow
                    },
                    new MessageProvider
                    {
                        Name = "SendGrid Email",
                        Type = ProviderType.Email,
                        IsActive = true,
                        IsPrimary = true,
                        HealthStatus = HealthStatus.Unknown,
                        CreatedAt = DateTime.UtcNow
                    }
                    };

                    context.MessageProviders.AddRange(providers);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} message providers.", providers.Count);
                }
                else
                {
                    logger?.LogInformation("Message providers already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding message providers.");
                throw;
            }
        }

        private static async Task SeedChannelRoutingConfigsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding channel routing configurations...");

            try
            {
                // Seed Channel Routing Configurations
                if (!await context.ChannelRoutingConfigs.AnyAsync())
                {
                    logger?.LogInformation("Creating channel routing configurations...");

                    var routingConfigs = new List<ChannelRoutingConfig>
                {
                    new ChannelRoutingConfig
                    {
                        Channel = ChannelType.SMS,
                        PrimaryProvider = "MockSMSProvider",
                        FallbackProvider = "BackupSMSProvider",
                        RoutingStrategy = RoutingStrategy.Primary,
                        EnableFallback = true,
                        MaxRetries = 3,
                        RetryStrategy = RetryStrategy.Exponential,
                        InitialRetryDelaySeconds = 60,
                        MaxRetryDelaySeconds = 3600,
                        IsActive = true,
                        Priority = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ChannelRoutingConfig
                    {
                        Channel = ChannelType.MMS,
                        PrimaryProvider = "MockMMSProvider",
                        FallbackProvider = "BackupMMSProvider",
                        RoutingStrategy = RoutingStrategy.Primary,
                        EnableFallback = true,
                        MaxRetries = 3,
                        RetryStrategy = RetryStrategy.Exponential,
                        InitialRetryDelaySeconds = 60,
                        MaxRetryDelaySeconds = 3600,
                        IsActive = true,
                        Priority = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ChannelRoutingConfig
                    {
                        Channel = ChannelType.Email,
                        PrimaryProvider = "MockEmailProvider",
                        FallbackProvider = "BackupEmailProvider",
                        RoutingStrategy = RoutingStrategy.Primary,
                        EnableFallback = true,
                        MaxRetries = 3,
                        RetryStrategy = RetryStrategy.Exponential,
                        InitialRetryDelaySeconds = 120,
                        MaxRetryDelaySeconds = 7200,
                        IsActive = true,
                        Priority = 1,
                        CreatedAt = DateTime.UtcNow
                    }
                    };

                    context.ChannelRoutingConfigs.AddRange(routingConfigs);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} channel routing configurations.", routingConfigs.Count);
                }
                else
                {
                    logger?.LogInformation("Channel routing configurations already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding channel routing configurations.");
                throw;
            }
        }

        private static async Task SeedPricingModelsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding pricing models...");

            try
            {
                // Seed Pricing Models for Landing Page
                if (!await context.PricingModels.AnyAsync())
                {
                    logger?.LogInformation("Creating pricing models...");

                    var pricingModels = new List<PricingModel>
                {
                    new PricingModel
                    {
                        Name = "Starter",
                        Description = "Perfect for small businesses getting started",
                        Type = PricingModelType.Flat,
                        BasePrice = 29.00m,
                        BillingPeriod = BillingPeriod.Monthly,
                        IsActive = true,
                        Priority = 1,
                        Configuration = "{\"features\":[\"1,000 SMS messages/month\",\"500 emails/month\",\"Basic analytics\",\"Email support\",\"1 user\"]}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PricingModel
                    {
                        Name = "Professional",
                        Description = "For growing businesses with larger audiences",
                        Type = PricingModelType.Flat,
                        BasePrice = 99.00m,
                        BillingPeriod = BillingPeriod.Monthly,
                        IsActive = true,
                        Priority = 2,
                        Configuration = "{\"features\":[\"10,000 SMS messages/month\",\"5,000 emails/month\",\"Advanced analytics\",\"Priority support\",\"5 users\",\"Custom templates\",\"Automation workflows\"],\"isPopular\":true}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PricingModel
                    {
                        Name = "Enterprise",
                        Description = "For large organizations with custom needs",
                        Type = PricingModelType.Flat,
                        BasePrice = 299.00m,
                        BillingPeriod = BillingPeriod.Monthly,
                        IsActive = true,
                        Priority = 3,
                        Configuration = "{\"features\":[\"Unlimited SMS messages\",\"Unlimited emails\",\"Advanced analytics & reporting\",\"24/7 phone support\",\"Unlimited users\",\"Custom branding\",\"API access\",\"Dedicated account manager\"]}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                    };

                    context.PricingModels.AddRange(pricingModels);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} pricing models.", pricingModels.Count);
                }
                else
                {
                    logger?.LogInformation("Pricing models already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding pricing models.");
                throw;
            }
        }

        private static async Task SeedLandingPageSettingsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding landing page settings...");

            try
            {
                // Seed Landing Page Configuration Settings
                if (!await context.PlatformSettings.AnyAsync(s => s.Category == "LandingPage"))
                {
                    logger?.LogInformation("Creating landing page settings...");

                    var landingPageSettings = new List<PlatformSetting>
                {
                    // Hero Section Settings
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.Type",
                        Value = "banner",
                        Category = "LandingPage",
                        Description = "Hero section type: 'banner' or 'slider'",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.Title",
                        Value = "Transform Your Marketing with SMS, MMS & Email",
                        Category = "LandingPage",
                        Description = "Hero section main title",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.Subtitle",
                        Value = "A powerful, enterprise-grade marketing platform to reach your customers where they are. Send targeted campaigns, track performance, and grow your business.",
                        Category = "LandingPage",
                        Description = "Hero section subtitle/description",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.BannerImage",
                        Value = "/images/hero-banner.jpg",
                        Category = "LandingPage",
                        Description = "Hero banner image URL",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.CTAText",
                        Value = "Get Started Free",
                        Category = "LandingPage",
                        Description = "Primary call-to-action button text",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Hero.CTALink",
                        Value = "/Auth/Register",
                        Category = "LandingPage",
                        Description = "Primary call-to-action button link",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Slider Settings (if slider type is selected)
                    new PlatformSetting
                    {
                        Key = "LandingPage.Slider.Slides",
                        Value = "[{\"title\":\"Transform Your Marketing\",\"subtitle\":\"Reach customers on SMS, MMS & Email\",\"image\":\"/images/slide1.jpg\",\"ctaText\":\"Get Started\",\"ctaLink\":\"/Auth/Register\"},{\"title\":\"Advanced Analytics\",\"subtitle\":\"Track and optimize your campaigns\",\"image\":\"/images/slide2.jpg\",\"ctaText\":\"Learn More\",\"ctaLink\":\"#features\"},{\"title\":\"Automate Your Workflow\",\"subtitle\":\"Save time with powerful automation\",\"image\":\"/images/slide3.jpg\",\"ctaText\":\"See How\",\"ctaLink\":\"#features\"}]",
                        Category = "LandingPage",
                        Description = "Slider slides configuration (JSON array)",
                        DataType = SettingDataType.Json,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Slider.AutoPlay",
                        Value = "true",
                        Category = "LandingPage",
                        Description = "Enable slider auto-play",
                        DataType = SettingDataType.Boolean,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Slider.Interval",
                        Value = "5000",
                        Category = "LandingPage",
                        Description = "Slider auto-play interval in milliseconds",
                        DataType = SettingDataType.Integer,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Menu/Navigation Settings
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.BackgroundColor",
                        Value = "#ffffff",
                        Category = "LandingPage",
                        Description = "Navigation menu background color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.TextColor",
                        Value = "#212529",
                        Category = "LandingPage",
                        Description = "Navigation menu text color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.HoverColor",
                        Value = "#667eea",
                        Category = "LandingPage",
                        Description = "Navigation menu hover color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.FontSize",
                        Value = "16",
                        Category = "LandingPage",
                        Description = "Navigation menu font size (in pixels)",
                        DataType = SettingDataType.Integer,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.Position",
                        Value = "top",
                        Category = "LandingPage",
                        Description = "Navigation menu position: 'top' or 'fixed'",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Menu.Items",
                        Value = "[{\"text\":\"Home\",\"link\":\"#home\",\"order\":1},{\"text\":\"Features\",\"link\":\"#features\",\"order\":2},{\"text\":\"Pricing\",\"link\":\"#pricing\",\"order\":3},{\"text\":\"Contact\",\"link\":\"#contact\",\"order\":4},{\"text\":\"Login\",\"link\":\"/Auth/Login\",\"order\":5,\"class\":\"btn-outline-primary\"}]",
                        Category = "LandingPage",
                        Description = "Navigation menu items (JSON array)",
                        DataType = SettingDataType.Json,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Theme Colors
                    new PlatformSetting
                    {
                        Key = "LandingPage.Theme.PrimaryColor",
                        Value = "#667eea",
                        Category = "LandingPage",
                        Description = "Primary theme color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Theme.SecondaryColor",
                        Value = "#764ba2",
                        Category = "LandingPage",
                        Description = "Secondary theme color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Theme.AccentColor",
                        Value = "#f093fb",
                        Category = "LandingPage",
                        Description = "Accent theme color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Company Info
                    new PlatformSetting
                    {
                        Key = "LandingPage.Company.Name",
                        Value = "Marketing Platform",
                        Category = "LandingPage",
                        Description = "Company name displayed on landing page",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Company.Logo",
                        Value = "/images/logo.png",
                        Category = "LandingPage",
                        Description = "Company logo URL",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Company.Tagline",
                        Value = "SMS, MMS & Email Marketing Platform",
                        Category = "LandingPage",
                        Description = "Company tagline",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Statistics Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.Stats.MessagesSent",
                        Value = "10M+",
                        Category = "LandingPage",
                        Description = "Messages sent statistic",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Stats.DeliveryRate",
                        Value = "98%",
                        Category = "LandingPage",
                        Description = "Delivery rate statistic",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Stats.ActiveUsers",
                        Value = "5K+",
                        Category = "LandingPage",
                        Description = "Active users statistic",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Stats.Support",
                        Value = "24/7",
                        Category = "LandingPage",
                        Description = "Support availability statistic",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Footer Settings
                    new PlatformSetting
                    {
                        Key = "LandingPage.Footer.CopyrightText",
                        Value = "© 2024 Marketing Platform. All rights reserved.",
                        Category = "LandingPage",
                        Description = "Footer copyright text",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Footer.SocialLinks",
                        Value = "[{\"platform\":\"facebook\",\"url\":\"https://facebook.com/marketingplatform\",\"icon\":\"bi-facebook\"},{\"platform\":\"twitter\",\"url\":\"https://twitter.com/marketingplatform\",\"icon\":\"bi-twitter\"},{\"platform\":\"linkedin\",\"url\":\"https://linkedin.com/company/marketingplatform\",\"icon\":\"bi-linkedin\"},{\"platform\":\"instagram\",\"url\":\"https://instagram.com/marketingplatform\",\"icon\":\"bi-instagram\"}]",
                        Category = "LandingPage",
                        Description = "Footer social media links (JSON array)",
                        DataType = SettingDataType.Json,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // SEO Settings
                    new PlatformSetting
                    {
                        Key = "LandingPage.SEO.Title",
                        Value = "Marketing Platform - SMS, MMS & Email Marketing",
                        Category = "LandingPage",
                        Description = "Page title for SEO",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.SEO.Description",
                        Value = "Transform your marketing with our enterprise-grade SMS, MMS & Email platform. Powerful automation, advanced analytics, and seamless integration.",
                        Category = "LandingPage",
                        Description = "Meta description for SEO",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.SEO.Keywords",
                        Value = "SMS marketing, email marketing, MMS marketing, marketing automation, campaign management",
                        Category = "LandingPage",
                        Description = "Meta keywords for SEO",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Features Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.Features.SectionTitle",
                        Value = "Powerful Features for Modern Marketing",
                        Category = "LandingPage",
                        Description = "Features section title",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Features.SectionSubtitle",
                        Value = "Everything you need to create, manage, and optimize your campaigns",
                        Category = "LandingPage",
                        Description = "Features section subtitle",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Features.List",
                        Value = "[{\"icon\":\"bi-broadcast\",\"title\":\"Multi-Channel Campaigns\",\"description\":\"Send SMS, MMS, and Email campaigns from one unified platform. Reach your audience on their preferred channels.\",\"color\":\"primary\"},{\"icon\":\"bi-graph-up-arrow\",\"title\":\"Advanced Analytics\",\"description\":\"Track campaign performance in real-time with detailed analytics and reporting. Make data-driven decisions to optimize your results.\",\"color\":\"success\"},{\"icon\":\"bi-clock-history\",\"title\":\"Automation & Scheduling\",\"description\":\"Schedule campaigns in advance and automate your marketing workflows. Save time and improve efficiency.\",\"color\":\"info\"},{\"icon\":\"bi-people\",\"title\":\"Contact Management\",\"description\":\"Organize your contacts with dynamic groups and tags. Segment your audience for targeted messaging.\",\"color\":\"warning\"},{\"icon\":\"bi-file-earmark-text\",\"title\":\"Template Library\",\"description\":\"Create reusable message templates with dynamic variables. Personalize content at scale.\",\"color\":\"danger\"},{\"icon\":\"bi-shield-check\",\"title\":\"Compliance & Security\",\"description\":\"Stay compliant with GDPR, CAN-SPAM, and TCPA regulations. Enterprise-grade security for your data.\",\"color\":\"secondary\"}]",
                        Category = "LandingPage",
                        Description = "Features list (JSON array)",
                        DataType = SettingDataType.Json,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Pricing Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.Pricing.SectionTitle",
                        Value = "Simple, Transparent Pricing",
                        Category = "LandingPage",
                        Description = "Pricing section title",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Pricing.SectionSubtitle",
                        Value = "Choose the plan that fits your business needs",
                        Category = "LandingPage",
                        Description = "Pricing section subtitle",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Pricing.ShowYearlyToggle",
                        Value = "true",
                        Category = "LandingPage",
                        Description = "Show monthly/yearly pricing toggle",
                        DataType = SettingDataType.Boolean,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // CTA Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.CTA.Title",
                        Value = "Ready to Transform Your Marketing?",
                        Category = "LandingPage",
                        Description = "Call-to-action section title",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.CTA.Subtitle",
                        Value = "Join thousands of businesses using our platform to grow their reach",
                        Category = "LandingPage",
                        Description = "Call-to-action section subtitle",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.CTA.ButtonText",
                        Value = "Start Free Trial",
                        Category = "LandingPage",
                        Description = "Call-to-action button text",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.CTA.ButtonLink",
                        Value = "/Auth/Register",
                        Category = "LandingPage",
                        Description = "Call-to-action button link",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.CTA.BackgroundColor",
                        Value = "#667eea",
                        Category = "LandingPage",
                        Description = "Call-to-action section background color",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Contact Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.Contact.Email",
                        Value = "support@marketingplatform.com",
                        Category = "LandingPage",
                        Description = "Contact email address",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Contact.Phone",
                        Value = "+1 (555) 123-4567",
                        Category = "LandingPage",
                        Description = "Contact phone number",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Contact.Address",
                        Value = "123 Marketing Street, San Francisco, CA 94102",
                        Category = "LandingPage",
                        Description = "Company address",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },

                    // Testimonials Section
                    new PlatformSetting
                    {
                        Key = "LandingPage.Testimonials.ShowSection",
                        Value = "true",
                        Category = "LandingPage",
                        Description = "Show testimonials section",
                        DataType = SettingDataType.Boolean,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Testimonials.SectionTitle",
                        Value = "What Our Customers Say",
                        Category = "LandingPage",
                        Description = "Testimonials section title",
                        DataType = SettingDataType.String,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    },
                    new PlatformSetting
                    {
                        Key = "LandingPage.Testimonials.List",
                        Value = "[{\"name\":\"John Smith\",\"company\":\"TechCorp Inc.\",\"position\":\"Marketing Director\",\"testimonial\":\"This platform has transformed how we communicate with our customers. The automation features alone have saved us countless hours.\",\"rating\":5,\"image\":\"/images/testimonials/john.jpg\"},{\"name\":\"Sarah Johnson\",\"company\":\"E-commerce Plus\",\"position\":\"CEO\",\"testimonial\":\"Outstanding service and support. Our email campaigns have never performed better. Highly recommended!\",\"rating\":5,\"image\":\"/images/testimonials/sarah.jpg\"},{\"name\":\"Michael Chen\",\"company\":\"Retail Solutions\",\"position\":\"Operations Manager\",\"testimonial\":\"The multi-channel approach is exactly what we needed. We can now reach our customers on their preferred platforms seamlessly.\",\"rating\":5,\"image\":\"/images/testimonials/michael.jpg\"}]",
                        Category = "LandingPage",
                        Description = "Testimonials list (JSON array)",
                        DataType = SettingDataType.Json,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    }
                    };

                    context.PlatformSettings.AddRange(landingPageSettings);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} landing page settings.", landingPageSettings.Count);
                }
                else
                {
                    logger?.LogInformation("Landing page settings already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding landing page settings.");
                throw;
            }
        }

        private static async Task SeedPageContentAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding page content (Privacy Policy and Terms of Service)...");

            try
            {
                // Seed Privacy Policy if it doesn't exist
                var existingPrivacy = await context.PageContents.FirstOrDefaultAsync(p => p.PageKey == "privacy-policy");
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
                var existingTerms = await context.PageContents.FirstOrDefaultAsync(p => p.PageKey == "terms-of-service");
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

                await context.SaveChangesAsync();
                logger?.LogInformation("Page content seeding completed.");
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding page content.");
                throw;
            }
        }
        private static async Task SeedLandingFeaturesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding landing features...");

            try
            {
                if (!await context.LandingFeatures.AnyAsync())
                {
                    logger?.LogInformation("Creating landing features...");

                    var landingFeatures = new List<LandingFeature>
                    {
                        new LandingFeature
                        {
                            Title = "Multi-Channel Campaigns",
                            ShortDescription = "Send SMS, MMS, and Email campaigns from one unified platform.",
                            DetailedDescription = "Reach your audience on their preferred channels with our integrated multi-channel campaign builder. Create, schedule, and manage SMS, MMS, and email campaigns from a single dashboard. Use smart routing to automatically select the best channel for each contact based on engagement history and preferences.",
                            IconClass = "bi-broadcast",
                            ColorClass = "primary",
                            DisplayOrder = 1,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "Start Sending",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Channels",
                            StatValue1 = "3+",
                            StatTitle2 = "Delivery Rate",
                            StatValue2 = "98.5%",
                            StatTitle3 = "Avg. Open Rate",
                            StatValue3 = "89%",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFeature
                        {
                            Title = "Advanced Analytics",
                            ShortDescription = "Track campaign performance in real-time with detailed analytics.",
                            DetailedDescription = "Make data-driven decisions with our comprehensive analytics dashboard. Monitor delivery rates, open rates, click-through rates, and conversions in real-time. Generate custom reports, compare campaign performance, and identify trends to optimize your marketing strategy.",
                            IconClass = "bi-graph-up-arrow",
                            ColorClass = "success",
                            DisplayOrder = 2,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "See Analytics",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Metrics",
                            StatValue1 = "50+",
                            StatTitle2 = "Report Types",
                            StatValue2 = "12",
                            StatTitle3 = "Real-time",
                            StatValue3 = "Yes",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFeature
                        {
                            Title = "Automation & Scheduling",
                            ShortDescription = "Automate your marketing workflows and schedule campaigns in advance.",
                            DetailedDescription = "Save time and improve efficiency with powerful automation tools. Set up trigger-based campaigns, drip sequences, and scheduled sends. Create complex workflows with conditional logic, A/B testing, and automatic follow-ups. Let your marketing run on autopilot while you focus on strategy.",
                            IconClass = "bi-clock-history",
                            ColorClass = "info",
                            DisplayOrder = 3,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "Automate Now",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Time Saved",
                            StatValue1 = "70%",
                            StatTitle2 = "Workflow Steps",
                            StatValue2 = "20+",
                            StatTitle3 = "Triggers",
                            StatValue3 = "15+",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFeature
                        {
                            Title = "Contact Management",
                            ShortDescription = "Organize your contacts with dynamic groups and smart segmentation.",
                            DetailedDescription = "Build and manage your contact database with ease. Import contacts from CSV, API, or manual entry. Create dynamic segments based on demographics, behavior, and engagement. Use tags and custom fields to organize your audience. Maintain compliance with built-in consent management and suppression lists.",
                            IconClass = "bi-people",
                            ColorClass = "warning",
                            DisplayOrder = 4,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "Manage Contacts",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Import Sources",
                            StatValue1 = "5+",
                            StatTitle2 = "Custom Fields",
                            StatValue2 = "Unlimited",
                            StatTitle3 = "Segments",
                            StatValue3 = "Dynamic",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFeature
                        {
                            Title = "Template Library",
                            ShortDescription = "Create reusable message templates with dynamic personalization.",
                            DetailedDescription = "Build professional messages quickly with our template library. Choose from pre-designed templates or create your own. Use dynamic variables for personalized content at scale. Preview messages across different devices and channels before sending. Share templates across your team for consistent brand messaging.",
                            IconClass = "bi-file-earmark-text",
                            ColorClass = "danger",
                            DisplayOrder = 5,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "Browse Templates",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Templates",
                            StatValue1 = "100+",
                            StatTitle2 = "Variables",
                            StatValue2 = "Dynamic",
                            StatTitle3 = "Preview",
                            StatValue3 = "Multi-device",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFeature
                        {
                            Title = "Compliance & Security",
                            ShortDescription = "Stay compliant with GDPR, CAN-SPAM, and TCPA regulations.",
                            DetailedDescription = "Enterprise-grade security and built-in compliance tools to protect your data and your customers. Automatic consent tracking, opt-out management, and suppression list handling. SOC 2 Type II certified infrastructure with end-to-end encryption. Regular security audits and penetration testing ensure your data stays safe.",
                            IconClass = "bi-shield-check",
                            ColorClass = "secondary",
                            DisplayOrder = 6,
                            IsActive = true,
                            ShowOnLanding = true,
                            CallToActionText = "Learn More",
                            CallToActionUrl = "/Auth/Register",
                            StatTitle1 = "Compliance",
                            StatValue1 = "GDPR/TCPA",
                            StatTitle2 = "Encryption",
                            StatValue2 = "256-bit",
                            StatTitle3 = "Uptime",
                            StatValue3 = "99.9%",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.LandingFeatures.AddRange(landingFeatures);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} landing features.", landingFeatures.Count);
                }
                else
                {
                    logger?.LogInformation("Landing features already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding landing features.");
                throw;
            }
        }

        private static async Task SeedLandingFaqsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding landing FAQs...");

            try
            {
                if (!await context.LandingFaqs.AnyAsync())
                {
                    logger?.LogInformation("Creating landing FAQs...");

                    var faqs = new List<LandingFaq>
                    {
                        new LandingFaq
                        {
                            Question = "How do I get started with the platform?",
                            Answer = "Getting started is easy! Simply create a free account, verify your email, and you'll have immediate access to our dashboard. From there, you can import your contacts, create your first campaign, and start sending messages. We also offer a guided onboarding tour and comprehensive documentation to help you every step of the way.",
                            IconClass = "bi-rocket-takeoff",
                            IconColor = "#667eea",
                            DisplayOrder = 1,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Getting Started",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "What messaging channels are supported?",
                            Answer = "Our platform supports SMS (text messages), MMS (multimedia messages with images, videos, and files), and Email campaigns. You can use all three channels from a single dashboard, create multi-channel campaigns, and let our smart routing system choose the best channel for each contact based on their preferences and engagement history.",
                            IconClass = "bi-chat-dots",
                            IconColor = "#764ba2",
                            DisplayOrder = 2,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Features",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "Can I try the platform for free?",
                            Answer = "Yes! We offer a free trial that includes 100 SMS messages, 50 MMS messages, and 500 emails. No credit card is required to sign up. You'll have full access to all features during the trial period so you can experience the full power of our platform before committing to a paid plan.",
                            IconClass = "bi-gift",
                            IconColor = "#f093fb",
                            DisplayOrder = 3,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Pricing",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "How does pricing work?",
                            Answer = "We offer three flexible pricing plans: Starter ($29.99/month), Professional ($79.99/month), and Enterprise ($249.99/month). Each plan includes different message limits and features. You can also save up to 17% with annual billing. All plans include core features like contact management, basic analytics, and template access. Upgrade or downgrade at any time.",
                            IconClass = "bi-currency-dollar",
                            IconColor = "#28a745",
                            DisplayOrder = 4,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Pricing",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "Is my data secure and compliant?",
                            Answer = "Absolutely. We take security and compliance very seriously. Our platform is SOC 2 Type II certified, uses 256-bit encryption for all data, and is fully compliant with GDPR, CAN-SPAM, and TCPA regulations. We provide built-in tools for consent management, opt-out handling, and suppression lists. Regular security audits and penetration testing ensure your data stays protected.",
                            IconClass = "bi-shield-lock",
                            IconColor = "#dc3545",
                            DisplayOrder = 5,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Security",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "Do you offer API access for developers?",
                            Answer = "Yes! Our RESTful API provides programmatic access to all platform features including sending messages, managing contacts, creating campaigns, and retrieving analytics. We offer SDKs for popular programming languages, comprehensive API documentation, and webhook support for real-time event notifications. API access is available on Professional and Enterprise plans.",
                            IconClass = "bi-code-slash",
                            IconColor = "#17a2b8",
                            DisplayOrder = 6,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Features",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "What kind of support do you offer?",
                            Answer = "We offer multiple levels of support depending on your plan. All plans include email support with a 24-hour response time. Professional plans get priority support with a 4-hour response time. Enterprise plans include 24/7 phone support and a dedicated account manager. We also provide extensive documentation, video tutorials, and a community forum for self-service help.",
                            IconClass = "bi-headset",
                            IconColor = "#6f42c1",
                            DisplayOrder = 7,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Support",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingFaq
                        {
                            Question = "Can I import my existing contacts?",
                            Answer = "Yes, you can easily import contacts from CSV files, Excel spreadsheets, or via our API. We also support direct integrations with popular CRM platforms like Salesforce, HubSpot, and Zoho. During import, our system automatically validates phone numbers and email addresses, removes duplicates, and checks against suppression lists to ensure compliance.",
                            IconClass = "bi-cloud-upload",
                            IconColor = "#fd7e14",
                            DisplayOrder = 8,
                            IsActive = true,
                            ShowOnLanding = true,
                            Category = "Getting Started",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.LandingFaqs.AddRange(faqs);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} landing FAQs.", faqs.Count);
                }
                else
                {
                    logger?.LogInformation("Landing FAQs already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding landing FAQs.");
                throw;
            }
        }

        private static async Task SeedTestimonialsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding testimonials...");

            try
            {
                if (!await context.Testimonials.AnyAsync())
                {
                    logger?.LogInformation("Creating testimonials...");

                    var testimonials = new List<Testimonial>
                    {
                        new Testimonial
                        {
                            CustomerName = "John Smith",
                            CustomerTitle = "Marketing Director",
                            CompanyName = "TechCorp Inc.",
                            AvatarUrl = "/images/testimonials/avatar1.jpg",
                            Rating = 5,
                            TestimonialText = "This platform has completely transformed how we communicate with our customers. The automation features alone have saved us countless hours every week, and our campaign engagement rates have increased by 45% since we switched.",
                            DisplayOrder = 1,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Testimonial
                        {
                            CustomerName = "Sarah Johnson",
                            CustomerTitle = "CEO",
                            CompanyName = "E-commerce Plus",
                            AvatarUrl = "/images/testimonials/avatar2.jpg",
                            Rating = 5,
                            TestimonialText = "Outstanding service and support. Our email campaigns have never performed better, and the SMS integration has opened up an entirely new channel for customer engagement. Highly recommended for any growing business!",
                            DisplayOrder = 2,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Testimonial
                        {
                            CustomerName = "Michael Chen",
                            CustomerTitle = "Operations Manager",
                            CompanyName = "Retail Solutions",
                            AvatarUrl = "/images/testimonials/avatar3.jpg",
                            Rating = 5,
                            TestimonialText = "The multi-channel approach is exactly what we needed. We can now reach our customers on their preferred platforms seamlessly. The analytics dashboard gives us clear insights into what's working and what needs improvement.",
                            DisplayOrder = 3,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Testimonial
                        {
                            CustomerName = "Emily Davis",
                            CustomerTitle = "Digital Marketing Lead",
                            CompanyName = "HealthFirst Clinic",
                            AvatarUrl = "/images/testimonials/avatar4.jpg",
                            Rating = 5,
                            TestimonialText = "We reduced our patient no-show rate by 60% using the automated SMS reminders. The HIPAA-compliant messaging gives us peace of mind, and the scheduling features are incredibly intuitive. A must-have for healthcare marketing.",
                            DisplayOrder = 4,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Testimonial
                        {
                            CustomerName = "David Park",
                            CustomerTitle = "VP of Sales",
                            CompanyName = "PropTech Realty",
                            AvatarUrl = "/images/testimonials/avatar5.jpg",
                            Rating = 4,
                            TestimonialText = "The platform has been a game-changer for our real estate business. We use it for property alerts, open house invitations, and follow-up sequences. Our showing appointments increased by 35% within the first month.",
                            DisplayOrder = 5,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new Testimonial
                        {
                            CustomerName = "Lisa Martinez",
                            CustomerTitle = "Founder",
                            CompanyName = "FitLife Studios",
                            AvatarUrl = "/images/testimonials/avatar6.jpg",
                            Rating = 5,
                            TestimonialText = "As a small business owner, I needed something powerful yet easy to use. This platform delivers on both fronts. The template library saved me so much time, and the pricing is very competitive compared to other solutions we evaluated.",
                            DisplayOrder = 6,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.Testimonials.AddRange(testimonials);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} testimonials.", testimonials.Count);
                }
                else
                {
                    logger?.LogInformation("Testimonials already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding testimonials.");
                throw;
            }
        }

        private static async Task SeedTrustedCompaniesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding trusted companies...");

            try
            {
                if (!await context.TrustedCompanies.AnyAsync())
                {
                    logger?.LogInformation("Creating trusted companies...");

                    var companies = new List<TrustedCompany>
                    {
                        new TrustedCompany
                        {
                            CompanyName = "Microsoft",
                            LogoUrl = "/images/logos/microsoft.svg",
                            WebsiteUrl = "https://microsoft.com",
                            DisplayOrder = 1,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Google",
                            LogoUrl = "/images/logos/google.svg",
                            WebsiteUrl = "https://google.com",
                            DisplayOrder = 2,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Salesforce",
                            LogoUrl = "/images/logos/salesforce.svg",
                            WebsiteUrl = "https://salesforce.com",
                            DisplayOrder = 3,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Shopify",
                            LogoUrl = "/images/logos/shopify.svg",
                            WebsiteUrl = "https://shopify.com",
                            DisplayOrder = 4,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "HubSpot",
                            LogoUrl = "/images/logos/hubspot.svg",
                            WebsiteUrl = "https://hubspot.com",
                            DisplayOrder = 5,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Slack",
                            LogoUrl = "/images/logos/slack.svg",
                            WebsiteUrl = "https://slack.com",
                            DisplayOrder = 6,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Stripe",
                            LogoUrl = "/images/logos/stripe.svg",
                            WebsiteUrl = "https://stripe.com",
                            DisplayOrder = 7,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Zoom",
                            LogoUrl = "/images/logos/zoom.svg",
                            WebsiteUrl = "https://zoom.us",
                            DisplayOrder = 8,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Adobe",
                            LogoUrl = "/images/logos/adobe.svg",
                            WebsiteUrl = "https://adobe.com",
                            DisplayOrder = 9,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Cisco",
                            LogoUrl = "/images/logos/cisco.svg",
                            WebsiteUrl = "https://cisco.com",
                            DisplayOrder = 10,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "IBM",
                            LogoUrl = "/images/logos/ibm.svg",
                            WebsiteUrl = "https://ibm.com",
                            DisplayOrder = 11,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new TrustedCompany
                        {
                            CompanyName = "Intel",
                            LogoUrl = "/images/logos/intel.svg",
                            WebsiteUrl = "https://intel.com",
                            DisplayOrder = 12,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.TrustedCompanies.AddRange(companies);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} trusted companies.", companies.Count);
                }
                else
                {
                    logger?.LogInformation("Trusted companies already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding trusted companies.");
                throw;
            }
        }

        private static async Task SeedSecurityBadgesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding security badges...");

            try
            {
                if (!await context.SecurityBadges.AnyAsync())
                {
                    logger?.LogInformation("Creating security badges...");

                    var badges = new List<SecurityBadge>
                    {
                        new SecurityBadge
                        {
                            Title = "GDPR Compliant",
                            Subtitle = "EU Data Protection",
                            IconUrl = "/images/badges/gdpr.svg",
                            Description = "Fully compliant with the General Data Protection Regulation. We ensure your data is processed lawfully, transparently, and for specific purposes.",
                            DisplayOrder = 1,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new SecurityBadge
                        {
                            Title = "SOC 2 Type II",
                            Subtitle = "Security Certified",
                            IconUrl = "/images/badges/soc2.svg",
                            Description = "SOC 2 Type II certified, demonstrating our commitment to security, availability, processing integrity, confidentiality, and privacy of customer data.",
                            DisplayOrder = 2,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new SecurityBadge
                        {
                            Title = "CCPA Compliant",
                            Subtitle = "California Privacy",
                            IconUrl = "/images/badges/ccpa.svg",
                            Description = "Compliant with the California Consumer Privacy Act, ensuring California residents' rights to know, delete, and opt-out of the sale of their personal information.",
                            DisplayOrder = 3,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new SecurityBadge
                        {
                            Title = "TCPA Compliant",
                            Subtitle = "Telecom Regulation",
                            IconUrl = "/images/badges/tcpa.svg",
                            Description = "Full compliance with the Telephone Consumer Protection Act. Built-in consent management, opt-out handling, and calling hour restrictions.",
                            DisplayOrder = 4,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new SecurityBadge
                        {
                            Title = "256-bit Encryption",
                            Subtitle = "Data Protection",
                            IconUrl = "/images/badges/encryption.svg",
                            Description = "All data is encrypted at rest and in transit using industry-standard AES-256 encryption. Your sensitive information is always protected.",
                            DisplayOrder = 5,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new SecurityBadge
                        {
                            Title = "99.9% Uptime",
                            Subtitle = "Service Reliability",
                            IconUrl = "/images/badges/uptime.svg",
                            Description = "Enterprise-grade infrastructure with 99.9% uptime SLA. Redundant systems, automatic failover, and 24/7 monitoring ensure your campaigns always get delivered.",
                            DisplayOrder = 6,
                            IsActive = true,
                            ShowOnLanding = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.SecurityBadges.AddRange(badges);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} security badges.", badges.Count);
                }
                else
                {
                    logger?.LogInformation("Security badges already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding security badges.");
                throw;
            }
        }

        private static async Task SeedUseCasesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding use cases...");

            try
            {
                if (!await context.UseCases.AnyAsync())
                {
                    logger?.LogInformation("Creating use cases...");

                    var useCases = new List<UseCase>
                    {
                        // E-Commerce
                        new UseCase
                        {
                            Title = "Abandoned Cart Recovery",
                            Description = "Recover lost sales with automated SMS and email reminders.\nSend personalized product recommendations based on browsing history.\nOffer time-sensitive discounts to encourage purchase completion.\nTrack recovery rates and optimize messaging for maximum ROI.",
                            IconClass = "bi-cart-check",
                            Industry = "E-Commerce",
                            ImageUrl = "/images/use-cases/e-commerce.svg",
                            ResultsText = "45% increase in recovered revenue",
                            ColorClass = "primary",
                            DisplayOrder = 1,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new UseCase
                        {
                            Title = "Order Status Updates",
                            Description = "Keep customers informed with real-time order tracking via SMS.\nAutomate shipping notifications and delivery confirmations.\nReduce support inquiries with proactive status updates.\nImprove customer satisfaction with timely communication.",
                            IconClass = "bi-box-seam",
                            Industry = "E-Commerce",
                            ImageUrl = "/images/use-cases/e-commerce.svg",
                            ResultsText = "30% reduction in support tickets",
                            ColorClass = "info",
                            DisplayOrder = 2,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        // Healthcare
                        new UseCase
                        {
                            Title = "Appointment Reminders",
                            Description = "Reduce no-shows with automated appointment reminders via SMS.\nAllow patients to confirm or reschedule with simple text replies.\nSend pre-visit instructions and required documentation reminders.\nHIPAA-compliant messaging ensures patient privacy.",
                            IconClass = "bi-calendar-check",
                            Industry = "Healthcare",
                            ImageUrl = "/images/use-cases/healthcare.svg",
                            ResultsText = "60% reduction in no-show rates",
                            ColorClass = "success",
                            DisplayOrder = 3,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new UseCase
                        {
                            Title = "Patient Engagement",
                            Description = "Send wellness tips and preventive care reminders.\nFollow up after visits with care instructions and satisfaction surveys.\nPromote health screenings and vaccination campaigns.\nBuild lasting patient relationships through consistent communication.",
                            IconClass = "bi-heart-pulse",
                            Industry = "Healthcare",
                            ImageUrl = "/images/use-cases/healthcare.svg",
                            ResultsText = "40% improvement in patient engagement",
                            ColorClass = "danger",
                            DisplayOrder = 4,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        // Real Estate
                        new UseCase
                        {
                            Title = "Property Alerts",
                            Description = "Send instant property alerts matching buyer preferences.\nShare virtual tour links and high-quality property images via MMS.\nSchedule open house invitations with one-click RSVP.\nAutomate follow-ups after property viewings.",
                            IconClass = "bi-house-door",
                            Industry = "Real Estate",
                            ImageUrl = "/images/use-cases/real-estate.svg",
                            ResultsText = "35% increase in property showings",
                            ColorClass = "warning",
                            DisplayOrder = 5,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        // Retail
                        new UseCase
                        {
                            Title = "Promotional Campaigns",
                            Description = "Drive foot traffic with location-based SMS promotions.\nSend flash sale alerts and exclusive member-only offers.\nCreate urgency with limited-time discount codes.\nTrack coupon redemption rates and campaign ROI.",
                            IconClass = "bi-shop",
                            Industry = "Retail",
                            ImageUrl = "/images/use-cases/retail.svg",
                            ResultsText = "50% increase in foot traffic during campaigns",
                            ColorClass = "primary",
                            DisplayOrder = 6,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        // SaaS
                        new UseCase
                        {
                            Title = "User Onboarding",
                            Description = "Guide new users through product setup with automated drip campaigns.\nSend helpful tips and tutorials based on user activity.\nReduce churn with engagement-triggered re-activation messages.\nCollect feedback with in-app survey links via SMS.",
                            IconClass = "bi-cloud-check",
                            Industry = "SaaS",
                            ImageUrl = "/images/use-cases/saas.svg",
                            ResultsText = "25% improvement in user activation rates",
                            ColorClass = "info",
                            DisplayOrder = 7,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        // Education
                        new UseCase
                        {
                            Title = "Student Communication",
                            Description = "Send class schedule updates and assignment reminders.\nNotify students about enrollment deadlines and campus events.\nShare emergency alerts and campus safety notifications.\nFacilitate parent-teacher communication for K-12 schools.",
                            IconClass = "bi-mortarboard",
                            Industry = "Education",
                            ImageUrl = "/images/use-cases/education.svg",
                            ResultsText = "55% increase in event attendance",
                            ColorClass = "success",
                            DisplayOrder = 8,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.UseCases.AddRange(useCases);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} use cases.", useCases.Count);
                }
                else
                {
                    logger?.LogInformation("Use cases already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding use cases.");
                throw;
            }
        }

        private static async Task SeedLandingStatsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding landing stats...");

            try
            {
                if (!await context.LandingStats.AnyAsync())
                {
                    logger?.LogInformation("Creating landing stats...");

                    var stats = new List<LandingStat>
                    {
                        new LandingStat
                        {
                            Value = "10M+",
                            Label = "Messages Sent",
                            Description = "Over 10 million messages delivered successfully across all channels",
                            IconClass = "bi-send-check",
                            ColorClass = "primary",
                            CounterTarget = 10000000,
                            CounterSuffix = "+",
                            CounterPrefix = "",
                            DisplayOrder = 1,
                            IsActive = true,
                            ShowOnLanding = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingStat
                        {
                            Value = "98.5%",
                            Label = "Delivery Rate",
                            Description = "Industry-leading delivery rate across SMS, MMS, and Email channels",
                            IconClass = "bi-check-circle",
                            ColorClass = "success",
                            CounterTarget = 98,
                            CounterSuffix = ".5%",
                            CounterPrefix = "",
                            DisplayOrder = 2,
                            IsActive = true,
                            ShowOnLanding = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingStat
                        {
                            Value = "5,000+",
                            Label = "Active Users",
                            Description = "Trusted by over 5,000 businesses worldwide",
                            IconClass = "bi-people",
                            ColorClass = "info",
                            CounterTarget = 5000,
                            CounterSuffix = "+",
                            CounterPrefix = "",
                            DisplayOrder = 3,
                            IsActive = true,
                            ShowOnLanding = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new LandingStat
                        {
                            Value = "24/7",
                            Label = "Support Available",
                            Description = "Round-the-clock customer support for enterprise customers",
                            IconClass = "bi-headset",
                            ColorClass = "warning",
                            CounterTarget = 24,
                            CounterSuffix = "/7",
                            CounterPrefix = "",
                            DisplayOrder = 4,
                            IsActive = true,
                            ShowOnLanding = true,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.LandingStats.AddRange(stats);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} landing stats.", stats.Count);
                }
                else
                {
                    logger?.LogInformation("Landing stats already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding landing stats.");
                throw;
            }
        }

        private static async Task SeedFooterSettingsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding footer settings...");

            try
            {
                if (!await context.FooterSettings.AnyAsync())
                {
                    logger?.LogInformation("Creating footer settings...");

                    var footerSettings = new FooterSettings
                    {
                        CompanyName = "Marketing Platform",
                        CompanyDescription = "A powerful, enterprise-grade marketing platform for SMS, MMS, and Email campaigns. Reach your customers where they are and grow your business.",
                        AddressLine1 = "123 Marketing Street",
                        AddressLine2 = "San Francisco, CA 94102",
                        Phone = "+1 (555) 123-4567",
                        Email = "support@marketingplatform.com",
                        BusinessHours = "Mon - Fri: 9:00 AM - 6:00 PM PST",
                        MapEmbedUrl = "https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3153.0977731692825!2d-122.4194!3d37.7749!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x0%3A0x0!2zMzfCsDQ2JzI5LjYiTiAxMjLCsDI1JzA5LjgiVw!5e0!3m2!1sen!2sus!4v1234567890",
                        FacebookUrl = "https://facebook.com/marketingplatform",
                        TwitterUrl = "https://twitter.com/marketingplatform",
                        LinkedInUrl = "https://linkedin.com/company/marketingplatform",
                        InstagramUrl = "https://instagram.com/marketingplatform",
                        YouTubeUrl = "https://youtube.com/@marketingplatform",
                        CopyrightText = "\u00a9 2024 Marketing Platform. All rights reserved.",
                        ShowNewsletter = true,
                        NewsletterTitle = "Stay Updated",
                        NewsletterDescription = "Subscribe to our newsletter for the latest marketing tips, product updates, and industry insights.",
                        ShowMap = true,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    context.FooterSettings.Add(footerSettings);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created footer settings.");
                }
                else
                {
                    logger?.LogInformation("Footer settings already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding footer settings.");
                throw;
            }
        }

        private static async Task SeedComplianceRulesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding compliance rules...");

            try
            {
                if (!await context.ComplianceRules.IgnoreQueryFilters().AnyAsync())
                {
                    logger?.LogInformation("Creating compliance rules...");

                    var complianceRules = new List<ComplianceRule>
                    {
                        new ComplianceRule
                        {
                            Name = "TCPA Consent Requirement",
                            Description = "Requires prior express written consent before sending SMS/MMS marketing messages to US phone numbers, as mandated by the Telephone Consumer Protection Act.",
                            RuleType = ComplianceRuleType.ConsentManagement,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"region\":\"US\",\"channels\":[\"SMS\",\"MMS\"],\"consentType\":\"express_written\",\"requireDoubleOptIn\":true}",
                            Priority = 100,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = "US",
                            ApplicableServices = "SMS,MMS",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "CAN-SPAM Unsubscribe Requirement",
                            Description = "All commercial email must include a clear unsubscribe mechanism, and opt-out requests must be honored within 10 business days, as required by the CAN-SPAM Act.",
                            RuleType = ComplianceRuleType.OptOutEnforcement,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"region\":\"US\",\"channels\":[\"Email\"],\"maxOptOutProcessingDays\":10,\"requireUnsubscribeLink\":true,\"requirePhysicalAddress\":true}",
                            Priority = 95,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = "US",
                            ApplicableServices = "Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "GDPR Data Protection",
                            Description = "Ensures lawful processing of personal data for EU residents, including explicit consent, right to erasure, data portability, and breach notification within 72 hours.",
                            RuleType = ComplianceRuleType.RegionalCompliance,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"region\":\"EU\",\"channels\":[\"SMS\",\"MMS\",\"Email\"],\"requireExplicitConsent\":true,\"rightToErasure\":true,\"dataPortability\":true,\"breachNotificationHours\":72}",
                            Priority = 100,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = "EU,EEA,UK",
                            ApplicableServices = "SMS,MMS,Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "CCPA Consumer Rights",
                            Description = "California Consumer Privacy Act compliance ensuring California residents' right to know, delete, and opt-out of the sale of personal information.",
                            RuleType = ComplianceRuleType.RegionalCompliance,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"region\":\"US-CA\",\"channels\":[\"SMS\",\"MMS\",\"Email\"],\"rightToKnow\":true,\"rightToDelete\":true,\"rightToOptOut\":true,\"doNotSellLink\":true}",
                            Priority = 90,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = "US-CA",
                            ApplicableServices = "SMS,MMS,Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "Data Retention Policy",
                            Description = "Default data retention policy. Campaign message logs retained for 2 years, consent records for 7 years, analytics data for 3 years.",
                            RuleType = ComplianceRuleType.DataRetention,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"messageLogRetentionDays\":730,\"consentRetentionDays\":2555,\"analyticsRetentionDays\":1095,\"auditLogRetentionDays\":2555,\"autoDeleteExpired\":false}",
                            Priority = 80,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = null,
                            ApplicableServices = "SMS,MMS,Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "Message Rate Limiting",
                            Description = "Default rate limiting rule to prevent spam. Limits the number of messages per contact per channel within a time window.",
                            RuleType = ComplianceRuleType.RateLimiting,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"smsMaxPerDay\":5,\"smsMaxPerWeek\":15,\"emailMaxPerDay\":3,\"emailMaxPerWeek\":10,\"mmsMaxPerDay\":3,\"mmsMaxPerWeek\":10,\"cooldownMinutes\":60}",
                            Priority = 70,
                            IsMandatory = false,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = null,
                            ApplicableServices = "SMS,MMS,Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new ComplianceRule
                        {
                            Name = "Message Content Standards",
                            Description = "Ensures message content meets platform standards. Prohibits spam, misleading content, and enforces required disclaimers.",
                            RuleType = ComplianceRuleType.MessageContent,
                            Status = ComplianceRuleStatus.Active,
                            Configuration = "{\"requireSenderIdentification\":true,\"prohibitMisleadingSubjects\":true,\"requireOptOutInstructions\":true,\"maxSmsLength\":1600,\"prohibitedContentKeywords\":[\"guaranteed\",\"free money\",\"act now\"]}",
                            Priority = 75,
                            IsMandatory = true,
                            EffectiveFrom = DateTime.UtcNow,
                            ApplicableRegions = null,
                            ApplicableServices = "SMS,MMS,Email",
                            CreatedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.ComplianceRules.AddRange(complianceRules);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} compliance rules.", complianceRules.Count);
                }
                else
                {
                    logger?.LogInformation("Compliance rules already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding compliance rules.");
                throw;
            }
        }

        private static async Task SeedFeatureTogglesAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding feature toggles...");

            try
            {
                if (!await context.FeatureToggles.IgnoreQueryFilters().AnyAsync())
                {
                    logger?.LogInformation("Creating feature toggles...");

                    var featureToggles = new List<FeatureToggle>
                    {
                        new FeatureToggle
                        {
                            Name = "sms_campaigns",
                            DisplayName = "SMS Campaigns",
                            Description = "Enable SMS campaign creation and sending",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Messaging",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "mms_campaigns",
                            DisplayName = "MMS Campaigns",
                            Description = "Enable MMS multimedia campaign creation and sending",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Messaging",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "email_campaigns",
                            DisplayName = "Email Campaigns",
                            Description = "Enable email campaign creation and sending",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Messaging",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "automation_workflows",
                            DisplayName = "Automation Workflows",
                            Description = "Enable workflow automation engine for trigger-based campaigns",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Automation",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "ab_testing",
                            DisplayName = "A/B Testing",
                            Description = "Enable A/B testing for campaigns with variant analytics",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Campaigns",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "advanced_analytics",
                            DisplayName = "Advanced Analytics",
                            Description = "Enable detailed analytics, custom reports, and export functionality",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            EnabledForRoles = "SuperAdmin,Admin,Manager,Analyst",
                            Category = "Analytics",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "api_access",
                            DisplayName = "API Access",
                            Description = "Enable REST API access for programmatic platform usage",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Integration",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "live_chat",
                            DisplayName = "Live Chat Support",
                            Description = "Enable real-time chat support for customers",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Support",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "contact_import",
                            DisplayName = "Contact Import",
                            Description = "Enable bulk contact import from CSV, Excel, and API",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Contacts",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "dynamic_groups",
                            DisplayName = "Dynamic Contact Groups",
                            Description = "Enable rule-based dynamic contact group segmentation",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Contacts",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "url_shortening",
                            DisplayName = "URL Shortening & Tracking",
                            Description = "Enable automatic URL shortening and click tracking in campaigns",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Campaigns",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        },
                        new FeatureToggle
                        {
                            Name = "two_factor_auth",
                            DisplayName = "Two-Factor Authentication",
                            Description = "Enable two-factor authentication for user accounts",
                            Status = FeatureToggleStatus.Enabled,
                            IsEnabled = true,
                            Category = "Security",
                            ModifiedBy = "System",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        }
                    };

                    context.FeatureToggles.AddRange(featureToggles);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} feature toggles.", featureToggles.Count);
                }
                else
                {
                    logger?.LogInformation("Feature toggles already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding feature toggles.");
                throw;
            }
        }

        private static async Task SeedPlatformConfigurationsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding platform configurations...");

            try
            {
                if (!await context.PlatformConfigurations.AnyAsync())
                {
                    logger?.LogInformation("Creating platform configurations...");

                    var configs = new List<PlatformConfiguration>
                    {
                        // General Settings
                        new PlatformConfiguration
                        {
                            Key = "Platform.Name",
                            Value = "Marketing Platform",
                            Category = ConfigurationCategory.General,
                            DataType = "string",
                            Description = "Platform display name",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Platform.SupportEmail",
                            Value = "support@marketingplatform.com",
                            Category = ConfigurationCategory.General,
                            DataType = "string",
                            Description = "Support email address displayed to users",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Platform.DefaultTimezone",
                            Value = "America/New_York",
                            Category = ConfigurationCategory.General,
                            DataType = "string",
                            Description = "Default timezone for new users and scheduled campaigns",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Platform.MaxFileUploadSizeMB",
                            Value = "25",
                            Category = ConfigurationCategory.General,
                            DataType = "int",
                            Description = "Maximum file upload size in megabytes",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },

                        // Security Settings
                        new PlatformConfiguration
                        {
                            Key = "Security.PasswordMinLength",
                            Value = "8",
                            Category = ConfigurationCategory.Security,
                            DataType = "int",
                            Description = "Minimum password length requirement",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Security.MaxLoginAttempts",
                            Value = "5",
                            Category = ConfigurationCategory.Security,
                            DataType = "int",
                            Description = "Maximum failed login attempts before account lockout",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Security.LockoutDurationMinutes",
                            Value = "30",
                            Category = ConfigurationCategory.Security,
                            DataType = "int",
                            Description = "Account lockout duration in minutes",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Security.SessionTimeoutMinutes",
                            Value = "60",
                            Category = ConfigurationCategory.Security,
                            DataType = "int",
                            Description = "User session timeout in minutes",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Security.RefreshTokenExpirationDays",
                            Value = "7",
                            Category = ConfigurationCategory.Security,
                            DataType = "int",
                            Description = "JWT refresh token expiration in days",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },

                        // Messaging Settings
                        new PlatformConfiguration
                        {
                            Key = "Messaging.DefaultSMSSenderId",
                            Value = "+15551234567",
                            Category = ConfigurationCategory.Messaging,
                            DataType = "string",
                            Description = "Default SMS sender ID / phone number",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Messaging.DefaultEmailFrom",
                            Value = "noreply@marketingplatform.com",
                            Category = ConfigurationCategory.Messaging,
                            DataType = "string",
                            Description = "Default from email address for campaigns",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Messaging.MaxSMSPerBatch",
                            Value = "1000",
                            Category = ConfigurationCategory.Messaging,
                            DataType = "int",
                            Description = "Maximum number of SMS messages per batch send",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Messaging.MaxEmailPerBatch",
                            Value = "5000",
                            Category = ConfigurationCategory.Messaging,
                            DataType = "int",
                            Description = "Maximum number of emails per batch send",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Messaging.RetryMaxAttempts",
                            Value = "3",
                            Category = ConfigurationCategory.Messaging,
                            DataType = "int",
                            Description = "Maximum retry attempts for failed message delivery",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },

                        // Compliance Settings
                        new PlatformConfiguration
                        {
                            Key = "Compliance.QuietHoursStart",
                            Value = "21:00",
                            Category = ConfigurationCategory.Compliance,
                            DataType = "string",
                            Description = "Default quiet hours start time (no SMS/MMS sending)",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Compliance.QuietHoursEnd",
                            Value = "08:00",
                            Category = ConfigurationCategory.Compliance,
                            DataType = "string",
                            Description = "Default quiet hours end time",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Compliance.DefaultOptOutKeywords",
                            Value = "STOP,UNSUBSCRIBE,CANCEL,END,QUIT",
                            Category = ConfigurationCategory.Compliance,
                            DataType = "string",
                            Description = "Default opt-out keywords for SMS (comma-separated)",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Compliance.DefaultOptInKeywords",
                            Value = "START,SUBSCRIBE,YES,JOIN",
                            Category = ConfigurationCategory.Compliance,
                            DataType = "string",
                            Description = "Default opt-in keywords for SMS (comma-separated)",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },

                        // Performance Settings
                        new PlatformConfiguration
                        {
                            Key = "Performance.CacheDurationMinutes",
                            Value = "15",
                            Category = ConfigurationCategory.Performance,
                            DataType = "int",
                            Description = "Default cache duration for frequently accessed data",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Performance.MaxConcurrentCampaigns",
                            Value = "10",
                            Category = ConfigurationCategory.Performance,
                            DataType = "int",
                            Description = "Maximum concurrent campaign processing threads",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },

                        // Billing Settings
                        new PlatformConfiguration
                        {
                            Key = "Billing.Currency",
                            Value = "USD",
                            Category = ConfigurationCategory.Billing,
                            DataType = "string",
                            Description = "Default billing currency",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Billing.InvoicePrefix",
                            Value = "INV",
                            Category = ConfigurationCategory.Billing,
                            DataType = "string",
                            Description = "Invoice number prefix",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        },
                        new PlatformConfiguration
                        {
                            Key = "Billing.GracePeriodDays",
                            Value = "7",
                            Category = ConfigurationCategory.Billing,
                            DataType = "int",
                            Description = "Number of grace period days after subscription expires",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.PlatformConfigurations.AddRange(configs);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} platform configurations.", configs.Count);
                }
                else
                {
                    logger?.LogInformation("Platform configurations already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding platform configurations.");
                throw;
            }
        }

        private static async Task SeedFileStorageSettingsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding file storage settings...");

            try
            {
                if (!await context.FileStorageSettings.AnyAsync())
                {
                    logger?.LogInformation("Creating file storage settings...");

                    var storageSettings = new FileStorageSettings
                    {
                        ProviderName = "Local",
                        LocalBasePath = "wwwroot/uploads",
                        IsEnabled = true,
                        IsDefault = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ConfigurationJson = "{\"maxFileSizeMB\":25,\"allowedExtensions\":[\".jpg\",\".jpeg\",\".png\",\".gif\",\".svg\",\".pdf\",\".csv\",\".xlsx\",\".mp4\",\".webp\"],\"createThumbnails\":true,\"thumbnailWidth\":200,\"thumbnailHeight\":200}"
                    };

                    context.FileStorageSettings.Add(storageSettings);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created file storage settings.");
                }
                else
                {
                    logger?.LogInformation("File storage settings already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding file storage settings.");
                throw;
            }
        }

        private static async Task SeedTaxConfigurationsAsync(ApplicationDbContext context, ILogger? logger)
        {
            logger?.LogInformation("Seeding tax configurations...");

            try
            {
                if (!await context.TaxConfigurations.AnyAsync())
                {
                    logger?.LogInformation("Creating tax configurations...");

                    var taxConfigs = new List<TaxConfiguration>
                    {
                        new TaxConfiguration
                        {
                            Name = "US Federal - Communications Tax",
                            Type = TaxType.ServiceFee,
                            RegionCode = "US",
                            Rate = 5.82m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 1,
                            Configuration = "{\"description\":\"Federal Universal Service Fund fee applied to messaging services\",\"appliesTo\":[\"SMS\",\"MMS\"]}",
                            CreatedAt = DateTime.UtcNow
                        },
                        new TaxConfiguration
                        {
                            Name = "US - Standard Sales Tax",
                            Type = TaxType.SalesTax,
                            RegionCode = "US",
                            Rate = 0m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 2,
                            Configuration = "{\"description\":\"Default US sales tax rate - varies by state, set to 0 as placeholder\",\"note\":\"Override per state as needed\"}",
                            CreatedAt = DateTime.UtcNow
                        },
                        new TaxConfiguration
                        {
                            Name = "EU VAT - Standard Rate",
                            Type = TaxType.VAT,
                            RegionCode = "EU",
                            Rate = 20.0m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 1,
                            Configuration = "{\"description\":\"Standard EU VAT rate for digital services\",\"reverseChargeEligible\":true}",
                            CreatedAt = DateTime.UtcNow
                        },
                        new TaxConfiguration
                        {
                            Name = "UK VAT - Standard Rate",
                            Type = TaxType.VAT,
                            RegionCode = "UK",
                            Rate = 20.0m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 1,
                            Configuration = "{\"description\":\"UK VAT standard rate for digital services\"}",
                            CreatedAt = DateTime.UtcNow
                        },
                        new TaxConfiguration
                        {
                            Name = "Canada GST",
                            Type = TaxType.GST,
                            RegionCode = "CA",
                            Rate = 5.0m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 1,
                            Configuration = "{\"description\":\"Canadian Goods and Services Tax\"}",
                            CreatedAt = DateTime.UtcNow
                        },
                        new TaxConfiguration
                        {
                            Name = "Platform Processing Fee",
                            Type = TaxType.ProcessingFee,
                            RegionCode = null,
                            Rate = 2.9m,
                            FlatAmount = 0.30m,
                            IsPercentage = true,
                            IsActive = true,
                            Priority = 10,
                            Configuration = "{\"description\":\"Payment processing fee (percentage + flat fee per transaction)\",\"appliesTo\":[\"card_payments\"]}",
                            CreatedAt = DateTime.UtcNow
                        }
                    };

                    context.TaxConfigurations.AddRange(taxConfigs);
                    await context.SaveChangesAsync();
                    logger?.LogInformation("Successfully created {Count} tax configurations.", taxConfigs.Count);
                }
                else
                {
                    logger?.LogInformation("Tax configurations already exist.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Error seeding tax configurations.");
                throw;
            }
        }
    }
}

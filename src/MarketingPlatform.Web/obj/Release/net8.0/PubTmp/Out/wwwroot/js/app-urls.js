/**
 * URL Configuration - Centralized URL Management
 * All application URLs and API endpoints in one place for easy maintenance
 * COMPREHENSIVE API ENDPOINT CONFIGURATION
 */

const AppUrls = {
    // ==================== MVC ROUTES (Web Pages) ====================
    
    // Home & Public Pages
    home: {
        index: '/home/index',
        privacy: '/home/privacy',
        terms: '/home/terms',
        error: '/home/error'
    },

    // Authentication Pages
    auth: {
        login: '/auth/login',
        register: '/auth/register',
        logout: '/auth/logout',
        forgotPassword: '/auth/forgot-password',
        resetPassword: '/auth/reset-password',
        recaptchaConfig: '/auth/recaptcha-config'
    },

    // Keywords Management Pages
    keywords: {
        index: '/keywords/index',
        create: '/keywords/create',
        edit: (id) => `/keywords/edit/${id}`,
        analytics: (id) => `/keywords/analytics/${id}`,
        details: (id) => `/keywords/details/${id}`,
        delete: (id) => `/keywords/delete/${id}`
    },

    // Campaigns Management Pages
    campaigns: {
        index: '/campaigns/index',
        create: '/campaigns/create',
        edit: (id) => `/campaigns/edit/${id}`,
        variants: (id) => `/campaigns/variants/${id}`,
        details: (id) => `/campaigns/details/${id}`,
        delete: (id) => `/campaigns/delete/${id}`,
        start: (id) => `/campaigns/start/${id}`,
        pause: (id) => `/campaigns/pause/${id}`
    },

    // Contacts Management Pages
    contacts: {
        index: '/contacts/index',
        create: '/contacts/create',
        details: (id) => `/contacts/details/${id}`,
        groups: '/contacts/groups',
        delete: (id) => `/contacts/delete/${id}`,
        addToGroup: '/contacts/addtogroup',
        removeFromGroup: '/contacts/removefromgroup'
    },

    // Messages Management Pages
    messages: {
        index: '/messages/index',
        compose: '/messages/compose',
        details: (id) => `/messages/details/${id}`,
        preview: '/messages/preview',
        send: (id) => `/messages/send/${id}`
    },

    // Templates Management Pages
    templates: {
        index: '/templates/index',
        create: '/templates/create',
        edit: (id) => `/templates/edit/${id}`,
        preview: (id) => `/templates/preview/${id}`,
        delete: (id) => `/templates/delete/${id}`,
        get: (id) => `/templates/get/${id}`
    },

    // Analytics & Reporting Pages
    analytics: {
        index: '/analytics/index',
        campaigns: '/analytics/campaigns',
        reports: '/analytics/reports',
        export: '/analytics/export'
    },

    // Workflows Management Pages
    workflows: {
        index: '/workflows/index',
        create: '/workflows/create',
        edit: (id) => `/workflows/edit/${id}`,
        details: (id) => `/workflows/details/${id}`,
        delete: (id) => `/workflows/delete/${id}`,
        execute: (id) => `/workflows/execute/${id}`
    },

    // Users Management Pages
    users: {
        index: '/users/index',
        dashboard: '/users/dashboard',
        profile: '/users/profile',
        settings: '/users/settings',
        details: (id) => `/users/details/${id}`,
        edit: (id) => `/users/edit/${id}`,
        delete: (id) => `/users/delete/${id}`
    },

    // Roles Management Pages
    roles: {
        index: '/roles/index',
        create: '/roles/create',
        edit: (id) => `/roles/edit/${id}`,
        details: (id) => `/roles/details/${id}`,
        delete: (id) => `/roles/delete/${id}`,
        togglePermission: '/roles/togglepermission'
    },

    // Settings Pages
    settings: {
        index: '/settings/index',
        integrations: '/settings/integrations',
        compliance: '/settings/compliance'
    },

    // Billing Pages
    billing: {
        index: '/billing/index',
        plans: '/billing/plans',
        history: '/billing/history',
        subscription: '/billing/subscription',
        subscribe: (planId) => `/billing/subscribe/${planId || ''}`,
        cancel: '/billing/cancel',
        invoice: (id) => `/billing/invoice/${id}`,
        pay: (id) => `/billing/pay/${id}`
    },

    // Providers Pages
    providers: {
        index: '/providers/index',
        create: '/providers/create',
        edit: (id) => `/providers/edit/${id}`,
        health: (id) => `/providers/health/${id}`,
        routing: '/providers/routing',
        delete: (id) => `/providers/delete/${id}`,
        test: (id) => `/providers/test/${id}`,
        toggle: (id) => `/providers/toggle/${id}`
    },

    // Suppression Lists Pages
    suppression: {
        index: '/suppression/index',
        create: '/suppression/create',
        entries: (id) => `/suppression/entries/${id}`,
        import: '/suppression/import',
        edit: (id) => `/suppression/edit/${id}`,
        delete: (id) => `/suppression/delete/${id}`
    },

    // Pricing Management Pages
    pricing: {
        index: '/pricing/index',
        create: '/pricing/create',
        edit: (id) => `/pricing/edit/${id}`,
        channels: (modelId) => `/pricing/channels/${modelId}`,
        delete: (id) => `/pricing/delete/${id}`
    },

    // Webhooks Management Pages
    webhooks: {
        index: '/webhooks/index',
        create: '/webhooks/create',
        edit: (id) => `/webhooks/edit/${id}`,
        delete: (id) => `/webhooks/delete/${id}`,
        test: (id) => `/webhooks/test/${id}`,
        logs: (id) => `/webhooks/logs/${id}`
    },

    // URL Management Pages
    urls: {
        index: '/urls/index',
        create: '/urls/create',
        edit: (id) => `/urls/edit/${id}`,
        delete: (id) => `/urls/delete/${id}`,
        analytics: (id) => `/urls/analytics/${id}`
    },

    // Super Admin Pages
    superAdmin: {
        auditLog: (id) => `/superadmin/auditlog/${id}`
    },

    // ==================== API ENDPOINTS ====================
    
    api: {
        base: '/api',
        
        // ========== 1. AUTHENTICATION & AUTHORIZATION ==========
        
        // Auth API
        auth: {
            register: '/api/auth/register',
            login: '/api/auth/login',
            refreshToken: '/api/auth/refresh-token',
            logout: '/api/auth/logout',
            changePassword: '/api/auth/change-password',
            me: '/api/auth/me',
            forgotPassword: '/api/auth/forgot-password',
            resetPassword: '/api/auth/reset-password',
            recaptchaConfig: '/api/auth/recaptcha-config',
            verifyEmail: '/api/auth/verify-email',
            resendOtp: '/api/auth/resend-otp'
        },
        
        // OAuth2 API
        oauth2: {
            providers: '/api/oauth2/providers',
            authorize: (providerName) => `/api/oauth2/authorize/${providerName}`,
            callback: (providerName) => `/api/oauth2/callback/${providerName}`,
            link: (providerName) => `/api/oauth2/link/${providerName}`,
            unlink: (providerName) => `/api/oauth2/unlink/${providerName}`,
            admin: {
                providers: '/api/oauth2/admin/providers',
                get: (id) => `/api/oauth2/admin/providers/${id}`,
                create: '/api/oauth2/admin/providers',
                update: (id) => `/api/oauth2/admin/providers/${id}`,
                delete: (id) => `/api/oauth2/admin/providers/${id}`,
                toggleStatus: (id) => `/api/oauth2/admin/providers/${id}/status`
            }
        },
        
        // Roles API
        roles: {
            list: '/api/roles',
            active: '/api/roles/active',
            get: (id) => `/api/roles/${id}`,
            getByName: (name) => `/api/roles/name/${name}`,
            create: '/api/roles',
            update: (id) => `/api/roles/${id}`,
            delete: (id) => `/api/roles/${id}`,
            users: (id) => `/api/roles/${id}/users`,
            assign: '/api/roles/assign',
            remove: '/api/roles/remove',
            permissions: '/api/roles/permissions',
            userPermissions: (userId) => `/api/roles/user/${userId}/permissions`
        },
        
        // ========== 2. CAMPAIGNS ==========
        
        campaigns: {
            list: '/api/campaigns',
            get: (id) => `/api/campaigns/${id}`,
            byStatus: (status) => `/api/campaigns/status/${status}`,
            create: '/api/campaigns',
            update: (id) => `/api/campaigns/${id}`,
            delete: (id) => `/api/campaigns/${id}`,
            duplicate: (id) => `/api/campaigns/${id}/duplicate`,
            schedule: (id) => `/api/campaigns/${id}/schedule`,
            start: (id) => `/api/campaigns/${id}/start`,
            pause: (id) => `/api/campaigns/${id}/pause`,
            resume: (id) => `/api/campaigns/${id}/resume`,
            cancel: (id) => `/api/campaigns/${id}/cancel`,
            stats: (id) => `/api/campaigns/${id}/stats`,
            calculateAudience: '/api/campaigns/calculate-audience',
            
            // Campaign Variants
            variants: {
                list: (campaignId) => `/api/campaigns/${campaignId}/variants`,
                get: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}`,
                create: (campaignId) => `/api/campaigns/${campaignId}/variants`,
                update: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}`,
                delete: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}`,
                activate: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}/activate`,
                deactivate: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}/deactivate`,
                comparison: (campaignId) => `/api/campaigns/${campaignId}/variants/comparison`,
                selectWinner: (campaignId, variantId) => `/api/campaigns/${campaignId}/variants/${variantId}/select-winner`
            }
        },
        
        // ========== 3. CONTACTS & GROUPS ==========
        
        contacts: {
            list: '/api/contacts',
            get: (id) => `/api/contacts/${id}`,
            create: '/api/contacts',
            update: (id) => `/api/contacts/${id}`,
            delete: (id) => `/api/contacts/${id}`,
            search: '/api/contacts/search',
            import: {
                csv: '/api/contacts/import/csv',
                excel: '/api/contacts/import/excel'
            },
            export: {
                csv: '/api/contacts/export/csv',
                excel: '/api/contacts/export/excel'
            },
            duplicates: {
                check: '/api/contacts/check-duplicates',
                report: '/api/contacts/duplicates/report',
                resolve: '/api/contacts/duplicates/resolve'
            }
        },
        
        contactGroups: {
            list: '/api/contactgroups',
            get: (id) => `/api/contactgroups/${id}`,
            create: '/api/contactgroups',
            update: (id) => `/api/contactgroups/${id}`,
            delete: (id) => `/api/contactgroups/${id}`,
            addContact: (groupId, contactId) => `/api/contactgroups/${groupId}/contacts/${contactId}`,
            removeContact: (groupId, contactId) => `/api/contactgroups/${groupId}/contacts/${contactId}`,
            contacts: (id) => `/api/contactgroups/${id}/contacts`,
            refresh: (id) => `/api/contactgroups/${id}/refresh`,
            refreshAll: '/api/contactgroups/refresh-all'
        },
        
        contactTags: {
            list: '/api/contacttags',
            get: (id) => `/api/contacttags/${id}`,
            create: '/api/contacttags',
            update: (id) => `/api/contacttags/${id}`,
            delete: (id) => `/api/contacttags/${id}`,
            assign: (contactId, tagId) => `/api/contacttags/contacts/${contactId}/tags/${tagId}`,
            remove: (contactId, tagId) => `/api/contacttags/contacts/${contactId}/tags/${tagId}`,
            byContact: (contactId) => `/api/contacttags/contacts/${contactId}`
        },
        
        // ========== 4. MESSAGES ==========
        
        messages: {
            list: '/api/messages',
            get: (id) => `/api/messages/${id}`,
            byCampaign: (campaignId) => `/api/messages/campaign/${campaignId}`,
            byStatus: (status) => `/api/messages/status/${status}`,
            create: '/api/messages',
            bulkCreate: '/api/messages/bulk',
            send: (id) => `/api/messages/${id}/send`,
            retry: (id) => `/api/messages/${id}/retry`,
            cancel: (id) => `/api/messages/${id}/cancel`,
            retryCampaign: (campaignId) => `/api/messages/campaign/${campaignId}/retry`,
            deliveryReport: (campaignId) => `/api/messages/campaign/${campaignId}/report`,
            preview: '/api/messages/preview',
            testSend: '/api/messages/test-send'
        },
        
        // ========== 5. KEYWORDS ==========
        
        keywords: {
            list: '/api/keywords',
            get: (id) => `/api/keywords/${id}`,
            byStatus: (status) => `/api/keywords/status/${status}`,
            checkAvailability: '/api/keywords/check-availability',
            create: '/api/keywords',
            update: (id) => `/api/keywords/${id}`,
            delete: (id) => `/api/keywords/${id}`,
            activities: (id) => `/api/keywords/${id}/activities`,
            analytics: (id) => `/api/keywords/${id}/analytics`,
            processInbound: '/api/keywords/process-inbound',
            
            // Reservations
            reservations: {
                create: '/api/keywords/reservations',
                list: '/api/keywords/reservations',
                get: (id) => `/api/keywords/reservations/${id}`,
                update: (id) => `/api/keywords/reservations/${id}`,
                approve: (id) => `/api/keywords/reservations/${id}/approve`,
                reject: (id) => `/api/keywords/reservations/${id}/reject`
            },
            
            // Assignments
            assignments: {
                create: '/api/keywords/assignments',
                list: '/api/keywords/assignments',
                get: (id) => `/api/keywords/assignments/${id}`,
                byCampaign: (campaignId) => `/api/keywords/assignments/campaign/${campaignId}`,
                unassign: (id) => `/api/keywords/assignments/${id}/unassign`
            },
            
            // Conflicts
            conflicts: {
                list: '/api/keywords/conflicts',
                check: '/api/keywords/conflicts/check',
                resolve: (id) => `/api/keywords/conflicts/${id}/resolve`
            }
        },
        
        // ========== 6. ANALYTICS & REPORTING ==========
        
        analytics: {
            dashboard: '/api/analytics/dashboard',
            campaignPerformance: '/api/analytics/campaigns/performance',
            campaignById: (campaignId) => `/api/analytics/campaigns/${campaignId}/performance`,
            contactEngagement: '/api/analytics/contacts/engagement',
            contactById: (contactId) => `/api/analytics/contacts/${contactId}/engagement`,
            conversions: '/api/analytics/conversions',
            campaignConversions: (campaignId) => `/api/analytics/campaigns/${campaignId}/conversions`,
            
            // Export endpoints
            export: {
                campaignPerformanceCsv: '/api/analytics/campaigns/performance/export/csv',
                campaignPerformanceExcel: '/api/analytics/campaigns/performance/export/excel',
                contactEngagementCsv: '/api/analytics/contacts/engagement/export/csv',
                contactEngagementExcel: '/api/analytics/contacts/engagement/export/excel',
                conversionsCsv: '/api/analytics/conversions/export/csv',
                conversionsExcel: '/api/analytics/conversions/export/excel'
            }
        },
        
        // ========== 7. TEMPLATES ==========
        
        templates: {
            list: '/api/templates',
            get: (id) => `/api/templates/${id}`,
            byChannel: (channel) => `/api/templates/channel/${channel}`,
            byCategory: (category) => `/api/templates/category/${category}`,
            create: '/api/templates',
            update: (id) => `/api/templates/${id}`,
            delete: (id) => `/api/templates/${id}`,
            duplicate: (id) => `/api/templates/${id}/duplicate`,
            setDefault: (id) => `/api/templates/${id}/set-default`,
            activate: (id) => `/api/templates/${id}/activate`,
            deactivate: (id) => `/api/templates/${id}/deactivate`,
            preview: '/api/templates/preview',
            extractVariables: '/api/templates/extract-variables',
            stats: (id) => `/api/templates/${id}/stats`,
            calculateCharCount: '/api/templates/calculate-character-count'
        },
        
        // ========== 8. COMPLIANCE & SUPPRESSION ==========
        
        compliance: {
            consentStatus: (contactId) => `/api/compliance/contacts/${contactId}/consent-status`,
            recordConsent: '/api/compliance/consent',
            bulkConsent: '/api/compliance/consent/bulk',
            revokeConsent: (contactId) => `/api/compliance/contacts/${contactId}/revoke-consent`,
            consents: (contactId) => `/api/compliance/contacts/${contactId}/consents`,
            consentHistory: (contactId) => `/api/compliance/contacts/${contactId}/consent-history`,
            settings: '/api/compliance/settings',
            updateSettings: '/api/compliance/settings',
            checkCompliance: (contactId) => `/api/compliance/contacts/${contactId}/check`,
            checkQuietHours: '/api/compliance/quiet-hours/check',
            checkSuppression: '/api/compliance/check-suppression',
            filterCompliant: '/api/compliance/filter-compliant',
            auditLogs: '/api/compliance/audit-logs',
            processOptout: (contactId) => `/api/compliance/contacts/${contactId}/process-optout`,
            processOptin: (contactId) => `/api/compliance/contacts/${contactId}/process-optin`
        },
        
        complianceRules: {
            list: '/api/compliancerules',
            get: (id) => `/api/compliancerules/${id}`,
            active: '/api/compliancerules/active',
            byType: (ruleType) => `/api/compliancerules/type/${ruleType}`,
            applicable: '/api/compliancerules/applicable',
            audit: (id) => `/api/compliancerules/${id}/audit`,
            create: '/api/compliancerules',
            update: (id) => `/api/compliancerules/${id}`,
            activate: (id) => `/api/compliancerules/${id}/activate`,
            deactivate: (id) => `/api/compliancerules/${id}/deactivate`,
            delete: (id) => `/api/compliancerules/${id}`
        },
        
        suppressionLists: {
            list: '/api/suppressionlists',
            get: (id) => `/api/suppressionlists/${id}`,
            create: '/api/suppressionlists',
            bulkCreate: '/api/suppressionlists/bulk',
            delete: (id) => `/api/suppressionlists/${id}`,
            check: (phoneOrEmail) => `/api/suppressionlists/check/${phoneOrEmail}`
        },
        
        // ========== 9. BILLING & SUBSCRIPTIONS ==========
        
        billing: {
            subscription: '/api/billing/subscription',
            subscribe: '/api/billing/subscribe',
            upgrade: '/api/billing/upgrade',
            cancel: '/api/billing/cancel',
            invoices: '/api/billing/invoices',
            history: '/api/billing/history',
            downloadInvoice: (invoiceId) => `/api/billing/invoices/${invoiceId}/download`
        },
        
        subscriptionPlans: {
            list: '/api/subscriptionplans',
            visible: '/api/subscriptionplans/visible',
            landing: '/api/subscriptionplans/landing',
            get: (id) => `/api/subscriptionplans/${id}`,
            create: '/api/subscriptionplans',
            update: (id) => `/api/subscriptionplans/${id}`,
            delete: (id) => `/api/subscriptionplans/${id}`,
            setVisibility: (id) => `/api/subscriptionplans/${id}/visibility`,
            setShowOnLanding: (id) => `/api/subscriptionplans/${id}/show-on-landing`,
            features: (id) => `/api/subscriptionplans/${id}/features`,
            eligibleUpgrades: (currentPlanId) => `/api/subscriptionplans/${currentPlanId}/eligible-upgrades`
        },

        landingFeatures: {
            list: '/api/landingfeatures',
            get: (id) => `/api/landingfeatures/${id}`,
            create: '/api/landingfeatures',
            update: (id) => `/api/landingfeatures/${id}`,
            delete: (id) => `/api/landingfeatures/${id}`
        },

        landingFaqs: {
            list: '/api/landingfaqs',
            get: (id) => `/api/landingfaqs/${id}`,
            create: '/api/landingfaqs',
            update: (id) => `/api/landingfaqs/${id}`,
            delete: (id) => `/api/landingfaqs/${id}`
        },

        securityBadges: {
            list: '/api/securitybadges',
            get: (id) => `/api/securitybadges/${id}`,
            create: '/api/securitybadges',
            update: (id) => `/api/securitybadges/${id}`,
            delete: (id) => `/api/securitybadges/${id}`
        },

        useCases: {
            list: '/api/usecases',
            get: (id) => `/api/usecases/${id}`,
            byIndustry: (industry) => `/api/usecases/industry/${industry}`,
            create: '/api/usecases',
            update: (id) => `/api/usecases/${id}`,
            delete: (id) => `/api/usecases/${id}`
        },

        landingStats: {
            list: '/api/landingstats',
            get: (id) => `/api/landingstats/${id}`,
            create: '/api/landingstats',
            update: (id) => `/api/landingstats/${id}`,
            delete: (id) => `/api/landingstats/${id}`
        },

        pricing: {
            // Models
            models: {
                list: '/api/pricing/models',
                get: (id) => `/api/pricing/models/${id}`,
                create: '/api/pricing/models',
                update: (id) => `/api/pricing/models/${id}`,
                delete: (id) => `/api/pricing/models/${id}`
            },
            
            // Channels
            channels: {
                list: (modelId) => `/api/pricing/models/${modelId}/channels`,
                get: (id) => `/api/pricing/channels/${id}`,
                create: '/api/pricing/channels',
                update: (id) => `/api/pricing/channels/${id}`,
                delete: (id) => `/api/pricing/channels/${id}`
            },
            
            // Regions
            regions: {
                list: (modelId) => `/api/pricing/models/${modelId}/regions`,
                get: (id) => `/api/pricing/regions/${id}`,
                create: '/api/pricing/regions',
                update: (id) => `/api/pricing/regions/${id}`,
                delete: (id) => `/api/pricing/regions/${id}`
            },
            
            // Usage
            usage: {
                list: (modelId) => `/api/pricing/models/${modelId}/usage`,
                get: (id) => `/api/pricing/usage/${id}`,
                create: '/api/pricing/usage',
                update: (id) => `/api/pricing/usage/${id}`,
                delete: (id) => `/api/pricing/usage/${id}`
            },
            
            // Taxes
            taxes: {
                list: '/api/pricing/taxes',
                get: (id) => `/api/pricing/taxes/${id}`,
                create: '/api/pricing/taxes',
                update: (id) => `/api/pricing/taxes/${id}`,
                delete: (id) => `/api/pricing/taxes/${id}`
            }
        },
        
        // ========== 10. URLS & TRACKING ==========
        
        urls: {
            create: '/api/urls',
            list: '/api/urls',
            get: (id) => `/api/urls/${id}`,
            byCampaign: (campaignId) => `/api/urls/campaign/${campaignId}`,
            stats: (id) => `/api/urls/${id}/stats`,
            campaignStats: (campaignId) => `/api/urls/campaign/${campaignId}/stats`,
            delete: (id) => `/api/urls/${id}`,
            redirect: (shortCode) => `/api/urls/r/${shortCode}`
        },
        
        // ========== 11. JOURNEYS & WORKFLOWS ==========

        journeys: {
            list: '/api/journeys',
            get: (id) => `/api/journeys/${id}`,
            create: '/api/journeys',
            update: (id) => `/api/journeys/${id}`,
            delete: (id) => `/api/journeys/${id}`,
            duplicate: (id) => `/api/journeys/${id}/duplicate`,
            stats: (id) => `/api/journeys/${id}/stats`,
            executions: (id) => `/api/journeys/${id}/executions`,
            execute: (id) => `/api/journeys/${id}/execute`,
            pauseExecution: (executionId) => `/api/journeys/executions/${executionId}/pause`,
            resumeExecution: (executionId) => `/api/journeys/executions/${executionId}/resume`,
            cancelExecution: (executionId) => `/api/journeys/executions/${executionId}/cancel`
        },

        workflows: {
            list: '/api/workflows',
            get: (id) => `/api/workflows/${id}`,
            create: '/api/workflows',
            update: (id) => `/api/workflows/${id}`,
            delete: (id) => `/api/workflows/${id}`,
            duplicate: (id) => `/api/workflows/${id}/duplicate`,
            activate: (id) => `/api/workflows/${id}/activate`,
            pause: (id) => `/api/workflows/${id}/pause`,
            execute: (id) => `/api/workflows/${id}/execute`,
            stats: (id) => `/api/workflows/${id}/stats`
        },
        
        // ========== 12. AUDIENCE & SEGMENTATION ==========
        
        audience: {
            evaluate: '/api/audience/evaluate',
            calculateSize: '/api/audience/calculate-size',
            refreshGroup: (groupId) => `/api/audience/groups/${groupId}/refresh`
        },
        
        // ========== 13. USERS & MANAGEMENT ==========
        
        users: {
            profile: '/api/users/profile',
            get: (userId) => `/api/users/${userId}`,
            list: '/api/users',
            update: '/api/users/profile',
            stats: '/api/users/stats',
            deactivate: (userId) => `/api/users/${userId}/deactivate`,
            activate: (userId) => `/api/users/${userId}/activate`
        },
        
        // ========== 14. WEBHOOKS & INTEGRATIONS ==========
        
        webhooks: {
            messageStatus: '/api/webhooks/message-status',
            smsInbound: '/api/webhooks/sms-inbound',
            smsDelivery: '/api/webhooks/sms-delivery',
            emailDelivery: '/api/webhooks/email-delivery',
            optOut: '/api/webhooks/opt-out',
            stripe: '/api/webhooks/stripe',
            paypal: '/api/webhooks/paypal'
        },
        
        integration: {
            testConnection: '/api/integration/crm/test-connection',
            getFields: '/api/integration/crm/fields',
            syncFrom: '/api/integration/crm/sync-from',
            syncTo: '/api/integration/crm/sync-to',
            syncCampaign: (campaignId) => `/api/integration/crm/sync-campaign/${campaignId}`
        },
        
        // ========== 15. CHAT API ==========
        
        chat: {
            rooms: '/api/chat/rooms',
            unassignedRooms: '/api/chat/rooms/unassigned',
            employeeRooms: (employeeId) => `/api/chat/rooms/employee/${employeeId}`,
            getRoom: (id) => `/api/chat/rooms/${id}`,
            getMessages: (id) => `/api/chat/rooms/${id}/messages`,
            createRoom: '/api/chat/rooms',
            assignRoom: (id) => `/api/chat/rooms/${id}/assign`,
            closeRoom: (id) => `/api/chat/rooms/${id}/close`,
            sendMessage: '/api/chat/messages',
            sendTranscript: (id) => `/api/chat/rooms/${id}/send-transcript`,
            unreadCount: (employeeId) => `/api/chat/unread-count/${employeeId}`,
            markRead: (id) => `/api/chat/rooms/${id}/mark-read`
        },
        
        // ========== 16. PLATFORM SETTINGS & ADMIN ==========
        
        platformSettings: {
            list: '/api/platformsettings',
            get: (id) => `/api/platformsettings/${id}`,
            getByKey: (key) => `/api/platformsettings/key/${key}`,
            byCategory: (category) => `/api/platformsettings/category/${category}`,
            create: '/api/platformsettings',
            update: (id) => `/api/platformsettings/${id}`,
            delete: (id) => `/api/platformsettings/${id}`
        },
        
        routingConfig: {
            list: '/api/routingconfig',
            get: (id) => `/api/routingconfig/${id}`,
            byChannel: (channel) => `/api/routingconfig/channel/${channel}`,
            create: '/api/routingconfig',
            update: (id) => `/api/routingconfig/${id}`,
            delete: (id) => `/api/routingconfig/${id}`,
            deliveryAttempts: (messageId) => `/api/routingconfig/delivery-attempts/${messageId}`,
            channelStats: (channel) => `/api/routingconfig/stats/channel/${channel}`,
            overallStats: '/api/routingconfig/stats/overall'
        },
        
        rateLimits: {
            status: '/api/ratelimits/status',
            list: '/api/ratelimits',
            create: '/api/ratelimits',
            update: (id) => `/api/ratelimits/${id}`,
            delete: (id) => `/api/ratelimits/${id}`,
            logs: '/api/ratelimits/logs'
        },
        
        featureToggles: {
            list: '/api/featuretoggles',
            get: (id) => `/api/featuretoggles/${id}`,
            getByName: (name) => `/api/featuretoggles/name/${name}`,
            byCategory: (category) => `/api/featuretoggles/category/${category}`,
            isEnabled: (name) => `/api/featuretoggles/${name}/enabled`,
            isEnabledForMe: (name) => `/api/featuretoggles/${name}/enabled/me`,
            isEnabledForRole: (name, roleName) => `/api/featuretoggles/${name}/enabled/role/${roleName}`,
            create: '/api/featuretoggles',
            update: (id) => `/api/featuretoggles/${id}`,
            toggle: (id) => `/api/featuretoggles/${id}/toggle`,
            delete: (id) => `/api/featuretoggles/${id}`
        },
        
        superAdmin: {
            roles: '/api/superadmin/roles',
            getRole: (userId) => `/api/superadmin/roles/${userId}`,
            assignRole: '/api/superadmin/roles/assign',
            revokeRole: '/api/superadmin/roles/revoke',
            checkRole: (userId) => `/api/superadmin/roles/check/${userId}`,
            logs: '/api/superadmin/logs',
            logsByEntity: (entityType, entityId) => `/api/superadmin/logs/entity/${entityType}/${entityId}`,
            logsByUser: (userId) => `/api/superadmin/logs/user/${userId}`,
            criticalLogs: '/api/superadmin/logs/critical',
            config: '/api/superadmin/config',
            getConfig: (key) => `/api/superadmin/config/${key}`,
            configByCategory: (category) => `/api/superadmin/config/category/${category}`,
            updateConfig: '/api/superadmin/config',
            toggleFeature: '/api/superadmin/config/toggle-feature',
            platformAnalytics: '/api/superadmin/analytics/platform',
            billingAnalytics: '/api/superadmin/analytics/billing',
            monthlyRevenue: '/api/superadmin/analytics/revenue/monthly',
            health: '/api/superadmin/health',
            providerHealth: '/api/superadmin/health/providers'
        },
        
        // ========== 17. HEALTH CHECK ==========
        
        health: {
            check: '/api/health'
        }
    },
    
    // ==================== SIGNALR HUBS ====================
    
    hubs: {
        chat: '/hubs/chat'
    }
};

// ==================== HELPER FUNCTIONS ====================

// Helper function to build URL with query parameters
AppUrls.buildUrl = function(baseUrl, params = {}) {
    const url = new URL(baseUrl, window.location.origin);
    Object.keys(params).forEach(key => {
        if (params[key] !== null && params[key] !== undefined) {
            url.searchParams.append(key, params[key]);
        }
    });
    return url.pathname + url.search;
};

// Helper function to get API base URL from configuration
// Returns the base URL from appConfig (set in _Layout.cshtml from appsettings)
AppUrls.getApiBaseUrl = function() {
    // Priority: appConfig (from appsettings) > fallback to empty string (same origin)
    return window.authConfig?.apiBaseUrl || window.appConfig?.apiBaseUrl ;
};

// Helper function to build full API URL by prepending base URL to a path
// This ensures the API base URL from appsettings is always used
// USAGE: AppUrls.buildApiUrl(AppUrls.api.campaigns.list)
AppUrls.buildApiUrl = function(apiPath) {
    const baseUrl = AppUrls.getApiBaseUrl();
    
    // Handle function results (like AppUrls.api.campaigns.get(5))
    const path = typeof apiPath === 'function' ? apiPath() : apiPath;
    
    // Remove leading slash from path if present
    const cleanPath = path.startsWith('/') ? path.substring(1) : path;
    
    // If baseUrl is empty, just return the path with leading slash
    if (!baseUrl) {
        return '/' + cleanPath;
    }
    
    // Remove trailing slash from baseUrl if present
    const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.substring(0, baseUrl.length - 1) : baseUrl;
    
    // Combine base URL and path
    return cleanBaseUrl + '/' + cleanPath;
};

// Helper function to build full Hub URL
// SignalR hubs should NOT use buildApiUrl as they have different routing
AppUrls.buildHubUrl = function(hubPath) {
    const baseUrl = AppUrls.getApiBaseUrl();
    
    // Handle function results
    const path = typeof hubPath === 'function' ? hubPath() : hubPath;
    
    // Remove leading slash from path if present
    const cleanPath = path.startsWith('/') ? path.substring(1) : path;
    
    // If baseUrl is empty, just return the path with leading slash
    if (!baseUrl) {
        return '/' + cleanPath;
    }
    
    // Remove trailing slash from baseUrl if present
    const cleanBaseUrl = baseUrl.endsWith('/') ? baseUrl.substring(0, baseUrl.length - 1) : baseUrl;
    
    // Combine base URL and path
    return cleanBaseUrl + '/' + cleanPath;
};

// Make it available globally
if (typeof window !== 'undefined') {
    window.AppUrls = AppUrls;
}

// Export for module systems
if (typeof module !== 'undefined' && module.exports) {
    module.exports = AppUrls;
}

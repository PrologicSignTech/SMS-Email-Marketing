/**
 * Landing Footer - Dynamic Footer Loading from Database
 * Loads footer settings from the API and updates the footer content
 */

(function () {
    'use strict';

    // Wait for DOM ready
    document.addEventListener('DOMContentLoaded', function () {
        loadFooterSettings();
    });

    /**
     * Load footer settings from API
     */
    async function loadFooterSettings() {
        try {
            const apiBaseUrl = window.appConfig?.apiBaseUrl || '';
            const apiUrl = apiBaseUrl ? `${apiBaseUrl}/api/footersettings` : '/api/footersettings';

            console.log('Loading footer settings from:', apiUrl);

            const response = await fetch(apiUrl, { credentials: 'include' });
            const result = await response.json();

            if (result.success && result.data && result.data.isActive) {
                updateFooterContent(result.data);
            } else {
                console.log('No active footer settings found, using default content');
            }
        } catch (error) {
            console.error('Error loading footer settings:', error);
            // Keep default footer content on error
        }
    }

    /**
     * Update footer content from API data
     */
    function updateFooterContent(settings) {
        // Update company name
        const companyNameEl = document.querySelector('.footer-logo-text');
        if (companyNameEl && settings.companyName) {
            companyNameEl.textContent = settings.companyName;
        }

        // Update company description
        const descEl = document.querySelector('.footer-description');
        if (descEl && settings.companyDescription) {
            descEl.textContent = settings.companyDescription;
        }

        // Update social links
        updateSocialLinks(settings);

        // Update contact info
        updateContactInfo(settings);

        // Update map
        updateMap(settings);

        // Update newsletter section
        updateNewsletter(settings);

        // Update copyright
        const copyrightEl = document.querySelector('.footer-bottom .mb-0');
        if (copyrightEl && settings.copyrightText) {
            copyrightEl.innerHTML = settings.copyrightText;
        }
    }

    /**
     * Update social media links
     */
    function updateSocialLinks(settings) {
        const socialContainer = document.querySelector('.footer-social');
        if (!socialContainer) return;

        // Clear existing links
        socialContainer.innerHTML = '';

        // Add each social link if URL is provided
        const socialLinks = [
            { url: settings.facebookUrl, icon: 'bi-facebook', title: 'Facebook' },
            { url: settings.twitterUrl, icon: 'bi-twitter-x', title: 'Twitter' },
            { url: settings.linkedInUrl, icon: 'bi-linkedin', title: 'LinkedIn' },
            { url: settings.instagramUrl, icon: 'bi-instagram', title: 'Instagram' },
            { url: settings.youTubeUrl, icon: 'bi-youtube', title: 'YouTube' }
        ];

        socialLinks.forEach(link => {
            if (link.url) {
                const anchor = document.createElement('a');
                anchor.href = link.url;
                anchor.className = 'social-link';
                anchor.title = link.title;
                anchor.target = '_blank';
                anchor.rel = 'noopener noreferrer';
                anchor.innerHTML = `<i class="bi ${link.icon}"></i>`;
                socialContainer.appendChild(anchor);
            }
        });
    }

    /**
     * Update contact information
     */
    function updateContactInfo(settings) {
        const contactList = document.querySelector('.footer-contact');
        if (!contactList) return;

        // Build address HTML
        let addressHtml = '';
        if (settings.addressLine1) {
            addressHtml = settings.addressLine1;
            if (settings.addressLine2) {
                addressHtml += '<br>' + escapeHtml(settings.addressLine2);
            }
        }

        // Update address
        const addressItem = contactList.querySelector('li:first-child');
        if (addressItem && addressHtml) {
            const addressSpan = addressItem.querySelector('.contact-text span');
            if (addressSpan) {
                addressSpan.innerHTML = addressHtml;
            }
        }

        // Update phone
        if (settings.phone) {
            const phoneItem = contactList.querySelectorAll('li')[1];
            if (phoneItem) {
                const phoneSpan = phoneItem.querySelector('.contact-text span');
                if (phoneSpan) {
                    phoneSpan.innerHTML = `<a href="tel:${settings.phone.replace(/[^+\d]/g, '')}">${escapeHtml(settings.phone)}</a>`;
                }
            }
        }

        // Update email
        if (settings.email) {
            const emailItem = contactList.querySelectorAll('li')[2];
            if (emailItem) {
                const emailSpan = emailItem.querySelector('.contact-text span');
                if (emailSpan) {
                    emailSpan.innerHTML = `<a href="mailto:${settings.email}">${escapeHtml(settings.email)}</a>`;
                }
            }
        }

        // Update business hours
        if (settings.businessHours) {
            const hoursItem = contactList.querySelectorAll('li')[3];
            if (hoursItem) {
                const hoursSpan = hoursItem.querySelector('.contact-text span');
                if (hoursSpan) {
                    hoursSpan.textContent = settings.businessHours;
                }
            }
        }
    }

    /**
     * Update map section
     */
    function updateMap(settings) {
        const mapContainer = document.querySelector('.footer-map');
        if (!mapContainer) return;

        if (!settings.showMap) {
            mapContainer.style.display = 'none';
            return;
        }

        mapContainer.style.display = 'block';

        if (settings.mapEmbedUrl) {
            const iframe = mapContainer.querySelector('iframe');
            if (iframe) {
                iframe.src = settings.mapEmbedUrl;
            }
        }
    }

    /**
     * Update newsletter section
     */
    function updateNewsletter(settings) {
        const newsletterSection = document.querySelector('.footer-newsletter');
        if (!newsletterSection) return;

        if (!settings.showNewsletter) {
            newsletterSection.style.display = 'none';
            return;
        }

        newsletterSection.style.display = 'block';

        // Update newsletter title
        if (settings.newsletterTitle) {
            const titleEl = newsletterSection.querySelector('.newsletter-title');
            if (titleEl) {
                titleEl.textContent = settings.newsletterTitle;
            }
        }

        // Update newsletter description
        if (settings.newsletterDescription) {
            const descEl = newsletterSection.querySelector('.newsletter-text');
            if (descEl) {
                descEl.textContent = settings.newsletterDescription;
            }
        }
    }

    /**
     * Escape HTML to prevent XSS
     */
    function escapeHtml(text) {
        if (!text) return '';
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }

})();

/**
 * Landing Page Animations
 * Handles scroll animations, counters, and interactive effects
 */

(function() {
    'use strict';

    // Initialize animations on DOM load
    document.addEventListener('DOMContentLoaded', function() {
        initScrollAnimations();
        initFloatingMessages();
    });

    /**
     * Initialize scroll-based animations (AOS - Animate On Scroll alternative)
     */
    function initScrollAnimations() {
        const observerOptions = {
            threshold: 0.1,
            rootMargin: '0px 0px -100px 0px'
        };

        const observer = new IntersectionObserver(function(entries) {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('aos-animate');
                    
                    // Trigger counter animation when stats section is visible
                    if (entry.target.classList.contains('counter')) {
                        animateCounter(entry.target);
                    }
                }
            });
        }, observerOptions);

        // Observe all elements with data-aos attribute
        const elementsToAnimate = document.querySelectorAll('[data-aos]');
        elementsToAnimate.forEach(el => observer.observe(el));

        // Observe counter elements
        const counters = document.querySelectorAll('.counter');
        counters.forEach(counter => observer.observe(counter));
    }

    /**
     * Animate counter numbers
     */
    function animateCounter(element) {
        // Skip if already animated
        if (element.classList.contains('counted')) {
            return;
        }
        element.classList.add('counted');

        const target = parseInt(element.getAttribute('data-target'));
        const duration = 2000; // 2 seconds
        const step = target / (duration / 16); // 60fps
        let current = 0;

        const updateCounter = () => {
            current += step;
            if (current < target) {
                element.textContent = Math.floor(current).toLocaleString();
                requestAnimationFrame(updateCounter);
            } else {
                element.textContent = target.toLocaleString();
            }
        };

        updateCounter();
    }

    /**
     * Create floating message animations in hero section
     */
    function initFloatingMessages() {
        const heroWrapper = document.querySelector('.hero-image-wrapper');
        if (!heroWrapper) return;

        // Create animated message bubbles
        const messages = [
            { icon: 'envelope-fill', color: '#667eea', text: 'Email Sent!', delay: 0 },
            { icon: 'chat-dots-fill', color: '#10b981', text: 'SMS Delivered', delay: 2000 },
            { icon: 'megaphone-fill', color: '#f59e0b', text: 'Campaign Live', delay: 4000 }
        ];

        messages.forEach((msg, index) => {
            setTimeout(() => {
                createFloatingMessage(msg);
            }, msg.delay);
            
            // Repeat animation every 6 seconds
            setInterval(() => {
                setTimeout(() => {
                    createFloatingMessage(msg);
                }, msg.delay);
            }, 6000);
        });
    }

    /**
     * Create a floating message element
     */
    function createFloatingMessage(msgData) {
        const heroWrapper = document.querySelector('.hero-image-wrapper');
        if (!heroWrapper) return;

        const message = document.createElement('div');
        message.className = 'floating-message';
        message.innerHTML = `
            <i class="bi bi-${msgData.icon}" style="color: ${msgData.color}; font-size: 20px; margin-right: 10px;"></i>
            <span style="font-size: 14px; font-weight: 600; color: #1f2937;">${msgData.text}</span>
        `;
        
        // Random position
        const randomX = Math.random() * 80 + 10; // 10% to 90%
        const randomY = Math.random() * 80 + 10; // 10% to 90%
        
        message.style.cssText = `
            position: absolute;
            left: ${randomX}%;
            top: ${randomY}%;
            background: white;
            padding: 12px 20px;
            border-radius: 50px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.2);
            display: flex;
            align-items: center;
            white-space: nowrap;
            z-index: 10;
            animation: floatingMessageAnim 3s ease-out forwards;
        `;

        heroWrapper.appendChild(message);

        // Remove after animation
        setTimeout(() => {
            message.remove();
        }, 3000);
    }

    /**
     * Smooth scroll for anchor links
     */
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            if (href === '#') return;
            
            e.preventDefault();
            const target = document.querySelector(href);
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    /**
     * Parallax effect on scroll
     */
    window.addEventListener('scroll', function() {
        const scrolled = window.pageYOffset;
        const parallaxElements = document.querySelectorAll('.hero-bg-pattern, .stats-bg-pattern, .cta-bg-pattern');
        
        parallaxElements.forEach(el => {
            const speed = 0.5;
            el.style.transform = `translateY(${scrolled * speed}px)`;
        });

        // Floating elements parallax
        const floatingElements = document.querySelectorAll('.floating-element');
        floatingElements.forEach((el, index) => {
            const speed = 0.3 + (index * 0.1);
            el.style.transform = `translateY(${scrolled * speed}px)`;
        });
    });

    /**
     * Add ripple effect to buttons
     */
    document.querySelectorAll('.btn').forEach(button => {
        button.addEventListener('click', function(e) {
            const ripple = document.createElement('span');
            ripple.classList.add('ripple');
            
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;
            
            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            
            this.appendChild(ripple);
            
            setTimeout(() => ripple.remove(), 600);
        });
    });

})();

// Add floating message animation keyframes dynamically
const style = document.createElement('style');
style.textContent = `
    @keyframes floatingMessageAnim {
        0% {
            opacity: 0;
            transform: translateY(30px) scale(0.8);
        }
        20% {
            opacity: 1;
            transform: translateY(0) scale(1);
        }
        80% {
            opacity: 1;
            transform: translateY(-20px) scale(1);
        }
        100% {
            opacity: 0;
            transform: translateY(-60px) scale(0.9);
        }
    }

    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.6);
        transform: scale(0);
        animation: rippleEffect 0.6s ease-out;
        pointer-events: none;
    }

    @keyframes rippleEffect {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

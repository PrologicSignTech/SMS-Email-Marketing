/**
 * Sidebar Management Script
 * Handles sidebar toggle, collapse/expand, mobile menu, and state persistence
 */

(function() {
    'use strict';

    // Wait for DOM to be ready
    document.addEventListener('DOMContentLoaded', function() {
        initSidebar();
    });

    function initSidebar() {
        const sidebar = document.getElementById('sidebar');
        const sidebarToggle = document.getElementById('sidebarToggle');
        const mobileToggle = document.getElementById('mobileToggle');
        const navbarSidebarToggle = document.getElementById('navbarSidebarToggle');

        if (!sidebar) {
            console.warn('Sidebar element not found');
            return;
        }

        // Desktop sidebar toggle (inside sidebar header)
        if (sidebarToggle) {
            sidebarToggle.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleSidebar();
            });
        }

        // Navbar sidebar toggle (appears when sidebar is collapsed)
        if (navbarSidebarToggle) {
            navbarSidebarToggle.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleSidebar();
            });
        }

        // Mobile sidebar toggle
        if (mobileToggle) {
            mobileToggle.addEventListener('click', function(e) {
                e.preventDefault();
                e.stopPropagation();
                toggleMobileSidebar();
            });
        }

        // Restore sidebar state from localStorage
        restoreSidebarState();

        // Close mobile sidebar when clicking outside
        setupMobileClickOutside();

        // Setup menu item interactions
        setupMenuInteractions();

        // Highlight active menu item
        highlightActiveMenuItem();
    }

    function toggleSidebar() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        const isCollapsed = sidebar.classList.toggle('collapsed');

        // Save state to localStorage
        try {
            localStorage.setItem('sidebarCollapsed', isCollapsed.toString());
        } catch (e) {
            console.warn('localStorage not available:', e);
        }

        // Update icon
        const toggleIcon = document.querySelector('#sidebarToggle i');
        if (toggleIcon) {
            if (isCollapsed) {
                toggleIcon.className = 'bi bi-chevron-right';
            } else {
                toggleIcon.className = 'bi bi-list';
            }
        }

        // Close all expanded submenus when collapsing
        if (isCollapsed) {
            const openSubmenus = sidebar.querySelectorAll('.submenu.show');
            openSubmenus.forEach(submenu => {
                submenu.classList.remove('show');
            });
        }
    }

    function toggleMobileSidebar() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        sidebar.classList.toggle('show');
    }

    function restoreSidebarState() {
        const sidebar = document.getElementById('sidebar');
        if (!sidebar) return;

        try {
            const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
            if (isCollapsed) {
                sidebar.classList.add('collapsed');
                const toggleIcon = document.querySelector('#sidebarToggle i');
                if (toggleIcon) {
                    toggleIcon.className = 'bi bi-chevron-right';
                }
            }
        } catch (e) {
            console.warn('localStorage not available:', e);
        }
    }

    function setupMobileClickOutside() {
        document.addEventListener('click', function(event) {
            if (window.innerWidth > 768) return;

            const sidebar = document.getElementById('sidebar');
            const mobileToggle = document.getElementById('mobileToggle');

            if (!sidebar) return;

            const isClickInside = sidebar.contains(event.target);
            const isToggleClick = mobileToggle && mobileToggle.contains(event.target);

            if (!isClickInside && !isToggleClick && sidebar.classList.contains('show')) {
                sidebar.classList.remove('show');
            }
        });
    }

    function setupMenuInteractions() {
        // Handle submenu toggles
        const submenuToggles = document.querySelectorAll('[data-bs-toggle="collapse"]');

        submenuToggles.forEach(toggle => {
            toggle.addEventListener('click', function(e) {
                const sidebar = document.getElementById('sidebar');

                // If sidebar is collapsed on desktop, expand it first
                if (sidebar && sidebar.classList.contains('collapsed') && window.innerWidth > 768) {
                    e.preventDefault();
                    toggleSidebar();

                    // Then expand the submenu after a brief delay
                    setTimeout(() => {
                        const target = this.getAttribute('href');
                        if (target) {
                            const submenu = document.querySelector(target);
                            if (submenu) {
                                const bsCollapse = new bootstrap.Collapse(submenu, {
                                    toggle: true
                                });
                            }
                        }
                    }, 300);
                }
            });
        });

        // Handle menu item clicks - close mobile sidebar
        const menuLinks = document.querySelectorAll('.menu-link:not([data-bs-toggle])');
        menuLinks.forEach(link => {
            link.addEventListener('click', function() {
                if (window.innerWidth <= 768) {
                    const sidebar = document.getElementById('sidebar');
                    if (sidebar && sidebar.classList.contains('show')) {
                        sidebar.classList.remove('show');
                    }
                }
            });
        });
    }

    function highlightActiveMenuItem() {
        const currentPath = window.location.pathname.toLowerCase();
        const menuLinks = document.querySelectorAll('.sidebar .menu-link');

        menuLinks.forEach(link => {
            const href = link.getAttribute('href');
            if (href && currentPath.includes(href.toLowerCase())) {
                link.classList.add('active');

                // Expand parent submenu if exists
                const parentSubmenu = link.closest('.submenu');
                if (parentSubmenu) {
                    parentSubmenu.classList.add('show');
                    const parentToggle = document.querySelector(`[href="#${parentSubmenu.id}"]`);
                    if (parentToggle) {
                        parentToggle.setAttribute('aria-expanded', 'true');
                    }
                }
            }
        });
    }

    // Expose functions globally if needed
    window.sidebarManager = {
        toggle: toggleSidebar,
        toggleMobile: toggleMobileSidebar,
        highlight: highlightActiveMenuItem
    };
})();

/**
 * landing-testimonials.js - Load testimonials dynamically
 */

document.addEventListener('DOMContentLoaded', function() {
    loadTestimonials();
});

async function loadTestimonials() {
    const container = document.getElementById('testimonials-container');
    if (!container) return;

    try {
        const apiUrl = window.AppUrls ?
            window.AppUrls.buildApiUrl('/api/testimonials') :
            '/api/testimonials';

        const response = await fetch(apiUrl);
        const result = await response.json();

        if (result.success && result.data && result.data.length > 0) {
            // Show only first 6 testimonials
            const testimonials = result.data.slice(0, 6);

            container.innerHTML = testimonials.map((testimonial, index) => {
                const colors = ['primary', 'success', 'info', 'warning', 'danger', 'secondary'];
                const color = colors[index % colors.length];

                // Generate star rating
                const stars = Array.from({length: 5}, (_, i) => {
                    return i < testimonial.rating
                        ? '<i class="bi bi-star-fill text-warning"></i>'
                        : '<i class="bi bi-star text-warning"></i>';
                }).join('');

                return `
                    <div class="col-md-6 col-lg-4" data-aos="fade-up" data-aos-delay="${(index % 3) * 100}">
                        <div class="testimonial-card">
                            <div class="stars mb-3">
                                ${stars}
                            </div>
                            <p class="mb-4">"${escapeHtml(testimonial.testimonialText)}"</p>
                            <div class="d-flex align-items-center">
                                ${testimonial.avatarUrl ? `
                                    <img src="${testimonial.avatarUrl}"
                                         alt="${escapeHtml(testimonial.customerName)}"
                                         class="testimonial-avatar rounded-circle me-3"
                                         style="width: 48px; height: 48px; object-fit: cover;">
                                ` : `
                                    <div class="testimonial-avatar bg-${color} text-white">
                                        <i class="bi bi-person"></i>
                                    </div>
                                `}
                                <div>
                                    <div class="fw-bold">${escapeHtml(testimonial.customerName)}</div>
                                    <small class="text-muted">${testimonial.customerTitle ? escapeHtml(testimonial.customerTitle) + ', ' : ''}${escapeHtml(testimonial.companyName)}</small>
                                </div>
                            </div>
                        </div>
                    </div>
                `;
            }).join('');

            // Reinitialize AOS for new elements
            if (typeof AOS !== 'undefined') {
                AOS.refresh();
            }
        } else {
            container.innerHTML = '<div class="col-12"><p class="text-center text-muted">No testimonials available</p></div>';
        }
    } catch (error) {
        console.error('Error loading testimonials:', error);
        container.innerHTML = '<div class="col-12"><p class="text-center text-danger">Failed to load testimonials</p></div>';
    }
}

function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

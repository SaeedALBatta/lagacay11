// Legacy Eleven - Vanilla JavaScript Site Actions

document.addEventListener("DOMContentLoaded", function () {
    // 1. Gallery Thumbnail Swapper (Products/Details)
    const thumbnails = document.querySelectorAll(".product-thumbnail");
    const mainImg = document.getElementById("mainProductImage");

    if (thumbnails.length > 0 && mainImg) {
        thumbnails.forEach(function (thumb) {
            thumb.addEventListener("click", function () {
                const newSrc = this.getAttribute("data-src");
                if (newSrc) {
                    mainImg.src = newSrc;
                    
                    // Toggle active border class
                    thumbnails.forEach(function (t) {
                        t.classList.remove("border-primary");
                    });
                    this.classList.add("border-primary");
                }
            });
        });
    }

    // 2. Auto-hide Alerts after 5 seconds
    const alerts = document.querySelectorAll(".alert-dismissible");
    alerts.forEach(function (alert) {
        setTimeout(function () {
            // Check if bootstrap is loaded to use native fade
            if (typeof bootstrap !== 'undefined') {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            } else {
                // Fallback fade out
                alert.style.transition = "opacity 0.6s ease";
                alert.style.opacity = "0";
                setTimeout(function () {
                    alert.remove();
                }, 600);
            }
        }, 5000);
    });

    // 3. Invoice auto-print trigger
    const invoicePage = document.getElementById("invoicePrintPage");
    if (invoicePage) {
        window.print();
    }

    // 4. Header scroll effect
    const header = document.querySelector('header');
    if (header) {
        window.addEventListener('scroll', function () {
            if (window.scrollY > 20) {
                header.classList.add('bg-surface');
                header.classList.remove('bg-surface/80');
            } else {
                header.classList.remove('bg-surface');
                header.classList.add('bg-surface/80');
            }
        });
    }

    // 4b. Mobile menu toggle
    const mobileMenuToggle = document.getElementById("mobileMenuToggle");
    const mobileMenu = document.getElementById("mobileMenu");
    if (mobileMenuToggle && mobileMenu) {
        mobileMenuToggle.addEventListener("click", function () {
            mobileMenu.classList.toggle("hidden");
        });
    }

    // 5. Mousemove spotlight effect
    document.addEventListener('mousemove', function (e) {
        const spotlights = document.querySelectorAll('.spotlight-bg');
        spotlights.forEach(function (spot) {
            const rect = spot.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            spot.style.background = `radial-gradient(circle at ${x}px ${y}px, rgba(212, 175, 55, 0.15) 0%, transparent 70%)`;
        });
    });

    // 6. Countdown Timer Logic
    function updateCountdown() {
        const daysEl = document.getElementById('days');
        const hoursEl = document.getElementById('hours');
        if (!daysEl || !hoursEl) return;

        const now = new Date();
        let targetDate = new Date(now.getFullYear(), 11, 25); // Christmas/Dec 25
        if (now > targetDate) {
            targetDate.setFullYear(targetDate.getFullYear() + 1);
        }
        
        const diff = targetDate - now;
        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
        
        daysEl.textContent = days;
        hoursEl.textContent = hours.toString().padStart(2, '0');
    }
    
    if (document.getElementById('days')) {
        setInterval(updateCountdown, 60000);
        updateCountdown();
    }
});

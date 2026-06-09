document.addEventListener("DOMContentLoaded", () => {
    // ============================================
    // SCROLL: Header shrink
    // ============================================
    const header = document.getElementById('mainHeader');
    if (header) {
        window.addEventListener('scroll', () => {
            if (window.scrollY > 30) {
                header.classList.add('scrolled');
            } else {
                header.classList.remove('scrolled');
            }
        });
    }

    // ============================================
    // SEARCH TOGGLE (inline)
    // ============================================
    const searchWrapper = document.getElementById('searchWrapper');
    const searchToggle = document.getElementById('searchToggle');
    const searchInput = document.getElementById('searchInput');

    if (searchWrapper && searchToggle && searchInput) {
        searchToggle.addEventListener('click', (e) => {
            e.stopPropagation();
            if (searchWrapper.classList.contains('open')) {
                if (searchInput.value.trim() !== '') {
                    // Submit search form
                    searchWrapper.querySelector('form').submit();
                } else {
                    searchWrapper.classList.remove('open');
                }
            } else {
                searchWrapper.classList.add('open');
                setTimeout(() => searchInput.focus(), 300);
            }
        });

        document.addEventListener('click', (e) => {
            if (!searchWrapper.contains(e.target)) {
                searchWrapper.classList.remove('open');
            }
        });
    }

    // ============================================
    // COMMAND PALETTE (Ctrl+K)
    // ============================================
    const cmdOverlay = document.getElementById('commandPaletteOverlay');
    const cmdInput = document.getElementById('commandPaletteInput');

    function openCommandPalette() {
        if (cmdOverlay) {
            cmdOverlay.classList.add('open');
            setTimeout(() => {
                if (cmdInput) cmdInput.focus();
            }, 200);
        }
    }

    function closeCommandPalette() {
        if (cmdOverlay) {
            cmdOverlay.classList.remove('open');
            if (cmdInput) cmdInput.value = '';
        }
    }

    if (cmdOverlay) {
        cmdOverlay.addEventListener('click', (e) => {
            if (e.target === cmdOverlay) closeCommandPalette();
        });
    }

    document.addEventListener('keydown', (e) => {
        if ((e.ctrlKey || e.metaKey) && e.key === 'k') {
            e.preventDefault();
            if (cmdOverlay) {
                if (cmdOverlay.classList.contains('open')) {
                    closeCommandPalette();
                } else {
                    openCommandPalette();
                }
            }
        }
        if (e.key === 'Escape') {
            closeCommandPalette();
            closeMobileMenu();
        }
    });

    // ============================================
    // MOBILE MENU
    // ============================================
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const mobileMenu = document.getElementById('mobileMenu');
    const mobileOverlay = document.getElementById('mobileOverlay');
    const mobileMenuClose = document.getElementById('mobileMenuClose');

    function openMobileMenu() {
        if (mobileMenu && mobileOverlay && mobileMenuBtn) {
            mobileMenu.classList.add('open');
            mobileOverlay.classList.add('open');
            mobileMenuBtn.classList.add('open');
            document.body.style.overflow = 'hidden';
        }
    }

    function closeMobileMenu() {
        if (mobileMenu && mobileOverlay && mobileMenuBtn) {
            mobileMenu.classList.remove('open');
            mobileOverlay.classList.remove('open');
            mobileMenuBtn.classList.remove('open');
            document.body.style.overflow = '';
        }
    }

    if (mobileMenuBtn) mobileMenuBtn.addEventListener('click', openMobileMenu);
    if (mobileMenuClose) mobileMenuClose.addEventListener('click', closeMobileMenu);
    if (mobileOverlay) mobileOverlay.addEventListener('click', closeMobileMenu);

    // ============================================
    // ACTIVE LINK INDICATOR & SLIDER
    // ============================================
    const navIndicator = document.getElementById('navIndicatorLine');
    const navLinksContainer = document.getElementById('navLinks');
    const navLinkWrappers = document.querySelectorAll('.nav-link-wrapper');

    function moveIndicator(element) {
        if (!navIndicator || !navLinksContainer) return;
        const link = element.querySelector('.nav-link');
        if (!link) return;
        const rect = link.getBoundingClientRect();
        const containerRect = navLinksContainer.getBoundingClientRect();

        navIndicator.style.width = rect.width + 'px';
        navIndicator.style.left = (rect.left - containerRect.left) + 'px';
        navIndicator.style.opacity = '1';
    }

    function hideIndicator() {
        if (!navIndicator) return;
        let activeWrapper = null;
        navLinkWrappers.forEach(w => {
            if (w.querySelector('.active')) {
                activeWrapper = w;
            }
        });

        if (activeWrapper) {
            moveIndicator(activeWrapper);
        } else {
            navIndicator.style.opacity = '0';
        }
    }

    navLinkWrappers.forEach(wrapper => {
        wrapper.addEventListener('mouseenter', () => moveIndicator(wrapper));
        wrapper.addEventListener('click', () => {
            navLinkWrappers.forEach(w => {
                const lnk = w.querySelector('.nav-link');
                if (lnk) lnk.classList.remove('active');
            });
            const mainLnk = wrapper.querySelector('.nav-link');
            if (mainLnk) mainLnk.classList.add('active');
        });
    });

    if (navLinksContainer) {
        navLinksContainer.addEventListener('mouseleave', hideIndicator);
        window.addEventListener('load', hideIndicator);
        window.addEventListener('resize', hideIndicator);
        
        // Run once loaded
        setTimeout(hideIndicator, 100);
    }

    // ============================================
    // CLEAR CART ACTION
    // ============================================
    const clearCartBtn = document.getElementById('clearCartBtn');
    if (clearCartBtn) {
        clearCartBtn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            const clearForm = document.getElementById('clearCartForm');
            if (clearForm) {
                clearForm.submit();
            }
        });
    }
});

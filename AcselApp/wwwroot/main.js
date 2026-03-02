document.addEventListener('DOMContentLoaded', () => {
    // 1. Reduced Motion Check
    const prefersReducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    // 2. Navbar Scroll Logic
    const navbar = document.getElementById('navbar');
    const hero = document.querySelector('.hero');

    if (hero && navbar) {
        const navObserver = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) {
                    navbar.classList.add('scrolled');
                } else {
                    navbar.classList.remove('scrolled');
                }
            });
        }, { threshold: 0.1 });
        navObserver.observe(hero);
    }

    // 3. Mobile Menu Auto-close on link click
    const navCheckbox = document.getElementById('nav-toggle');
    const navLinks = document.querySelectorAll('.nav-link, .nav-cta');
    navLinks.forEach(link => {
        link.addEventListener('click', () => {
            if (navCheckbox) navCheckbox.checked = false;
        });
    });

    // 4. Entrance Animations
    if (!prefersReducedMotion) {
        // Hero animation trigger
        if (hero) hero.classList.add('animate');

        // Scroll reveal observers
        const scrollElements = document.querySelectorAll('.animate-on-scroll');
        const scrollObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('visible');
                    observer.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

        scrollElements.forEach(el => scrollObserver.observe(el));
    } else {
        // For reduced motion, ensure everything starts visible
        if (hero) hero.classList.add('animate', 'visible');
        document.querySelectorAll('.animate-on-scroll').forEach(el => el.classList.add('visible'));
    }

    // 5. Parallax Background
    if (!prefersReducedMotion) {
        const bg = document.getElementById('parallax-bg');
        const section = bg ? bg.parentElement : null;
        if (bg && section) {
            window.addEventListener('scroll', () => {
                const rect = section.getBoundingClientRect();
                const windowHeight = window.innerHeight;
                // Only animate if in view
                if (rect.top <= windowHeight && rect.bottom >= 0) {
                    const scrollProgress = 1 - (rect.bottom / (windowHeight + rect.height));
                    // Map scroll progress (0 to 1) to translateY (-50px to 50px)
                    const yOffset = (scrollProgress - 0.5) * 150;
                    bg.style.transform = `translateY(${yOffset}px)`;
                }
            }, { passive: true });
        }
    }

    // 6. Diagnostic Shuffler Card
    const shufflerContainer = document.getElementById('shuffler-container');
    if (shufflerContainer && !prefersReducedMotion) {
        const items = Array.from(shufflerContainer.children);

        const updateStackStyles = () => {
            items.forEach((item, index) => {
                item.style.zIndex = items.length - index;
                item.style.transform = `translateY(${index * 15}px) scale(${1 - index * 0.05})`;
                item.style.opacity = index < 3 ? 1 - index * 0.3 : 0;
            });
        };

        // Initial setup
        updateStackStyles();

        setInterval(() => {
            // Unshift pop style shuffle
            const last = items.pop();
            items.unshift(last);
            updateStackStyles();
        }, 3000);
    } else if (shufflerContainer) {
        // Static layout for reduced motion
        const items = Array.from(shufflerContainer.children);
        items.forEach((item, index) => {
            item.style.position = 'relative';
            item.style.marginBottom = '0.5rem';
        });
    }

    // 7. Telemetry Typewriter Card
    const textEl = document.getElementById('typing-text');
    if (textEl && !prefersReducedMotion) {
        const messages = [
            "Initializing protocol...",
            "Loading speaker bios...",
            "Expert: Dr. A. Reynolds (MIT)",
            "System online: Safety First.",
            "Analyzing lab environments...",
            "Decoding best practices."
        ];
        let msgIndex = 0;
        let charIndex = 0;
        let isDeleting = false;

        function typeWriter() {
            const currentMsg = messages[msgIndex];

            if (isDeleting) {
                textEl.textContent = currentMsg.substring(0, charIndex - 1);
                charIndex--;
            } else {
                textEl.textContent = currentMsg.substring(0, charIndex + 1);
                charIndex++;
            }

            let typeSpeed = isDeleting ? 30 : 70;
            if (!isDeleting && charIndex === currentMsg.length) {
                typeSpeed = 2000;
                isDeleting = true;
            } else if (isDeleting && charIndex === 0) {
                isDeleting = false;
                msgIndex = (msgIndex + 1) % messages.length;
                typeSpeed = 500;
            }
            setTimeout(typeWriter, typeSpeed);
        }
        setTimeout(typeWriter, 1000);
    } else if (textEl) {
        textEl.textContent = "System online: Safety First. Loading speaker bios...";
    }

    // 8. Cursor Protocol Scheduler Card
    const schedulerGrid = document.getElementById('scheduler-grid');
    const cursor = document.getElementById('scheduler-cursor');
    if (schedulerGrid && cursor && !prefersReducedMotion) {
        // Generate grid cells (7 columns x 5 rows)
        const cells = [];
        for (let i = 0; i < 35; i++) {
            const cell = document.createElement('div');
            cell.className = 'grid-cell';
            schedulerGrid.appendChild(cell);
            cells.push(cell);
        }

        // Pre-activate target cells for visual (e.g. 19-20 Nov shape)
        const targetIndices = [18, 19];

        const runSchedulerAnimation = async () => {
            // Reset
            cells.forEach(c => c.classList.remove('active'));
            cursor.style.transition = 'none';
            cursor.style.transform = `translate(0px, 0px)`;
            cursor.style.opacity = '0';

            await new Promise(r => setTimeout(r, 500));
            // Cursor entry
            cursor.style.transition = 'all 0.8s var(--ease-spring)';
            cursor.style.opacity = '1';

            for (let target of targetIndices) {
                const cell = cells[target];
                const cellRect = cell.getBoundingClientRect();
                const containerRect = schedulerGrid.getBoundingClientRect();

                const tx = cellRect.left - containerRect.left + (cellRect.width / 2) - 16;
                const ty = cellRect.top - containerRect.top + (cellRect.height / 2) - 16;

                // Move
                cursor.style.transform = `translate(${tx}px, ${ty}px)`;
                await new Promise(r => setTimeout(r, 800));

                // Click interaction
                cursor.style.transform = `translate(${tx}px, ${ty}px) scale(0.85)`;
                await new Promise(r => setTimeout(r, 150));

                cell.classList.add('active');
                cursor.style.transform = `translate(${tx}px, ${ty}px) scale(1)`;

                await new Promise(r => setTimeout(r, 400));
            }

            // Move away and fade
            cursor.style.transform = `translate(100%, 100%) scale(1)`;
            cursor.style.opacity = '0';

            // Loop
            setTimeout(runSchedulerAnimation, 3000);
        };

        // Start animation when card comes into view
        const schedulerObserver = new IntersectionObserver(entries => {
            if (entries[0].isIntersecting) {
                runSchedulerAnimation();
                schedulerObserver.disconnect();
            }
        });
        schedulerObserver.observe(schedulerGrid);

    } else if (schedulerGrid) {
        // Fallback for no JS or prefers-reduced-motion
        for (let i = 0; i < 35; i++) {
            const cell = document.createElement('div');
            cell.className = 'grid-cell';
            if (i === 18 || i === 19) cell.classList.add('active');
            schedulerGrid.appendChild(cell);
        }
        if (cursor) cursor.style.display = 'none';
    }

});

(() => {
    'use strict';

    const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;
    const anime = window.anime;

    if (!anime || prefersReducedMotion) {
        document.documentElement.classList.add('access-motion-ready');
        return;
    }

    document.documentElement.classList.add('access-anime-enabled');

    const qsa = (selector, scope = document) => Array.from(scope.querySelectorAll(selector));

    const splitBrandTitles = () => {
        qsa('.brand-title').forEach((title) => {
            if (title.querySelector('.brand-title__line')) return;

            const parts = title.innerHTML
                .split(/<br\s*\/?\s*>/i)
                .map((part) => part.replace(/<[^>]*>/g, '').trim())
                .filter(Boolean);

            const lines = parts.length > 1 ? parts : title.textContent.trim().split(/\s+(?=\S+$)/);

            title.textContent = '';
            lines.forEach((line) => {
                const span = document.createElement('span');
                span.className = 'brand-title__line';
                span.textContent = line;
                title.appendChild(span);
            });
        });
    };

    const splitTypingLines = () => {
        qsa('.brand-title__line--typing').forEach((line) => {
            if (line.dataset.typingReady === 'true') return;

            const text = line.dataset.typingText || line.textContent.trim();
            line.dataset.typingReady = 'true';
            line.textContent = '';

            Array.from(text).forEach((char) => {
                const span = document.createElement('span');
                span.className = 'brand-title__char';
                span.textContent = char === ' ' ? '\u00A0' : char;
                line.appendChild(span);
            });

            const cursor = document.createElement('span');
            cursor.className = 'brand-title__cursor';
            cursor.setAttribute('aria-hidden', 'true');
            line.appendChild(cursor);
        });
    };

    const setInitialState = () => {
        const targets = qsa([
            '.institutional-logos',
            '.brand-kicker',
            '.brand-title__line:not(.brand-title__line--typing)',
            '.brand-title__char',
            '.brand-title__cursor',
            '.title-rule',
            '.brand-text',
            '.feature-item',
            '.auth-panel',
            '.lock-badge',
            '.login-title',
            '.login-subtitle',
            '.login-form .mb-3',
            '.login-alert',
            '.login-submit',
            '.login-links',
            '.access-note',
            '.expired-alert',
            '.security-alert-card',
            '.security-countdown',
            '.recovery-success'
        ].join(','));

        anime.set(targets, {
            opacity: 0,
            translateY: 18
        });

        anime.set('.title-rule', {
            scaleX: 0,
            transformOrigin: 'left center'
        });

        anime.set('.auth-panel', {
            translateY: 28,
            scale: 0.985
        });

        anime.set('.lock-badge', {
            scale: 0.88,
            rotate: -3
        });
    };

    const animateEntrance = () => {
        const timeline = anime.timeline({
            easing: 'easeOutExpo',
            duration: 760,
            complete: () => document.documentElement.classList.add('access-motion-ready')
        });

        timeline
            .add({
                targets: '.institutional-logos',
                opacity: [0, 1],
                translateY: [-10, 0],
                duration: 620
            })
            .add({
                targets: '.brand-kicker',
                opacity: [0, 1],
                translateY: [14, 0],
                duration: 640
            }, '-=360')
            .add({
                targets: '.brand-title__line--typing',
                opacity: [0, 1],
                translateY: [18, 0],
                filter: ['blur(6px)', 'blur(0px)'],
                duration: 360
            }, '-=280')
            .add({
                targets: '.brand-title__line--typing .brand-title__cursor',
                opacity: [0, 1],
                duration: 160
            }, '-=140')
            .add({
                targets: '.brand-title__line--typing .brand-title__char',
                opacity: [0, 1],
                translateY: ['0.24em', 0],
                filter: ['blur(4px)', 'blur(0px)'],
                delay: anime.stagger(42),
                duration: 260,
                easing: 'easeOutCubic'
            }, '-=80')
            .add({
                targets: '.brand-title__line:not(.brand-title__line--typing)',
                opacity: [0, 1],
                translateY: [34, 0],
                filter: ['blur(8px)', 'blur(0px)'],
                delay: anime.stagger(115),
                duration: 780
            }, '-=120')
            .add({
                targets: '.brand-title__line--typing .brand-title__cursor',
                opacity: [1, 0, 1, 0],
                duration: 920,
                easing: 'steps(1)'
            }, '-=520')
            .add({
                targets: '.title-rule',
                opacity: [0, 1],
                scaleX: [0, 1],
                translateY: [0, 0],
                duration: 680
            }, '-=560')
            .add({
                targets: '.brand-text',
                opacity: [0, 1],
                translateY: [18, 0],
                duration: 660
            }, '-=420')
            .add({
                targets: '.feature-item',
                opacity: [0, 1],
                translateY: [18, 0],
                delay: anime.stagger(80),
                duration: 620
            }, '-=420')
            .add({
                targets: '.auth-panel',
                opacity: [0, 1],
                translateY: [30, 0],
                scale: [0.985, 1],
                duration: 840
            }, '-=1040')
            .add({
                targets: '.lock-badge',
                opacity: [0, 1],
                scale: [0.88, 1],
                rotate: [-3, 0],
                duration: 680
            }, '-=500')
            .add({
                targets: '.login-title, .login-subtitle, .expired-alert, .security-alert-card, .security-countdown, .recovery-success',
                opacity: [0, 1],
                translateY: [14, 0],
                delay: anime.stagger(70),
                duration: 560
            }, '-=470')
            .add({
                targets: '.login-form .mb-3, .login-alert, .login-submit, .login-links, .access-note',
                opacity: [0, 1],
                translateY: [14, 0],
                delay: anime.stagger(65),
                duration: 560
            }, '-=430');
    };

    const animateInteractions = () => {
        qsa('.login-input').forEach((inputWrap) => {
            inputWrap.addEventListener('focusin', () => {
                anime.remove(inputWrap);
                anime({
                    targets: inputWrap,
                    scale: 1.012,
                    boxShadow: '0 14px 28px rgba(106, 8, 42, 0.10)',
                    duration: 360,
                    easing: 'easeOutCubic'
                });
            });

            inputWrap.addEventListener('focusout', () => {
                anime.remove(inputWrap);
                anime({
                    targets: inputWrap,
                    scale: 1,
                    boxShadow: '0 0 0 rgba(106, 8, 42, 0)',
                    duration: 360,
                    easing: 'easeOutCubic'
                });
            });
        });

        qsa('.feature-icon, .lock-badge').forEach((icon) => {
            anime({
                targets: icon,
                translateY: [0, -5, 0],
                duration: 4200,
                delay: anime.random(0, 1200),
                loop: true,
                easing: 'easeInOutSine'
            });
        });

        qsa('.login-submit').forEach((button) => {
            button.addEventListener('mouseenter', () => {
                anime.remove(button);
                anime({
                    targets: button,
                    translateY: -2,
                    scale: 1.01,
                    duration: 260,
                    easing: 'easeOutCubic'
                });
            });

            button.addEventListener('mouseleave', () => {
                anime.remove(button);
                anime({
                    targets: button,
                    translateY: 0,
                    scale: 1,
                    duration: 260,
                    easing: 'easeOutCubic'
                });
            });
        });
    };

    const animateOnSubmit = () => {
        qsa('.login-form').forEach((form) => {
            form.addEventListener('submit', () => {
                const button = form.querySelector('.login-submit');
                if (!button) return;

                anime.remove(button);
                anime({
                    targets: button,
                    scale: [1, 0.985, 1],
                    duration: 420,
                    easing: 'easeOutCubic'
                });
            });
        });
    };

    const init = () => {
        splitBrandTitles();
        splitTypingLines();
        setInitialState();
        requestAnimationFrame(() => {
            animateEntrance();
            animateInteractions();
            animateOnSubmit();
        });
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init, { once: true });
    } else {
        init();
    }
})();

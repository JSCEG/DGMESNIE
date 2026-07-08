(() => {
    'use strict';

    const anime = window.anime;
    const prefersReducedMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

    if (!anime || prefersReducedMotion) {
        document.documentElement.classList.add('gestor-motion-ready');
        return;
    }

    const qsa = (selector, scope = document) => Array.from(scope.querySelectorAll(selector));
    const visible = (element) => !!element && !element.hidden && element.offsetParent !== null;

    let initialAnimated = false;
    let activeViewAnimation = null;

    const getEntranceTargets = () => {
        const activeView = document.querySelector('.gestor-view.active:not([hidden])');
        const stable = qsa([
            '.gestor-page__view-header',
            '.gestor-nav',
            '.gestor-breadcrumbs',
            '.global-filter-bar'
        ].join(',')).filter(visible);

        const viewTargets = activeView
            ? qsa([
                '.dashboard-kpi-card',
                '.dashboard-critical-panel',
                '.dashboard-panel',
                '.panel:not(.dashboard-panel):not(.dashboard-critical-panel)',
                '.internal-toolbar',
                '.toolbar',
                '.chart-box',
                '.table-responsive',
                '.table-wrap',
                'table',
                '.kanban-board',
                '.gantt-wrapper',
                '.calendar-grid',
                '.internal-report-deck',
                '.report-card',
                '.responsable-card'
            ].join(','), activeView).filter(visible)
            : [];

        return { stable, viewTargets };
    };

    const setInitial = (elements) => {
        anime.set(elements, {
            opacity: 0,
            translateY: 22,
            scale: 0.985
        });
    };

    const animateInitialEntrance = () => {
        if (initialAnimated) return;
        initialAnimated = true;

        const { stable, viewTargets } = getEntranceTargets();
        const tabs = qsa('.gestor-tab').filter(visible);
        const allTargets = [...stable, ...tabs, ...viewTargets];

        if (!allTargets.length) return;

        setInitial(allTargets);

        anime.timeline({
            easing: 'easeOutExpo',
            complete: () => document.documentElement.classList.add('gestor-motion-ready')
        })
            .add({
                targets: stable,
                opacity: [0, 1],
                translateY: [22, 0],
                scale: [0.985, 1],
                delay: anime.stagger(75),
                duration: 760
            })
            .add({
                targets: tabs,
                opacity: [0, 1],
                translateY: [14, 0],
                scale: [0.98, 1],
                delay: anime.stagger(38),
                duration: 520
            }, '-=470')
            .add({
                targets: viewTargets,
                opacity: [0, 1],
                translateY: [26, 0],
                scale: [0.985, 1],
                delay: anime.stagger(58, { grid: [3, 8], from: 'first' }),
                duration: 720
            }, '-=300');
    };

    const animateActiveView = () => {
        const activeView = document.querySelector('.gestor-view.active:not([hidden])');
        if (!activeView) return;

        const targets = qsa([
            '.dashboard-kpi-card',
            '.dashboard-critical-panel',
            '.dashboard-panel',
            '.panel:not(.dashboard-panel):not(.dashboard-critical-panel)',
            '.internal-toolbar',
            '.toolbar',
            '.table-responsive',
            '.table-wrap',
            'table',
            '.kanban-board',
            '.gantt-wrapper',
            '.calendar-grid',
            '.internal-report-deck',
            '.report-card',
            '.responsable-card'
        ].join(','), activeView).filter(visible).slice(0, 32);

        if (!targets.length) return;

        if (activeViewAnimation) activeViewAnimation.pause();
        anime.remove(targets);
        anime.set(targets, {
            opacity: 0,
            translateY: 18,
            scale: 0.988
        });

        activeViewAnimation = anime({
            targets,
            opacity: [0, 1],
            translateY: [18, 0],
            scale: [0.988, 1],
            delay: anime.stagger(45),
            duration: 620,
            easing: 'easeOutExpo'
        });
    };

    const animateTabPress = (tab) => {
        anime.remove(tab);
        anime({
            targets: tab,
            scale: [1, 0.965, 1],
            translateY: [0, 1, 0],
            duration: 360,
            easing: 'easeOutCubic'
        });
    };

    const bindInteractions = () => {
        qsa('.gestor-tab').forEach((tab) => {
            tab.addEventListener('click', () => {
                animateTabPress(tab);
                window.setTimeout(animateActiveView, 80);
            });
        });

        qsa('.dashboard-kpi-card, .dashboard-panel, .panel').forEach((card) => {
            card.addEventListener('mouseenter', () => {
                anime.remove(card);
                anime({
                    targets: card,
                    translateY: -3,
                    duration: 260,
                    easing: 'easeOutCubic'
                });
            });

            card.addEventListener('mouseleave', () => {
                anime.remove(card);
                anime({
                    targets: card,
                    translateY: 0,
                    duration: 260,
                    easing: 'easeOutCubic'
                });
            });
        });
    };

    const watchPreloader = () => {
        const preloader = document.getElementById('tracking-preloader');
        if (!preloader) {
            window.setTimeout(animateInitialEntrance, 180);
            return;
        }

        if (preloader.classList.contains('is-hidden') || preloader.hidden) {
            window.setTimeout(animateInitialEntrance, 120);
            return;
        }

        const observer = new MutationObserver(() => {
            if (preloader.classList.contains('is-hidden') || preloader.hidden) {
                observer.disconnect();
                window.setTimeout(animateInitialEntrance, 160);
            }
        });

        observer.observe(preloader, {
            attributes: true,
            attributeFilter: ['class', 'hidden', 'style']
        });

        window.setTimeout(animateInitialEntrance, 1800);
    };

    const init = () => {
        document.documentElement.classList.add('gestor-anime-enabled');
        bindInteractions();
        watchPreloader();
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init, { once: true });
    } else {
        init();
    }
})();

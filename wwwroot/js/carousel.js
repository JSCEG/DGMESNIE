document.addEventListener('DOMContentLoaded', function() {
    const carousels = document.querySelectorAll('.carousel-container');

    carousels.forEach(carousel => {
        const track = carousel.querySelector('.carousel-track');
        const cards = carousel.querySelectorAll('.carousel-card');
        const prev = carousel.querySelector('.prev');
        const next = carousel.querySelector('.next');
        
        // Clone items for infinite scroll
        cards.forEach(card => {
            const clone = card.cloneNode(true);
            track.appendChild(clone);
        });

        let currentIndex = 0;
        const cardWidth = cards[0].offsetWidth + 20; // Including gap
        let isTransitioning = false;

        function updateActiveCards() {
            const visibleCards = Math.floor(carousel.offsetWidth / cardWidth);
            cards.forEach((card, index) => {
                card.classList.remove('active');
                if (index >= currentIndex && index < currentIndex + visibleCards) {
                    card.classList.add('active');
                }
            });
        }

        function slideNext() {
            if (isTransitioning) return;
            isTransitioning = true;
            currentIndex++;
            track.style.transform = `translateX(-${currentIndex * cardWidth}px)`;
            
            // Reset position if we reached the cloned section
            if (currentIndex >= cards.length) {
                setTimeout(() => {
                    track.style.transition = 'none';
                    currentIndex = 0;
                    track.style.transform = `translateX(0)`;
                    setTimeout(() => {
                        track.style.transition = 'transform 0.5s ease-in-out';
                    }, 10);
                }, 500);
            }
            
            setTimeout(() => {
                isTransitioning = false;
                updateActiveCards();
            }, 500);
        }

        function slidePrev() {
            if (isTransitioning) return;
            isTransitioning = true;
            currentIndex--;
            
            if (currentIndex < 0) {
                currentIndex = cards.length - 1;
                track.style.transition = 'none';
                track.style.transform = `translateX(-${(cards.length) * cardWidth}px)`;
                setTimeout(() => {
                    track.style.transition = 'transform 0.5s ease-in-out';
                    track.style.transform = `translateX(-${currentIndex * cardWidth}px)`;
                }, 10);
            } else {
                track.style.transform = `translateX(-${currentIndex * cardWidth}px)`;
            }
            
            setTimeout(() => {
                isTransitioning = false;
                updateActiveCards();
            }, 500);
        }

        // Event Listeners
        next.addEventListener('click', slideNext);
        prev.addEventListener('click', slidePrev);

        // Auto slide every 5 seconds
        setInterval(slideNext, 5000);

        // Initial active cards
        updateActiveCards();

        // Update on resize
        window.addEventListener('resize', updateActiveCards);
    });
});
document.addEventListener('DOMContentLoaded', () => {
    const header = document.querySelector('.header-custom');

    if (header) {
        const handleScroll = () => {
            if (window.scrollY > 50) { // Activa el efecto después de 50px de scroll
                header.classList.add('header-scrolled');
            } else {
                header.classList.remove('header-scrolled');
            }
        };

        window.addEventListener('scroll', handleScroll);
    }
});

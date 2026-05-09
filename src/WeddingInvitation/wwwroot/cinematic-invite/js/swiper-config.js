/**
 * Swiper — main story reel.
 * Backgrounds use native <img loading="lazy"> (Swiper 11 bundle does not apply lazy to data-background reliably).
 */
(function () {
    window.CinematicSwiper = {
        initMain: function () {
            var el = document.querySelector("#cin-main-swiper");
            if (!el || typeof Swiper === "undefined") {
                return null;
            }
            return new Swiper(el, {
                effect: "fade",
                fadeEffect: {
                    crossFade: true
                },
                speed: 1080,
                slidesPerView: 1,
                spaceBetween: 0,
                grabCursor: true,
                resistanceRatio: 0.52,
                keyboard: { enabled: true },
                watchSlidesProgress: true,
                touchEventsTarget: "container",
                shortSwipes: true,
                longSwipesRatio: 0.34
            });
        }
    };
})();

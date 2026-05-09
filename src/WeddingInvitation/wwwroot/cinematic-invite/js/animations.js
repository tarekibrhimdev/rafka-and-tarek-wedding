/**
 * Cinematic scene reveals — GSAP timelines per slide (slow, editorial).
 */
(function () {
    var reduced =
        typeof window.matchMedia === "function" &&
        window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    function revealSlide(slide) {
        if (!slide) {
            return;
        }

        var anim = slide.querySelectorAll("[data-animate]");
        var kids = slide.querySelectorAll("[data-animate-child]");
        var nodes = [].slice.call(anim).concat([].slice.call(kids));

        if (!nodes.length) {
            return;
        }

        if (typeof window.gsap === "undefined") {
            nodes.forEach(function (el) {
                el.style.opacity = "1";
                el.style.filter = "none";
                el.style.transform = "none";
            });
            return;
        }

        if (reduced) {
            window.gsap.set(nodes, { clearProps: "all", opacity: 1, y: 0, filter: "none" });
            return;
        }

        window.gsap.killTweensOf(nodes);

        window.gsap.set(nodes, { opacity: 0, y: 26, filter: "blur(10px)" });

        var tl = window.gsap.timeline({ defaults: { ease: "power3.out" } });
        if (anim.length) {
            tl.to(anim, {
                opacity: 1,
                y: 0,
                filter: "blur(0px)",
                duration: 0.78,
                stagger: 0.09
            });
        }
        if (kids.length) {
            tl.to(
                kids,
                {
                    opacity: 1,
                    y: 0,
                    filter: "blur(0px)",
                    duration: 0.68,
                    stagger: 0.11
                },
                anim.length ? "-=0.45" : 0
            );
        }

        /* If a transition event is missed (mobile / fast swipe), never leave copy stuck invisible */
        window.setTimeout(function () {
            nodes.forEach(function (el) {
                var o = parseFloat(window.getComputedStyle(el).opacity);
                if (isNaN(o) || o < 0.92) {
                    window.gsap.set(el, { opacity: 1, y: 0, filter: "none", clearProps: "transform" });
                }
            });
        }, 1600);
    }

    window.CinematicAnimations = {
        /**
         * Re-run reveal for whatever slide is active (e.g. after copy/i18n updates).
         * @param {object|null} swiper
         */
        revealActive: function (swiper) {
            if (!swiper || !swiper.slides || swiper.activeIndex < 0) {
                return;
            }
            revealSlide(swiper.slides[swiper.activeIndex]);
        },

        /**
         * @param {object|null} swiper
         */
        init: function (swiper) {
            if (!swiper) {
                return;
            }
            var fallbackMs = 1300;
            if (swiper.params && typeof swiper.params.speed === "number") {
                fallbackMs = swiper.params.speed + 420;
            }
            var fallbackTimer = null;

            function clearFallback() {
                if (fallbackTimer !== null) {
                    window.clearTimeout(fallbackTimer);
                    fallbackTimer = null;
                }
            }

            var run = function () {
                clearFallback();
                revealSlide(swiper.slides[swiper.activeIndex]);
            };

            swiper.on("slideChange", function () {
                clearFallback();
                var idx = swiper.activeIndex;
                fallbackTimer = window.setTimeout(function () {
                    fallbackTimer = null;
                    if (swiper.destroyed || swiper.activeIndex !== idx) {
                        return;
                    }
                    revealSlide(swiper.slides[idx]);
                }, fallbackMs);
            });

            swiper.on("slideChangeTransitionEnd", run);

            /* First slide: reveal immediately (CSS leaves [data-animate] at opacity:0 until this runs). */
            revealSlide(swiper.slides[swiper.activeIndex]);
        }
    };
})();

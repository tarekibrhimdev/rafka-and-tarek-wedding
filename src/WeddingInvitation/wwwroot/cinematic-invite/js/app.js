/**
 * Orchestration: progress UI, navigation, loader, RSVP.
 */
(function () {
    function buildProgress(count) {
        var host = document.getElementById("cin-progress");
        if (!host) {
            return;
        }
        host.innerHTML = "";
        for (var i = 0; i < count; i++) {
            var seg = document.createElement("div");
            seg.className = "cin-progress__seg";
            var fill = document.createElement("span");
            fill.className = "cin-progress__fill";
            seg.appendChild(fill);
            host.appendChild(seg);
        }
    }

    function updateProgress(index) {
        var segs = document.querySelectorAll(".cin-progress__seg");
        segs.forEach(function (seg, idx) {
            seg.classList.toggle("is-complete", idx < index);
            seg.classList.toggle("is-current", idx === index);
        });
    }

    function hideLoader() {
        var el = document.getElementById("cin-loader");
        if (!el) {
            return;
        }
        var finish = function () {
            el.classList.add("is-done");
            el.setAttribute("aria-busy", "false");
        };
        if (typeof window.gsap !== "undefined") {
            el.style.pointerEvents = "none";
            window.gsap.to(el, {
                opacity: 0,
                duration: 0.88,
                ease: "power2.inOut",
                onComplete: finish
            });
        } else {
            el.style.pointerEvents = "none";
            el.style.opacity = "0";
            setTimeout(finish, 80);
        }
    }

    function bindChromeNav(mainSwiper) {
        var prev = document.getElementById("cin-prev");
        var next = document.getElementById("cin-next");
        if (!mainSwiper) {
            return;
        }
        var syncDisabled = function () {
            if (prev) {
                prev.disabled = mainSwiper.isBeginning;
            }
            if (next) {
                next.disabled = mainSwiper.isEnd;
            }
        };
        if (prev) {
            prev.addEventListener("click", function () {
                mainSwiper.slidePrev();
            });
        }
        if (next) {
            next.addEventListener("click", function () {
                mainSwiper.slideNext();
            });
        }
        mainSwiper.on("slideChange", syncDisabled);
        syncDisabled();
    }

    function initRsvp() {
        var form = document.getElementById("cin-rsvp-form");
        var root = document.getElementById("cin-rsvp");
        var ok = document.getElementById("cin-rsvp-success");
        if (!form || !root || !ok) {
            return;
        }

        var partyFieldset = form.querySelector("[data-cin-rsvp-party]");
        var clientErr = document.querySelector("[data-cin-rsvp-client-err]");
        var valBox = document.querySelector("[data-cin-rsvp-validation]");
        var isDemo = form.hasAttribute("data-cin-rsvp-demo");
        var attendNo = form.querySelector('input[name="RsvpAttending"][value="no"]');

        function togglePartyVisibility() {
            if (!partyFieldset || !attendYes || !attendNo) {
                return;
            }
            var show = attendYes.checked;
            partyFieldset.classList.toggle("is-hidden", !show);
        }

        function clearClientPartyErr() {
            if (clientErr) {
                clientErr.classList.remove("is-visible");
            }
            if (valBox) {
                var hasServer = valBox.querySelector("ul li");
                if (!hasServer) {
                    valBox.classList.remove("is-visible");
                }
            }
        }

        form.querySelectorAll('input[name="RsvpAttending"]').forEach(function (r) {
            r.addEventListener("change", function () {
                clearClientPartyErr();
                togglePartyVisibility();
            });
        });
        form.querySelectorAll('input[name="PartySlotIndexes"]').forEach(function (cb) {
            cb.addEventListener("change", clearClientPartyErr);
        });
        togglePartyVisibility();

        form.addEventListener("submit", function (e) {
            if (typeof form.reportValidity === "function" && !form.reportValidity()) {
                return;
            }
            clearClientPartyErr();
            var yes = attendYes && attendYes.checked;
            if (yes && partyFieldset && !partyFieldset.classList.contains("is-hidden")) {
                var any = form.querySelectorAll('input[name="PartySlotIndexes"]:checked').length > 0;
                if (!any) {
                    e.preventDefault();
                    if (valBox) {
                        valBox.classList.add("is-visible");
                    }
                    if (clientErr) {
                        clientErr.classList.add("is-visible");
                    }
                    return;
                }
            }
            if (isDemo) {
                e.preventDefault();
                root.classList.add("is-sent");
                ok.classList.add("is-visible");
                if (typeof window.gsap !== "undefined") {
                    window.gsap.fromTo(
                        ok,
                        { opacity: 0, y: 18, filter: "blur(8px)" },
                        { opacity: 1, y: 0, filter: "blur(0px)", duration: 1, ease: "power3.out" }
                    );
                }
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var main = window.CinematicSwiper && window.CinematicSwiper.initMain();
        var count = main && main.slides ? main.slides.length : 9;
        buildProgress(count);
        if (main) {
            updateProgress(main.activeIndex);
            main.on("slideChange", function () {
                updateProgress(main.activeIndex);
            });
            window.CinematicAnimations && window.CinematicAnimations.init(main);
            if (typeof window.gsap === "undefined") {
                document.querySelectorAll("[data-animate], [data-animate-child]").forEach(function (el) {
                    el.style.opacity = "1";
                    el.style.filter = "none";
                    el.style.transform = "none";
                });
            }
            bindChromeNav(main);
            window.__cinRevealActiveSlide = function () {
                window.CinematicAnimations && window.CinematicAnimations.revealActive(main);
            };
        }

        window.CinematicCountdown && window.CinematicCountdown.init(document.getElementById("cin-countdown-root"));
        window.CinematicAudio && window.CinematicAudio.init();
        initRsvp();
    });

    window.addEventListener("load", function () {
        window.setTimeout(hideLoader, 140);
    });
})();

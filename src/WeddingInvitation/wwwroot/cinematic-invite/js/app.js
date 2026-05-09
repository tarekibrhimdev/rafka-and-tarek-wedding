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

    function initExternalMapsOpen() {
        document.addEventListener(
            "click",
            function (e) {
                var a = e.target.closest("a[data-cin-external-maps]");
                if (!a || !a.href) {
                    return;
                }
                if (e.defaultPrevented) {
                    return;
                }
                if (e.ctrlKey || e.metaKey || e.shiftKey || e.altKey) {
                    return;
                }
                var btn = typeof e.button === "number" ? e.button : 0;
                if (btn !== 0) {
                    return;
                }
                e.preventDefault();
                var w = window.open(a.href, "_blank", "noopener,noreferrer");
                if (w) {
                    w.opener = null;
                }
            },
            false
        );
    }

    /**
     * After server RSVP redirect the page reloads; show thank-you on RSVP, then continue to Finale (gifts already seen).
     */
    function schedulePostRsvpAdvanceToFinale(swiper) {
        var root = document.getElementById("invite-root");
        if (!root || root.getAttribute("data-cin-after-rsvp-success") !== "true" || !swiper) {
            return;
        }
        var slides = swiper.el.querySelectorAll(".swiper-slide");
        var finaleIdx = -1;
        for (var i = 0; i < slides.length; i++) {
            if (slides[i].getAttribute("data-scene") === "finale") {
                finaleIdx = i;
                break;
            }
        }
        if (finaleIdx < 0) {
            return;
        }
        var reduced =
            typeof window.matchMedia === "function" &&
            window.matchMedia("(prefers-reduced-motion: reduce)").matches;
        var delayMs = reduced ? 500 : 1400;
        window.setTimeout(function () {
            swiper.slideTo(finaleIdx, reduced ? 0 : 1080, false);
        }, delayMs);
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
        var attendYes = form.querySelector('input[name="RsvpAttending"][value="yes"]');
        var attendNo = form.querySelector('input[name="RsvpAttending"][value="no"]');
        var nameInput = document.getElementById("cin-guest-name");

        function primaryPartyNameEl() {
            var cb = form.querySelector('input[name="PartySlotIndexes"][value="0"]');
            if (!cb) {
                return null;
            }
            var row = cb.closest(".cin-rsvp-party__row");
            return row ? row.querySelector(".cin-rsvp-party__name") : null;
        }

        function syncNameToPrimaryPartyRow(allowEmpty) {
            var span = primaryPartyNameEl();
            if (!nameInput || !span) {
                return;
            }
            var v = (nameInput.value || "").trim();
            if (v.length === 0 && !allowEmpty) {
                return;
            }
            span.textContent = v;
        }

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

        if (nameInput) {
            nameInput.addEventListener("input", function () {
                syncNameToPrimaryPartyRow(true);
            });
            nameInput.addEventListener("change", function () {
                syncNameToPrimaryPartyRow(true);
            });
            syncNameToPrimaryPartyRow(false);
        }

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
        initExternalMapsOpen();
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
            schedulePostRsvpAdvanceToFinale(main);
        }

        window.CinematicCountdown && window.CinematicCountdown.init(document.getElementById("cin-countdown-root"));
        window.CinematicAudio && window.CinematicAudio.init();
        initRsvp();
    });

    window.addEventListener("load", function () {
        window.setTimeout(hideLoader, 140);
    });
})();

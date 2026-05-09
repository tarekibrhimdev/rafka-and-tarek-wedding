/**
 * EN/AR copy from #invite-copy-json (same contract as legacy luxury invite).
 */
(function () {
    function ready(fn) {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", fn);
        } else {
            fn();
        }
    }

    ready(function () {
        var root = document.getElementById("invite-root");
        var jsonEl = document.getElementById("invite-copy-json");
        if (!root || !jsonEl) {
            return;
        }

        var copy = {};
        function refreshCopy() {
            try {
                var raw = (jsonEl.textContent || "").trim() || "{}";
                var parsed = JSON.parse(raw);
                if (parsed && typeof parsed === "object" && !Array.isArray(parsed)) {
                    copy = parsed;
                    if (!copy.ar && copy.AR && typeof copy.AR === "object") {
                        copy.ar = copy.AR;
                    }
                }
            } catch (e) {
                copy = {};
            }
        }
        refreshCopy();

        var langBtns = root.querySelectorAll(".cin-lang-toggle-opt[data-set-lang]");
        var lang = sessionStorage.getItem("inviteLang") || "en";
        if (lang !== "en" && lang !== "ar") {
            lang = "en";
        }

        function pick(dict, key) {
            return dict && Object.prototype.hasOwnProperty.call(dict, key) ? dict[key] : null;
        }

        function resolveCopy(key) {
            var pack = copy[lang] || {};
            var en = copy.en || {};
            var v = pick(pack, key);
            if (v !== null) {
                return v;
            }
            v = pick(en, key);
            return v !== null ? v : "";
        }

        function applyLang(nextLang) {
            lang = nextLang;
            refreshCopy();
            sessionStorage.setItem("inviteLang", lang);
            document.documentElement.lang = lang === "ar" ? "ar" : "en";
            /* Keep html LTR so Swiper + footer arrows stay spatially correct; Arabic flow is scoped in CSS */
            document.documentElement.dir = "ltr";
            root.classList.toggle("lang-ar", lang === "ar");
            root.classList.toggle("lang-en", lang !== "ar");

            langBtns.forEach(function (b) {
                var isAct = b.getAttribute("data-set-lang") === lang;
                b.classList.toggle("is-active", isAct);
                b.setAttribute("aria-pressed", isAct ? "true" : "false");
            });

            root.querySelectorAll("[data-i18n-key]").forEach(function (el) {
                if (el.hasAttribute("data-cap") || el.hasAttribute("data-fmt-arg")) {
                    return;
                }
                var k = el.getAttribute("data-i18n-key");
                el.textContent = resolveCopy(k);
            });

            root.querySelectorAll("[data-i18n-key][data-cap]").forEach(function (el) {
                var k = el.getAttribute("data-i18n-key");
                var cap = el.getAttribute("data-cap") || "";
                var tmpl = resolveCopy(k);
                el.textContent = tmpl.split("{0}").join(cap);
            });

            root.querySelectorAll("[data-i18n-key][data-fmt-arg]").forEach(function (el) {
                var k = el.getAttribute("data-i18n-key");
                var arg = el.getAttribute("data-fmt-arg") || "";
                var tmpl = resolveCopy(k);
                el.textContent = tmpl.split("{0}").join(arg);
            });

            root.querySelectorAll("[data-i18n-placeholder]").forEach(function (el) {
                var k = el.getAttribute("data-i18n-placeholder");
                el.setAttribute("placeholder", resolveCopy(k));
            });

            if (typeof window.__cinRevealActiveSlide === "function") {
                window.__cinRevealActiveSlide();
            }
        }

        langBtns.forEach(function (b) {
            b.addEventListener("click", function () {
                applyLang(b.getAttribute("data-set-lang") || "en");
            });
        });

        applyLang(lang);
        requestAnimationFrame(function () {
            applyLang(lang);
        });
        window.addEventListener("load", function () {
            applyLang(lang);
        });
        window.__applyInviteLang = applyLang;
    });
})();

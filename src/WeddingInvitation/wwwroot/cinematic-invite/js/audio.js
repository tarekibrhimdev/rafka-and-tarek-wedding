/**
 * Optional ambient bed — loads after guest taps sound. CIN_BASE is set on /invite/* for correct paths.
 */
(function () {
    var AUDIO_REL = "music/ambient.mp3";

    function audioUrl() {
        var base = typeof window.CIN_BASE === "string" ? window.CIN_BASE.trim() : "";
        if (base) {
            return base.replace(/\/+$/, "") + "/" + AUDIO_REL;
        }
        return AUDIO_REL;
    }

    function hideAudioUi(btn) {
        if (!btn) {
            return;
        }
        btn.style.visibility = "hidden";
        btn.setAttribute("aria-hidden", "true");
        btn.tabIndex = -1;
    }

    window.CinematicAudio = {
        init: function () {
            var btn = document.getElementById("cin-audio-btn");
            var audio = document.getElementById("cin-audio-el");
            if (!btn || !audio) {
                return;
            }

            audio.volume = 0.32;
            audio.muted = true;
            audio.preload = "none";

            var syncUi = function () {
                btn.setAttribute("aria-pressed", audio.muted ? "true" : "false");
            };

            syncUi();

            audio.addEventListener(
                "error",
                function () {
                    hideAudioUi(btn);
                },
                { once: true }
            );

            btn.addEventListener("click", function () {
                if (!audio.src) {
                    audio.src = audioUrl();
                    audio.load();
                }
                if (audio.muted) {
                    audio.muted = false;
                    audio.play().catch(function () {
                        hideAudioUi(btn);
                    });
                } else {
                    audio.muted = true;
                }
                syncUi();
            });
        }
    };
})();

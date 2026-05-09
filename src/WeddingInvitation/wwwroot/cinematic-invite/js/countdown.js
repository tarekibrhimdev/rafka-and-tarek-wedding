/**
 * Live countdown to ceremony — edit target for your wedding.
 */
(function () {
    var DEFAULT_TARGET = new Date("2026-06-27T18:00:00");

    function pad(n) {
        return String(Math.max(0, n)).padStart(2, "0");
    }

    function tick(root, target) {
        var now = new Date();
        var diff = target.getTime() - now.getTime();
        if (diff < 0) {
            diff = 0;
        }
        var s = Math.floor(diff / 1000);
        var days = Math.floor(s / 86400);
        var hours = Math.floor((s % 86400) / 3600);
        var minutes = Math.floor((s % 3600) / 60);
        var seconds = s % 60;

        var daysEl = root.querySelector('[data-cd="days"]');
        var hoursEl = root.querySelector('[data-cd="hours"]');
        var minEl = root.querySelector('[data-cd="minutes"]');
        var secEl = root.querySelector('[data-cd="seconds"]');
        if (daysEl) {
            daysEl.textContent = String(days);
        }
        if (hoursEl) {
            hoursEl.textContent = pad(hours);
        }
        if (minEl) {
            minEl.textContent = pad(minutes);
        }
        if (secEl) {
            secEl.textContent = pad(seconds);
        }
    }

    window.CinematicCountdown = {
        /**
         * @param {HTMLElement|null} root
         * @param {Date} [targetDate]
         */
        init: function (root, targetDate) {
            if (!root) {
                return;
            }
            var target = targetDate instanceof Date ? targetDate : DEFAULT_TARGET;
            tick(root, target);
            setInterval(function () {
                tick(root, target);
            }, 1000);
        }
    };
})();

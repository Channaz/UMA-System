if (!Array.prototype.forEach) {
    Array.prototype.forEach = function (cb) {
        var i = 0, s = this.length;
        for (; i < s; i++) {
            cb && cb(this[i], i, this);
        }
    };
}

document.fonts && document.fonts.forEach(function (font) {
    font.loaded.then(function () {
        if (font.family.match(/Led/)) {
            document.gauges.forEach(function (gauge) {
                gauge.update();
            });
        }
    });
});

var timers = [];

function animateGauges() {
    document.gauges.forEach(function (gauge) {
        timers.push(setInterval(function () {
            var min = gauge.options.minValue - 20;
            var max = gauge.options.maxValue + 20;

            gauge.value = min + Math.random() * (max - min);
        }, gauge.animation.duration + 50));
    });
}

function stopGaugesAnimation() {
    timers.forEach(function (timer) {
        clearInterval(timer);
    });
    timers = [];
}
